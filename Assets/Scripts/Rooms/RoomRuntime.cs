using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns instantiated rooms and all movement between them for one dungeon run.
/// </summary>
public sealed class RoomRuntime : IRoomRuntimeContext
{
    private readonly DungeonLayout layout;
    private readonly Camera gameCamera;
    private readonly RoomController roomPrefab;
    private readonly RoomPrefabCatalog roomPrefabCatalog;
    private readonly Transform roomRoot;
    private readonly Vector2 roomWorldSpacing;
    private readonly Func<RoomNode, bool> nextFloorHandler;
    private readonly Dictionary<RoomCoordinate, RoomController> roomInstances =
        new Dictionary<RoomCoordinate, RoomController>();

    public Player Player { get; }
    public DungeonLayout Layout => layout;
    public IReadOnlyDictionary<RoomCoordinate, RoomController> RoomInstances => roomInstances;
    public int ActiveSeed { get; }
    public RoomController CurrentRoom { get; private set; }
    public RoomDirection? CurrentEntranceDirection { get; private set; }
    public RoomPrefabCatalog PrefabCatalog => roomPrefabCatalog;

    public event Action<RoomNode> RoomChanged;

    public RoomRuntime(
        Player player,
        DungeonLayout dungeonLayout,
        Camera camera,
        RoomController dungeonRoomPrefab,
        Transform roomsRoot,
        Vector2 worldSpacing,
        int activeSeed,
        Func<RoomNode, bool> floorTransitionHandler = null)
        : this(
            player,
            dungeonLayout,
            camera,
            dungeonRoomPrefab,
            null,
            roomsRoot,
            worldSpacing,
            activeSeed,
            floorTransitionHandler)
    {
    }

