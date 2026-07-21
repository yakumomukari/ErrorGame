using System;
using UnityEngine;

public sealed class GameSession : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Player player;
    [SerializeField] private StageAHudController hud;
    [SerializeField] private MinimapController minimap;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private RoomController roomPrefab;
    [SerializeField] private RoomPrefabCatalog roomPrefabCatalog;
    [SerializeField] private Transform roomRoot;

    [Header("Dungeon Generation")]
    [SerializeField] private bool useFixedSeed;
    [SerializeField] private int fixedDungeonSeed = 20260720;
    [SerializeField, Min(8)] private int minimumVisibleRooms = 8;
    [SerializeField, Min(8)] private int maximumVisibleRooms = 12;
    [SerializeField] private Vector2 roomWorldSpacing = new Vector2(28f, 20f);

    private RoomRuntime roomRuntime;
    private GamePersistenceCoordinator persistence;
    private IGameSaveRepository saveRepository;
    private ISceneNavigator sceneNavigator;
    private bool floorTransitionInProgress;

    public Player Player => player;
    public DungeonLayout Layout { get; private set; }
    public RoomController CurrentRoom => roomRuntime != null ? roomRuntime.CurrentRoom : null;
    public RoomRuntime Rooms => roomRuntime;
    public RoomPrefabCatalog RoomPrefabs => roomPrefabCatalog;
    public GamePersistenceCoordinator Persistence => persistence;
    public IGameSaveRepository SaveRepository => saveRepository;
    public int ActiveSeed { get; private set; }
    public int CurrentFloor { get; private set; } = 1;

    public event Action<RoomNode> RoomChanged;

    private void Start()
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("GameSession is missing a Player, HUD, minimap, Camera, room prefab, or room root reference.", this);
            enabled = false;
            return;
        }

        Time.timeScale = 1f;
        sceneNavigator = new UnitySceneNavigator();
        saveRepository = new JsonGameSaveRepository();
        bool continueRequested = GameLaunchContext.ConsumeContinueRequest();
        GameSaveData saveData = null;
        bool loaded = continueRequested && saveRepository.TryLoad(out saveData);
        if (continueRequested && !loaded) saveRepository.Delete();

        CurrentFloor = loaded ? Mathf.Max(1, saveData.floorNumber) : 1;
        ActiveSeed = loaded ? saveData.dungeonSeed : useFixedSeed ? fixedDungeonSeed : CreateRandomSeed();
        Layout = new DungeonGenerator().Generate(ActiveSeed, minimumVisibleRooms, maximumVisibleRooms);
        roomRuntime = new RoomRuntime(
            player,
            Layout,
            gameCamera,
            roomPrefab,
            roomPrefabCatalog,
            roomRoot,
            roomWorldSpacing,
            ActiveSeed,
            TryAdvanceToNextFloor);
        roomRuntime.RoomChanged += OnRuntimeRoomChanged;
        persistence = new GamePersistenceCoordinator(
            player,
            Layout,
            () => CurrentRoom != null ? CurrentRoom.Node : null,
            () => roomRuntime != null ? roomRuntime.CurrentEntranceDirection : null,
            saveRepository,
            ActiveSeed,
            CurrentFloor);
        if (loaded) persistence.RestoreLayout(saveData);
        roomRuntime.BuildRooms();
        if (loaded) persistence.RestorePlayer(saveData.player);
        hud.Bind(player);
        persistence.BeginAutosave();

        bool enterAtFloorStart = loaded && saveData.beginsAtFloorStart;
        RoomCoordinate entryCoordinate = loaded && !enterAtFloorStart
            ? new RoomCoordinate(saveData.currentRoomX, saveData.currentRoomY)
            : Layout.StartCoordinate;
        RoomDirection? entryDirection = loaded && !enterAtFloorStart && saveData.hasCurrentRoomEntrance &&
                                        saveData.currentRoomEntranceDirection >= 0 &&
                                        saveData.currentRoomEntranceDirection < RoomDirectionUtility.All.Count
            ? (RoomDirection?)saveData.currentRoomEntranceDirection
            : null;
        roomRuntime.EnterInitialRoom(entryCoordinate, entryDirection);
        // Bind after the initial room is active and visited. This avoids drawing
        // an empty map during the same-frame scene reload used between floors.
        minimap.Bind(this);
        persistence.Save();
        Debug.Log(loaded
            ? $"Continued floor {CurrentFloor} with seed {ActiveSeed}."
            : $"Started floor {CurrentFloor} with seed {ActiveSeed}.", this);
    }

    public void Configure(
        Player playerReference,
        StageAHudController hudReference,
        MinimapController minimapReference,
        Camera cameraReference,
        RoomController dungeonRoomPrefab,
        Transform roomsRoot,
        bool fixedSeedEnabled,
        int fixedSeed)
    {
        Configure(
            playerReference,
            hudReference,
            minimapReference,
            cameraReference,
            dungeonRoomPrefab,
            null,
            roomsRoot,
            fixedSeedEnabled,
            fixedSeed);
    }

    public void Configure(
        Player playerReference,
        StageAHudController hudReference,
        MinimapController minimapReference,
        Camera cameraReference,
        RoomController dungeonRoomPrefab,
        RoomPrefabCatalog prefabCatalog,
        Transform roomsRoot,
        bool fixedSeedEnabled,
        int fixedSeed)
    {
        player = playerReference;
        hud = hudReference;
        minimap = minimapReference;
        gameCamera = cameraReference;
        roomPrefab = dungeonRoomPrefab;
        roomPrefabCatalog = prefabCatalog;
        roomRoot = roomsRoot;
        useFixedSeed = fixedSeedEnabled;
        fixedDungeonSeed = fixedSeed;
    }

    public bool TryTransition(RoomController sourceRoom, RoomDirection direction)
    {
        return roomRuntime != null && roomRuntime.TryTransition(sourceRoom, direction);
    }

    public bool IsSecretPassageCandidate(RoomNode room, RoomDirection direction)
    {
        return roomRuntime != null && roomRuntime.IsSecretPassageCandidate(room, direction);
    }

    public bool TryOpenSecretPassage(RoomController sourceRoom, RoomDirection direction)
    {
        return roomRuntime != null && roomRuntime.TryOpenSecretPassage(sourceRoom, direction);
    }

    public bool TryAdvanceToNextFloor(RoomNode sourceRoom)
    {
        if (floorTransitionInProgress || sourceRoom == null || sourceRoom.Type != RoomType.Boss ||
            !sourceRoom.IsCleared || CurrentRoom == null || CurrentRoom.Node != sourceRoom ||
            persistence == null)
        {
            return false;
        }

        int nextFloor = CurrentFloor + 1;
        int nextSeed = CreateNextFloorSeed(ActiveSeed, nextFloor);
        if (!persistence.SaveNextFloor(nextSeed, nextFloor)) return false;

        floorTransitionInProgress = true;
        GameLaunchContext.RequestContinue();
        sceneNavigator.LoadGame();
        return true;
    }

    private bool HasRequiredReferences()
    {
        return player != null && hud != null && minimap != null && gameCamera != null && roomPrefab != null && roomRoot != null;
    }

    private static int CreateRandomSeed()
    {
        return Guid.NewGuid().GetHashCode();
    }

    private static int CreateNextFloorSeed(int currentSeed, int nextFloor)
    {
        unchecked
        {
            int nextSeed = currentSeed * 486187739 ^ nextFloor * 16777619 ^ 0x4E58544C;
            return nextSeed != currentSeed ? nextSeed : nextSeed ^ int.MinValue;
        }
    }

    private void OnRuntimeRoomChanged(RoomNode room)
    {
        RoomChanged?.Invoke(room);
        persistence?.Save();
    }

    private void OnApplicationQuit()
    {
        persistence?.Save();
    }

    private void OnDestroy()
    {
        persistence?.Save();
        persistence?.Dispose();
        if (roomRuntime != null) roomRuntime.RoomChanged -= OnRuntimeRoomChanged;
    }
}
