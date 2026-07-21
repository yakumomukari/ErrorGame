using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RoomController : MonoBehaviour
{
    [SerializeField] private DoorController[] doors;
    [SerializeField] private MonoBehaviour[] roomFeatureBehaviours;
    [SerializeField] private SpriteRenderer typeMarker;

    private readonly List<IRoomFeature> roomFeatures = new List<IRoomFeature>();
    private readonly List<IRoomLockSource> lockSources = new List<IRoomLockSource>();
    private IRoomRuntimeContext roomRuntime;

    public RoomNode Node { get; private set; }
    public bool IsCurrentRoom { get; private set; }

    public void Configure(
        DoorController[] roomDoors,
        MonoBehaviour[] featureBehaviours,
        SpriteRenderer roomTypeMarker)
    {
        doors = roomDoors;
        roomFeatureBehaviours = featureBehaviours;
        typeMarker = roomTypeMarker;
    }

    public void Initialize(IRoomRuntimeContext runtimeContext, RoomNode roomNode)
    {
        roomRuntime = runtimeContext;
        Node = roomNode;
        gameObject.name = $"{roomNode.Type} Room {roomNode.Coordinate}";

        foreach (DoorController door in doors)
        {
            door.Bind(
                this,
                roomNode.HasConnection(door.Direction),
                roomRuntime.IsSecretPassageCandidate(roomNode, door.Direction));
        }

        InitializeRoomFeatures();
        ApplyRoomTypeColor();
    }

    public void Enter()
    {
        IsCurrentRoom = true;
        Node.MarkVisited();

        SetDoorsLocked(RoomShouldBeLocked());
        foreach (IRoomFeature feature in roomFeatures) feature.OnRoomEntered();
    }

    public void Exit()
    {
        IsCurrentRoom = false;
    }

    public void TryUseDoor(RoomDirection direction, Player player)
    {
        if (!IsCurrentRoom || player != roomRuntime.Player) return;
        roomRuntime.TryTransition(this, direction);
    }

    public bool TryOpenSecretPassage(RoomDirection direction)
    {
        return IsCurrentRoom && roomRuntime.TryOpenSecretPassage(this, direction);
    }

    public void SetConnectionAvailable(RoomDirection direction, bool available)
    {
        foreach (DoorController door in doors)
        {
            if (door.Direction == direction)
            {
                door.SetConnectionAvailable(available);
                return;
            }
        }
    }

    public Vector3 GetArrivalPosition(RoomDirection entranceDirection)
    {
        const float inset = 4.75f;
        switch (entranceDirection)
        {
            case RoomDirection.North: return transform.position + new Vector3(0f, inset, 0f);
            case RoomDirection.South: return transform.position + new Vector3(0f, -inset, 0f);
            case RoomDirection.West: return transform.position + new Vector3(-inset * 1.75f, 0f, 0f);
            case RoomDirection.East: return transform.position + new Vector3(inset * 1.75f, 0f, 0f);
            default: return transform.position;
        }
    }

    private void OnEncounterCleared()
    {
        Node.MarkCleared();
        SetDoorsLocked(false);
    }

    private void SetDoorsLocked(bool locked)
    {
        foreach (DoorController door in doors) door.SetLocked(locked);
    }

    private void InitializeRoomFeatures()
    {
        roomFeatures.Clear();
        lockSources.Clear();
        HashSet<IRoomFeature> initializedFeatures = new HashSet<IRoomFeature>();

        if (roomFeatureBehaviours != null)
        {
            foreach (MonoBehaviour behaviour in roomFeatureBehaviours)
            {
                if (!(behaviour is IRoomFeature))
                {
                    if (behaviour != null)
                    {
                        Debug.LogError($"{behaviour.name} does not implement IRoomFeature.", behaviour);
                    }
                    continue;
                }

                InitializeRoomFeature(behaviour, initializedFeatures);
            }
        }

        foreach (MonoBehaviour behaviour in GetComponentsInChildren<MonoBehaviour>(true))
        {
            InitializeRoomFeature(behaviour, initializedFeatures);
        }
    }

    private void InitializeRoomFeature(
        MonoBehaviour behaviour,
        HashSet<IRoomFeature> initializedFeatures)
    {
        if (!(behaviour is IRoomFeature feature) || !initializedFeatures.Add(feature)) return;

        roomFeatures.Add(feature);
        feature.Initialize(roomRuntime, Node);
        if (feature is IRoomLockSource lockSource)
        {
            lockSources.Add(lockSource);
            lockSource.Cleared += OnEncounterCleared;
        }
    }

    private bool RoomShouldBeLocked()
    {
        foreach (IRoomLockSource lockSource in lockSources)
        {
            if (lockSource.LocksRoom) return true;
        }
        return false;
    }

    private void ApplyRoomTypeColor()
    {
        if (typeMarker == null) return;
        typeMarker.color = RoomPresentationCatalog.Get(Node.Type).RoomColor;
    }

    private void OnDestroy()
    {
        foreach (IRoomLockSource lockSource in lockSources)
        {
            lockSource.Cleared -= OnEncounterCleared;
        }
    }
}