    public RoomRuntime(
        Player player,
        DungeonLayout dungeonLayout,
        Camera camera,
        RoomController dungeonRoomPrefab,
        RoomPrefabCatalog prefabCatalog,
        Transform roomsRoot,
        Vector2 worldSpacing,
        int activeSeed,
        Func<RoomNode, bool> floorTransitionHandler = null)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        layout = dungeonLayout ?? throw new ArgumentNullException(nameof(dungeonLayout));
        gameCamera = camera ?? throw new ArgumentNullException(nameof(camera));
        roomPrefab = dungeonRoomPrefab ?? throw new ArgumentNullException(nameof(dungeonRoomPrefab));
        roomPrefabCatalog = prefabCatalog;
        roomRoot = roomsRoot ?? throw new ArgumentNullException(nameof(roomsRoot));
        roomWorldSpacing = worldSpacing;
        ActiveSeed = activeSeed;
        nextFloorHandler = floorTransitionHandler;
    }

    public void BuildRooms()
    {
        foreach (RoomNode node in layout.Rooms.Values)
        {
            RoomController selectedPrefab = SelectRoomPrefab(node);
            RoomController room = UnityEngine.Object.Instantiate(selectedPrefab, roomRoot);
            room.transform.localPosition = new Vector3(
                node.Coordinate.X * roomWorldSpacing.x,
                node.Coordinate.Y * roomWorldSpacing.y,
                0f);
            room.Initialize(this, node);
            room.gameObject.SetActive(false);
            roomInstances.Add(node.Coordinate, room);
        }
    }

    private RoomController SelectRoomPrefab(RoomNode node)
    {
        if (roomPrefabCatalog == null) return roomPrefab;

        if (roomPrefabCatalog.TryResolve(node.Type, node.RoomVariantId, out RoomPrefabEntry restoredEntry))
        {
            return restoredEntry.Prefab;
        }

        int selectionKey = CalculateRoomSelectionKey(ActiveSeed, node);
        if (!roomPrefabCatalog.TrySelect(node.Type, selectionKey, out RoomPrefabEntry selectedEntry))
        {
            return roomPrefab;
        }

        node.AssignRoomVariant(selectedEntry.StableId);
        return selectedEntry.Prefab;
    }

    public void EnterInitialRoom(RoomCoordinate coordinate, RoomDirection? savedEntranceDirection)
    {
        if (!roomInstances.TryGetValue(coordinate, out RoomController entryRoom) ||
            (RoomTypeUtility.IsHiddenRoom(entryRoom.Node.Type) && entryRoom.Node.Connections.Count == 0))
        {
            entryRoom = roomInstances[layout.StartCoordinate];
            savedEntranceDirection = null;
        }

        CurrentRoom = entryRoom;
        CurrentEntranceDirection = IsValidEntrance(entryRoom.Node, savedEntranceDirection)
            ? savedEntranceDirection
            : null;
        CurrentRoom.gameObject.SetActive(true);
        Player.transform.position = CurrentEntranceDirection.HasValue
            ? CurrentRoom.GetArrivalPosition(CurrentEntranceDirection.Value)
            : CurrentRoom.transform.position;
        MoveCamera(CurrentRoom.transform.position);
        CurrentRoom.Enter();
        RoomChanged?.Invoke(CurrentRoom.Node);
    }

    public bool TryTransition(RoomController sourceRoom, RoomDirection direction)
    {
        if (sourceRoom == null || sourceRoom != CurrentRoom) return false;
        if (!layout.TryGetConnectedRoom(sourceRoom.Node, direction, out RoomNode destinationNode)) return false;
        if (!roomInstances.TryGetValue(destinationNode.Coordinate, out RoomController destinationRoom)) return false;

        destinationRoom.gameObject.SetActive(true);
        RoomDirection entrance = RoomDirectionUtility.Opposite(direction);
        Player.transform.position = destinationRoom.GetArrivalPosition(entrance);
        MoveCamera(destinationRoom.transform.position);

        sourceRoom.Exit();
        sourceRoom.gameObject.SetActive(false);
        CurrentRoom = destinationRoom;
        CurrentEntranceDirection = entrance;
        destinationRoom.Enter();
        RoomChanged?.Invoke(destinationNode);
        return true;
    }

    public bool IsSecretPassageCandidate(RoomNode room, RoomDirection direction)
    {
        return layout.IsSecretPassageCandidate(room, direction);
    }

    public bool TryOpenSecretPassage(RoomController sourceRoom, RoomDirection direction)
    {
        if (sourceRoom == null || sourceRoom != CurrentRoom ||
            !layout.IsSecretPassageCandidate(sourceRoom.Node, direction))
        {
            return false;
        }

        RoomCoordinate destinationCoordinate = sourceRoom.Node.Coordinate.Offset(direction);
        if (!roomInstances.TryGetValue(destinationCoordinate, out RoomController destinationRoom) ||
            !layout.TryOpenSecretPassage(sourceRoom.Node, direction))
        {
            return false;
        }

        sourceRoom.SetConnectionAvailable(direction, true);
        destinationRoom.SetConnectionAvailable(RoomDirectionUtility.Opposite(direction), true);
        RoomChanged?.Invoke(CurrentRoom.Node);
        return true;
    }

    public bool TryAdvanceToNextFloor(RoomNode sourceRoom)
    {
        return sourceRoom != null && CurrentRoom != null && CurrentRoom.Node == sourceRoom &&
               nextFloorHandler != null && nextFloorHandler(sourceRoom);
    }

    private void MoveCamera(Vector3 roomPosition)
    {
        gameCamera.transform.position = new Vector3(roomPosition.x, roomPosition.y, -10f);
    }

    private static bool IsValidEntrance(RoomNode room, RoomDirection? direction)
    {
        return room != null && direction.HasValue && room.HasConnection(direction.Value);
    }

    private static int CalculateRoomSelectionKey(int dungeonSeed, RoomNode room)
    {
        unchecked
        {
            int hash = dungeonSeed;
            hash = hash * 397 ^ room.Coordinate.X;
            hash = hash * 397 ^ room.Coordinate.Y;
            hash = hash * 397 ^ (int)room.Type;
            return hash;
        }
    }
}
