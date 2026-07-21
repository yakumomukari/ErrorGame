using System;
using System.Collections.Generic;

/// <summary>
/// Owns the runtime-to-save-data mapping and the autosave event lifecycle.
/// GameSession only decides when a run starts, changes room, or shuts down.
/// </summary>
public sealed class GamePersistenceCoordinator : IDisposable
{
    private readonly Player player;
    private readonly DungeonLayout layout;
    private readonly Func<RoomNode> currentRoomProvider;
    private readonly Func<RoomDirection?> currentEntranceProvider;
    private readonly IGameSaveRepository saveRepository;
    private readonly int dungeonSeed;
    private readonly int floorNumber;

    private bool ready;
    private bool subscribed;

    public bool IsReady => ready;
    public bool IsSubscribed => subscribed;
    public IGameSaveRepository Repository => saveRepository;

    public GamePersistenceCoordinator(
        Player playerReference,
        DungeonLayout dungeonLayout,
        Func<RoomNode> roomProvider,
        Func<RoomDirection?> entranceProvider,
        IGameSaveRepository repository,
        int activeDungeonSeed,
        int activeFloorNumber)
    {
        player = playerReference ?? throw new ArgumentNullException(nameof(playerReference));
        layout = dungeonLayout ?? throw new ArgumentNullException(nameof(dungeonLayout));
        currentRoomProvider = roomProvider ?? throw new ArgumentNullException(nameof(roomProvider));
        currentEntranceProvider = entranceProvider ?? throw new ArgumentNullException(nameof(entranceProvider));
        saveRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        dungeonSeed = activeDungeonSeed;
        floorNumber = Math.Max(1, activeFloorNumber);
    }

    public void RestoreLayout(GameSaveData saveData)
    {
        if (saveData == null) return;

        foreach (RoomSaveData roomData in saveData.rooms)
        {
            if (!layout.TryGetRoom(new RoomCoordinate(roomData.x, roomData.y), out RoomNode node)) continue;
            node.RestoreState(
                roomData.visited,
                roomData.cleared,
                roomData.itemClaimed,
                roomData.purchasedShopSlots,
                roomData.combatRewardType,
                roomData.combatRewardCollected,
                roomData.collectedSecretRewards,
                roomData.roomVariantId);
        }

        foreach (SecretPassageSaveData passage in saveData.openedSecretPassages)
        {
            if (passage.direction < 0 || passage.direction >= RoomDirectionUtility.All.Count) continue;
            if (!layout.TryGetRoom(new RoomCoordinate(passage.sourceX, passage.sourceY), out RoomNode source)) continue;
            layout.TryOpenSecretPassage(source, (RoomDirection)passage.direction);
        }
    }

    public void RestorePlayer(PlayerSaveData playerData)
    {
        if (playerData == null) return;

        player.Health.Restore(playerData.currentHealthUnits, playerData.maxHealthUnits);
        player.Inventory.Restore(playerData.coins, playerData.bombs);
        player.Stats.Restore(
            playerData.moveSpeed,
            playerData.fireRate,
            playerData.damage,
            playerData.range,
            playerData.projectileSpeed,
            playerData.luck);
    }

    public void BeginAutosave()
    {
        if (subscribed) return;

        player.Health.HealthChanged += OnHealthChanged;
        player.Health.Died += OnPlayerDied;
        player.Inventory.ResourcesChanged += Save;
        player.Stats.Changed += Save;
        foreach (RoomNode node in layout.Rooms.Values) node.StateChanged += Save;

        subscribed = true;
        ready = true;
    }

    public void Save()
    {
        RoomNode currentRoom = currentRoomProvider();
        if (!ready || currentRoom == null || player.Health.IsDead) return;
        RoomDirection? currentEntrance = currentEntranceProvider();

        GameSaveData saveData = new GameSaveData
        {
            dungeonSeed = dungeonSeed,
            floorNumber = floorNumber,
            beginsAtFloorStart = false,
            currentRoomX = currentRoom.Coordinate.X,
            currentRoomY = currentRoom.Coordinate.Y,
            hasCurrentRoomEntrance = currentEntrance.HasValue,
            currentRoomEntranceDirection = currentEntrance.HasValue ? (int)currentEntrance.Value : 0,
            player = CapturePlayer()
        };

        foreach (RoomNode node in layout.Rooms.Values)
        {
            saveData.rooms.Add(new RoomSaveData
            {
                x = node.Coordinate.X,
                y = node.Coordinate.Y,
                visited = node.IsVisited,
                cleared = node.IsCleared,
                itemClaimed = node.IsItemClaimed,
                purchasedShopSlots = new List<int>(node.PurchasedShopSlots),
                combatRewardType = node.CombatRewardType,
                combatRewardCollected = node.IsCombatRewardCollected,
                collectedSecretRewards = new List<int>(node.CollectedSecretRewards),
                roomVariantId = node.RoomVariantId
            });

            AddOpenedHiddenPassages(saveData, node);
        }

        saveRepository.Save(saveData);
    }

    public bool SaveNextFloor(int nextDungeonSeed, int nextFloorNumber)
    {
        if (!ready || player.Health.IsDead) return false;

        saveRepository.Save(new GameSaveData
        {
            dungeonSeed = nextDungeonSeed,
            floorNumber = Math.Max(1, nextFloorNumber),
            beginsAtFloorStart = true,
            currentRoomX = 0,
            currentRoomY = 0,
            hasCurrentRoomEntrance = false,
            player = CapturePlayer()
        });

        // Prevent OnDestroy and autosave callbacks from overwriting the pending
        // next-floor snapshot with the completed floor that is being unloaded.
        ready = false;
        return true;
    }

    public void Dispose()
    {
        if (!subscribed) return;

        player.Health.HealthChanged -= OnHealthChanged;
        player.Health.Died -= OnPlayerDied;
        player.Inventory.ResourcesChanged -= Save;
        player.Stats.Changed -= Save;
        foreach (RoomNode node in layout.Rooms.Values) node.StateChanged -= Save;

        subscribed = false;
        ready = false;
    }

    private void AddOpenedHiddenPassages(GameSaveData saveData, RoomNode node)
    {
        if (RoomTypeUtility.IsHiddenRoom(node.Type)) return;

        foreach (RoomDirection direction in node.Connections)
        {
            if (layout.TryGetConnectedRoom(node, direction, out RoomNode neighbor) &&
                RoomTypeUtility.IsHiddenRoom(neighbor.Type))
            {
                saveData.openedSecretPassages.Add(new SecretPassageSaveData
                {
                    sourceX = node.Coordinate.X,
                    sourceY = node.Coordinate.Y,
                    direction = (int)direction
                });
            }
        }
    }

    private PlayerSaveData CapturePlayer()
    {
        return new PlayerSaveData
        {
            currentHealthUnits = player.Health.CurrentHealthUnits,
            maxHealthUnits = player.Health.MaxHealthUnits,
            coins = player.Inventory.Coins,
            bombs = player.Inventory.Bombs,
            moveSpeed = player.Stats.MoveSpeed,
            fireRate = player.Stats.FireRate,
            damage = player.Stats.Damage,
            range = player.Stats.Range,
            projectileSpeed = player.Stats.ProjectileSpeed,
            luck = player.Stats.Luck
        };
    }

    private void OnHealthChanged(int current, int maximum)
    {
        Save();
    }

    private void OnPlayerDied()
    {
        ready = false;
        saveRepository.Delete();
    }
}
