using System.Collections.Generic;
using System.IO;
using System.Linq;
using InvalidOperationException = System.InvalidOperationException;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class StageAProjectBuilder
{
    private const string GeneratedArtFolder = "Assets/Art/Generated";
    private const string PrefabPlayerFolder = "Assets/Prefabs/Player";
    private const string PrefabCombatFolder = "Assets/Prefabs/Combat";
    private const string PrefabEnemyFolder = "Assets/Prefabs/Enemies";
    private const string PrefabRoomFolder = "Assets/Prefabs/Rooms";
    private const string PrefabDoorFolder = "Assets/Prefabs/Doors";
    private const string PrefabPickupFolder = "Assets/Prefabs/Pickups";
    private const string PrefabUiFolder = "Assets/Prefabs/UI";
    private const string WhiteSpritePath = GeneratedArtFolder + "/StageAWhite.asset";
    private const string HeartSpritePath = GeneratedArtFolder + "/StageAHeart.asset";
    private const string CircleSpritePath = GeneratedArtFolder + "/StageACircle.asset";
    private const string ProjectilePrefabPath = PrefabCombatFolder + "/PlayerProjectile.prefab";
    private const string EnemyProjectilePrefabPath = PrefabCombatFolder + "/EnemyProjectile.prefab";
    private const string BombPrefabPath = PrefabCombatFolder + "/PlayerBomb.prefab";
    private const string PlayerPrefabPath = PrefabPlayerFolder + "/Player.prefab";
    private const string EnemyPrefabPath = PrefabEnemyFolder + "/MeleeEnemy.prefab";
    private const string RangedEnemyPrefabPath = PrefabEnemyFolder + "/RangedEnemy.prefab";
    private const string BossPrefabPath = PrefabEnemyFolder + "/BossEnemy.prefab";
    private const string DoorPrefabPath = PrefabDoorFolder + "/CombatDoor.prefab";
    private const string DungeonRoomPrefabPath = PrefabRoomFolder + "/DungeonRoom.prefab";
    private const string CoinPickupPrefabPath = PrefabPickupFolder + "/CoinPickup.prefab";
    private const string HeartPickupPrefabPath = PrefabPickupFolder + "/HeartPickup.prefab";
    private const string BombPickupPrefabPath = PrefabPickupFolder + "/BombPickup.prefab";
    private const string NormalUpgradePrefabPath = PrefabPickupFolder + "/NormalUpgradePickup.prefab";
    private const string SuperMushroomPrefabPath = PrefabPickupFolder + "/SuperMushroom.prefab";
    private const string ShopProductPrefabPath = PrefabPickupFolder + "/ShopProduct.prefab";
    private const string HudPrefabPath = PrefabUiFolder + "/StageAHud.prefab";
    private const string InputActionsPath = "Assets/Scripts/InputSystem.inputactions";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/Game.unity";

    static StageAProjectBuilder()
    {
        EditorApplication.delayCall += BuildIfMissing;
    }

    [MenuItem("Tools/Error Game/Rebuild Final Demo Resources")]
    public static void RebuildFromMenu()
    {
        BuildProject(true);
    }

    public static void BuildFromCommandLine()
    {
        BuildProject(false);
    }

    private static void BuildIfMissing()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += BuildIfMissing;
            return;
        }

        bool missing = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(EnemyProjectilePrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(BombPrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(RangedEnemyPrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(DungeonRoomPrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(NormalUpgradePrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(SuperMushroomPrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(ShopProductPrefabPath) == null ||
                       AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath) == null ||
                       !File.Exists(MainMenuScenePath) ||
                       !File.Exists(GameScenePath);
        if (missing) BuildProject(false);
    }

    private static void BuildProject(bool showDialog)
    {
        PlayerSettings.productName = "ErrorGame";
        EnsureFolders();
        Sprite whiteSprite = CreateOrLoadWhiteSprite();
        Sprite heartSprite = CreateOrLoadHeartSprite();
        Sprite circleSprite = CreateOrLoadCircleSprite();
        Projectile projectilePrefab = CreateProjectilePrefab(whiteSprite);
        EnemyProjectile enemyProjectilePrefab = CreateEnemyProjectilePrefab(circleSprite);
        Bomb bombPrefab = CreateBombPrefab(circleSprite, whiteSprite);
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        if (inputActions == null) throw new InvalidOperationException("Input actions asset is missing.");
        GameObject playerPrefab = CreatePlayerPrefab(whiteSprite, projectilePrefab, bombPrefab, inputActions);
        MeleeEnemy enemyPrefab = CreateEnemyPrefab(whiteSprite);
        RangedEnemy rangedEnemyPrefab = CreateRangedEnemyPrefab(circleSprite, whiteSprite, enemyProjectilePrefab);
        EnemySpawnTable combatEnemySpawnTable = new EnemySpawnTable(new[]
        {
            new EnemySpawnEntry(enemyPrefab, 2),
            new EnemySpawnEntry(rangedEnemyPrefab, 1)
        });
        BossEnemy bossPrefab = CreateBossPrefab(circleSprite, whiteSprite);
        GameObject doorPrefab = CreateDoorPrefab(whiteSprite);
        BasicResourcePickup coinPickup = CreatePickupPrefab(CoinPickupPrefabPath, "Coin Pickup", circleSprite, new Color(1f, 0.76f, 0.12f), BasicResourceType.Coin, 3);
        BasicResourcePickup heartPickup = CreatePickupPrefab(HeartPickupPrefabPath, "Heart Pickup", heartSprite, new Color(0.95f, 0.15f, 0.22f), BasicResourceType.HalfHeart);
        BasicResourcePickup bombPickup = CreatePickupPrefab(BombPickupPrefabPath, "Bomb Pickup", circleSprite, new Color(0.4f, 0.44f, 0.52f), BasicResourceType.Bomb);
        NormalUpgradePickup upgradePickup = CreateNormalUpgradePrefab(circleSprite);
        SuperMushroomPickup superMushroom = CreateSuperMushroomPrefab(circleSprite, whiteSprite);
        ShopProduct shopProduct = CreateShopProductPrefab(whiteSprite);
        GameObject dungeonRoomPrefab = CreateDungeonRoomPrefab(
            whiteSprite,
            doorPrefab,
            combatEnemySpawnTable,
            bossPrefab,
            new[] { coinPickup, heartPickup, bombPickup },
            upgradePickup,
            superMushroom,
            shopProduct);
        GameObject hudPrefab = CreateHudPrefab(whiteSprite, heartSprite, circleSprite);
        CreateGameScene(playerPrefab, dungeonRoomPrefab, hudPrefab);
        CreateMainMenuScene(whiteSprite);
        ConfigureBuildScenes();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ValidateGeneratedResources();
        Debug.Log("ErrorGame final demo resources rebuilt: main menu, save/continue flow, gameplay systems, prefabs, and scenes.");
        if (showDialog)
        {
            EditorUtility.DisplayDialog("ErrorGame", "Final demo resources rebuilt successfully.", "OK");
        }
    }

    private static void ValidateGeneratedResources()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        GameSession session = Object.FindObjectOfType<GameSession>();
        Player player = Object.FindObjectOfType<Player>();
        StageAHudController hud = Object.FindObjectOfType<StageAHudController>();
        MinimapController minimap = Object.FindObjectOfType<MinimapController>();
        EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();

        if (session == null || player == null || hud == null || minimap == null || eventSystem == null)
        {
            throw new InvalidOperationException("Game scene is missing its GameSession, Player, HUD, minimap, or EventSystem.");
        }
        GameInputReader inputReader = player.GetComponent<GameInputReader>();
        if (inputReader == null || eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            throw new InvalidOperationException("Game scene has not been migrated to the new Input System.");
        }
        SerializedObject inputReaderObject = new SerializedObject(inputReader);
        if (inputReaderObject.FindProperty("inputActions").objectReferenceValue == null)
        {
            throw new InvalidOperationException("GameInputReader is missing its InputActionAsset.");
        }

        int missingScripts = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Sum(transform => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject));
        if (missingScripts > 0)
        {
            throw new InvalidOperationException($"Game scene contains {missingScripts} missing script reference(s).");
        }

        SerializedObject shooter = new SerializedObject(player.GetComponent<PlayerShooter>());
        if (shooter.FindProperty("muzzle").objectReferenceValue == null ||
            shooter.FindProperty("projectilePrefab").objectReferenceValue == null)
        {
            throw new InvalidOperationException("PlayerShooter prefab references are incomplete.");
        }

        PlayerBombPlacer playerBombPlacer = player.GetComponent<PlayerBombPlacer>();
        if (playerBombPlacer == null)
        {
            throw new InvalidOperationException("Player prefab is missing PlayerBombPlacer.");
        }
        SerializedObject bombPlacer = new SerializedObject(playerBombPlacer);
        if (bombPlacer.FindProperty("bombPrefab").objectReferenceValue == null ||
            Mathf.Abs(bombPlacer.FindProperty("placementCooldown").floatValue - 1f) > 0.001f)
        {
            throw new InvalidOperationException("PlayerBombPlacer requires a bomb prefab and a one-second placement cooldown.");
        }

        Bomb bombPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BombPrefabPath)?.GetComponent<Bomb>();
        if (bombPrefab == null)
        {
            throw new InvalidOperationException("Player bomb prefab is missing.");
        }
        SerializedObject bombObject = new SerializedObject(bombPrefab);
        if (bombObject.FindProperty("visual").objectReferenceValue == null ||
            Mathf.Abs(bombObject.FindProperty("fuseDuration").floatValue - 1.5f) > 0.001f)
        {
            throw new InvalidOperationException("Player bomb requires a visual and a 1.5-second fuse.");
        }

        SerializedObject sessionObject = new SerializedObject(session);
        string[] sessionReferences = { "player", "hud", "minimap", "gameCamera", "roomPrefab", "roomRoot" };
        if (sessionReferences.Any(property => sessionObject.FindProperty(property).objectReferenceValue == null) ||
            sessionObject.FindProperty("useFixedSeed").boolValue)
        {
            throw new InvalidOperationException("GameSession references are incomplete or the generated scene is not configured for random runs.");
        }

        GameObject dungeonRoomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DungeonRoomPrefabPath);
        RoomController room = dungeonRoomPrefab != null ? dungeonRoomPrefab.GetComponent<RoomController>() : null;
        if (room == null)
        {
            throw new InvalidOperationException("Dungeon room prefab is missing RoomController.");
        }

        SerializedObject roomObject = new SerializedObject(room);
        SerializedProperty doors = roomObject.FindProperty("doors");
        SerializedProperty roomFeatures = roomObject.FindProperty("roomFeatureBehaviours");
        if (doors.arraySize != 4 ||
            roomFeatures.arraySize != 6 ||
            roomObject.FindProperty("typeMarker").objectReferenceValue == null ||
            Enumerable.Range(0, doors.arraySize).Any(index => doors.GetArrayElementAtIndex(index).objectReferenceValue == null) ||
            Enumerable.Range(0, roomFeatures.arraySize).Any(index => roomFeatures.GetArrayElementAtIndex(index).objectReferenceValue == null))
        {
            throw new InvalidOperationException("Dungeon room prefab references are incomplete.");
        }

        CombatEncounterController encounter = dungeonRoomPrefab.GetComponent<CombatEncounterController>();
        if (encounter == null)
        {
            throw new InvalidOperationException("Dungeon room prefab is missing CombatEncounterController.");
        }
        SerializedObject encounterObject = new SerializedObject(encounter);
        SerializedProperty spawnPoints = encounterObject.FindProperty("spawnPoints");
        SerializedProperty rewardPrefabs = encounterObject.FindProperty("rewardPrefabs");
        EnemySpawnTable spawnTable = encounter.SpawnTable;
        if (encounterObject.FindProperty("enemyParent").objectReferenceValue == null ||
            encounterObject.FindProperty("rewardSpawnPoint").objectReferenceValue == null ||
            spawnTable == null || spawnTable.Entries.Count != 2 || spawnTable.TotalWeight != 3 ||
            spawnTable.Entries[0].Weight != 2 || spawnTable.Entries[1].Weight != 1 ||
            spawnTable.Entries.Any(entry => entry == null || !entry.IsUsable) ||
            spawnPoints.arraySize < 1 || rewardPrefabs.arraySize != 3 ||
            Enumerable.Range(0, spawnPoints.arraySize).Any(index => spawnPoints.GetArrayElementAtIndex(index).objectReferenceValue == null) ||
            Enumerable.Range(0, rewardPrefabs.arraySize).Any(index => rewardPrefabs.GetArrayElementAtIndex(index).objectReferenceValue == null))
        {
            throw new InvalidOperationException("Combat encounter prefab references are incomplete.");
        }

        BossEncounterController bossEncounter = dungeonRoomPrefab.GetComponent<BossEncounterController>();
        if (bossEncounter == null)
        {
            throw new InvalidOperationException("Dungeon room prefab is missing BossEncounterController.");
        }
        SerializedObject bossEncounterObject = new SerializedObject(bossEncounter);
        if (bossEncounterObject.FindProperty("spawnPoint").objectReferenceValue == null ||
            bossEncounterObject.FindProperty("bossPrefab").objectReferenceValue == null ||
            bossEncounterObject.FindProperty("rewardSpawnPoint").objectReferenceValue == null ||
            bossEncounterObject.FindProperty("rewardPrefab").objectReferenceValue == null)
        {
            throw new InvalidOperationException("Boss encounter prefab references are incomplete.");
        }

        ItemRoomController itemRoom = dungeonRoomPrefab.GetComponent<ItemRoomController>();
        ShopController shop = dungeonRoomPrefab.GetComponent<ShopController>();
        SecretRoomController secretRoom = dungeonRoomPrefab.GetComponent<SecretRoomController>();
        SuperSecretRoomController superSecretRoom = dungeonRoomPrefab.GetComponent<SuperSecretRoomController>();
        if (itemRoom == null || shop == null || secretRoom == null || superSecretRoom == null)
        {
            throw new InvalidOperationException("Dungeon room prefab is missing a special-room controller.");
        }
        SerializedObject itemRoomObject = new SerializedObject(itemRoom);
        SerializedObject shopObject = new SerializedObject(shop);
        SerializedObject secretRoomObject = new SerializedObject(secretRoom);
        SerializedObject superSecretRoomObject = new SerializedObject(superSecretRoom);
        SerializedProperty shopSlots = shopObject.FindProperty("slotPoints");
        SerializedProperty secretRewards = secretRoomObject.FindProperty("rewardPrefabs");
        SerializedProperty secretRewardPoints = secretRoomObject.FindProperty("spawnPoints");
        if (itemRoomObject.FindProperty("pickupPrefab").objectReferenceValue == null ||
            itemRoomObject.FindProperty("spawnPoint").objectReferenceValue == null ||
            shopObject.FindProperty("productPrefab").objectReferenceValue == null ||
            shopSlots.arraySize != 3 ||
            Enumerable.Range(0, shopSlots.arraySize).Any(index => shopSlots.GetArrayElementAtIndex(index).objectReferenceValue == null) ||
            secretRewards.arraySize != 3 || secretRewardPoints.arraySize != 3 ||
            superSecretRoomObject.FindProperty("mushroomPrefab").objectReferenceValue == null ||
            superSecretRoomObject.FindProperty("spawnPoint").objectReferenceValue == null ||
            Enumerable.Range(0, 3).Any(index =>
                secretRewards.GetArrayElementAtIndex(index).objectReferenceValue == null ||
                secretRewardPoints.GetArrayElementAtIndex(index).objectReferenceValue == null))
        {
            throw new InvalidOperationException("Special-room prefab references are incomplete.");
        }

        NormalUpgradePickup upgradePickup = AssetDatabase.LoadAssetAtPath<GameObject>(NormalUpgradePrefabPath)?.GetComponent<NormalUpgradePickup>();
        SuperMushroomPickup superMushroom = AssetDatabase.LoadAssetAtPath<GameObject>(SuperMushroomPrefabPath)?.GetComponent<SuperMushroomPickup>();
        ShopProduct shopProduct = AssetDatabase.LoadAssetAtPath<GameObject>(ShopProductPrefabPath)?.GetComponent<ShopProduct>();
        if (upgradePickup == null || superMushroom == null || shopProduct == null)
        {
            throw new InvalidOperationException("Normal upgrade or shop product prefab is missing.");
        }
        SerializedObject upgradeObject = new SerializedObject(upgradePickup);
        SerializedObject mushroomObject = new SerializedObject(superMushroom);
        SerializedObject productObject = new SerializedObject(shopProduct);
        if (upgradeObject.FindProperty("visual").objectReferenceValue == null ||
            upgradeObject.FindProperty("label").objectReferenceValue == null ||
            mushroomObject.FindProperty("visualRoot").objectReferenceValue == null ||
            mushroomObject.FindProperty("label").objectReferenceValue == null ||
            SuperMushroomPickup.MaxHealthIncreaseUnits != 2 ||
            Mathf.Abs(SuperMushroomPickup.OtherStatMultiplier - 1.5f) > 0.001f ||
            productObject.FindProperty("visual").objectReferenceValue == null ||
            productObject.FindProperty("label").objectReferenceValue == null)
        {
            throw new InvalidOperationException("Normal upgrade or shop product visual references are incomplete.");
        }

        DoorController[] doorControllers = dungeonRoomPrefab.GetComponentsInChildren<DoorController>(true);
        if (doorControllers.Length != 4 || doorControllers.Select(door => door.Direction).Distinct().Count() != 4 ||
            doorControllers.Any(door => !(door is IExplosionReceiver)) ||
            AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath)?.GetComponent<MeleeEnemy>() == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(RangedEnemyPrefabPath)?.GetComponent<RangedEnemy>() == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(EnemyProjectilePrefabPath)?.GetComponent<EnemyProjectile>() == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath)?.GetComponent<BossEnemy>() == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(CoinPickupPrefabPath)?.GetComponent<BasicResourcePickup>() == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(HeartPickupPrefabPath)?.GetComponent<BasicResourcePickup>() == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(BombPickupPrefabPath)?.GetComponent<BasicResourcePickup>() == null)
        {
            throw new InvalidOperationException("Stage B enemy, door, or pickup resources are invalid.");
        }

        BasicResourcePickup coinPickup = AssetDatabase.LoadAssetAtPath<GameObject>(CoinPickupPrefabPath).GetComponent<BasicResourcePickup>();
        if (new SerializedObject(coinPickup).FindProperty("amount").intValue != 3 ||
            System.Enum.GetValues(typeof(NormalUpgradeType)).Length != 6 ||
            System.Enum.GetValues(typeof(ShopProductType)).Length != 7)
        {
            throw new InvalidOperationException("Stage D economy or effect catalog configuration is invalid.");
        }

        MeleeEnemy meleeEnemy = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath).GetComponent<MeleeEnemy>();
        if (Mathf.Abs(new SerializedObject(meleeEnemy).FindProperty("chaseActivationDelay").floatValue - 0.3f) > 0.001f)
        {
            throw new InvalidOperationException("Melee enemy must wait 0.3 seconds before chasing the player.");
        }

        RangedEnemy rangedEnemy = AssetDatabase.LoadAssetAtPath<GameObject>(RangedEnemyPrefabPath).GetComponent<RangedEnemy>();
        SerializedObject rangedObject = new SerializedObject(rangedEnemy);
        if (rangedEnemy.GetComponent<EnemyHealth>().MaxHealthUnits != 3 ||
            Mathf.Abs(rangedEnemy.ActivationDelay - 0.3f) > 0.001f ||
            Mathf.Abs(rangedEnemy.FireInterval - 1.4f) > 0.001f ||
            rangedEnemy.ProjectilePrefab == null ||
            rangedObject.FindProperty("muzzle").objectReferenceValue == null ||
            rangedObject.FindProperty("bodyVisual").objectReferenceValue == null)
        {
            throw new InvalidOperationException("Ranged enemy movement, attack, or prefab references are invalid.");
        }

        BossEnemy bossEnemy = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath).GetComponent<BossEnemy>();
        SerializedObject bossObject = new SerializedObject(bossEnemy);
        if (bossObject.FindProperty("bodyVisual").objectReferenceValue == null ||
            bossObject.FindProperty("warningVisual").objectReferenceValue == null ||
            bossEnemy.GetComponent<EnemyHealth>().MaxHealthUnits != 24 ||
            bossEnemy.TelegraphDuration < 0.5f)
        {
            throw new InvalidOperationException("Boss prefab requires 24 health and a visible pre-dash warning.");
        }

        foreach (DoorController door in doorControllers)
        {
            SerializedObject doorObject = new SerializedObject(door);
            if (doorObject.FindProperty("blocker").objectReferenceValue == null ||
                doorObject.FindProperty("transitionTrigger").objectReferenceValue == null ||
                doorObject.FindProperty("visual").objectReferenceValue == null)
            {
                throw new InvalidOperationException("A dungeon door contains an unassigned reference.");
            }
        }

        DungeonGenerator generator = new DungeonGenerator();
        DungeonLayout firstLayout = generator.Generate(20260720, 8, 12);
        DungeonLayout secondLayout = generator.Generate(20260720, 8, 12);
        List<RoomNode> visibleRooms = firstLayout.VisibleRooms.ToList();
        bool differentSeedsCanVary = Enumerable.Range(1, 16)
            .Select(seed => generator.Generate(seed, 8, 12).GetDeterministicSignature())
            .Any(signature => signature != firstLayout.GetDeterministicSignature());
        if (firstLayout.GetDeterministicSignature() != secondLayout.GetDeterministicSignature() ||
            !differentSeedsCanVary ||
            visibleRooms.Count < 8 || visibleRooms.Count > 12 ||
            visibleRooms.Count(roomNode => roomNode.Type == RoomType.Start) != 1 ||
            visibleRooms.Count(roomNode => roomNode.Type == RoomType.Item) != 1 ||
            visibleRooms.Count(roomNode => roomNode.Type == RoomType.Shop) != 1 ||
            visibleRooms.Count(roomNode => roomNode.Type == RoomType.Boss) != 1 ||
            firstLayout.Rooms.Values.Count(roomNode => roomNode.Type == RoomType.Secret) != 1 ||
            firstLayout.Rooms.Values.Count(roomNode => roomNode.Type == RoomType.SuperSecret) != 1)
        {
            throw new InvalidOperationException("DungeonGenerator failed deterministic count or room-type validation.");
        }

        bool invalidBossTerminal = Enumerable.Range(0, 256).Any(seed =>
        {
            DungeonLayout layout = generator.Generate(seed, 8, 12);
            RoomNode bossRoom = layout.VisibleRooms.Single(roomNode => roomNode.Type == RoomType.Boss);
            if (bossRoom.Connections.Count != 1) return true;
            RoomDirection entrance = bossRoom.Connections.Single();
            if (!layout.TryGetConnectedRoom(bossRoom, entrance, out RoomNode neighbor) ||
                neighbor.Type != RoomType.Combat)
            {
                return true;
            }

            if (RoomDirectionUtility.All.Any(direction =>
                layout.TryGetRoom(bossRoom.Coordinate.Offset(direction), out RoomNode adjacent) &&
                RoomTypeUtility.IsHiddenRoom(adjacent.Type)))
            {
                return true;
            }

            Dictionary<RoomCoordinate, int> distances = new Dictionary<RoomCoordinate, int>
            {
                [layout.StartCoordinate] = 0
            };
            Queue<RoomCoordinate> pendingRooms = new Queue<RoomCoordinate>();
            pendingRooms.Enqueue(layout.StartCoordinate);
            while (pendingRooms.Count > 0)
            {
                RoomNode current = layout.Rooms[pendingRooms.Dequeue()];
                foreach (RoomDirection direction in current.Connections)
                {
                    if (layout.TryGetConnectedRoom(current, direction, out RoomNode connected) &&
                        !RoomTypeUtility.IsHiddenRoom(connected.Type) &&
                        !distances.ContainsKey(connected.Coordinate))
                    {
                        distances[connected.Coordinate] = distances[current.Coordinate] + 1;
                        pendingRooms.Enqueue(connected.Coordinate);
                    }
                }
            }

            if (distances.Count != layout.VisibleRooms.Count()) return true;
            int farthestNonBossDistance = layout.VisibleRooms
                .Where(roomNode => roomNode != bossRoom)
                .Max(roomNode => distances[roomNode.Coordinate]);
            return distances[bossRoom.Coordinate] <= farthestNonBossDistance;
        });
        if (invalidBossTerminal)
        {
            throw new InvalidOperationException("Boss room must be the unique farthest one-door terminal connected to Combat.");
        }

        HashSet<RoomCoordinate> reachable = new HashSet<RoomCoordinate> { firstLayout.StartCoordinate };
        Queue<RoomCoordinate> pending = new Queue<RoomCoordinate>();
        pending.Enqueue(firstLayout.StartCoordinate);
        while (pending.Count > 0)
        {
            RoomNode current = firstLayout.Rooms[pending.Dequeue()];
            foreach (RoomDirection direction in current.Connections)
            {
                if (firstLayout.TryGetConnectedRoom(current, direction, out RoomNode neighbor) &&
                    !RoomTypeUtility.IsHiddenRoom(neighbor.Type) && reachable.Add(neighbor.Coordinate))
                {
                    pending.Enqueue(neighbor.Coordinate);
                }
            }
        }
        if (reachable.Count != visibleRooms.Count)
        {
            throw new InvalidOperationException("DungeonGenerator produced disconnected visible rooms.");
        }

        bool missingAdjacentConnection = Enumerable.Range(0, 64).Any(seed =>
        {
            DungeonLayout layout = generator.Generate(seed, 8, 12);
            return layout.VisibleRooms.Any(roomNode => RoomDirectionUtility.All.Any(direction =>
                layout.TryGetRoom(roomNode.Coordinate.Offset(direction), out RoomNode neighbor) &&
                !RoomTypeUtility.IsHiddenRoom(neighbor.Type) &&
                (!roomNode.HasConnection(direction) ||
                 !neighbor.HasConnection(RoomDirectionUtility.Opposite(direction)))));
        });
        if (missingAdjacentConnection)
        {
            throw new InvalidOperationException("Two adjacent visible rooms were not connected by matching doors.");
        }

        bool invalidSecretNeighborhood = Enumerable.Range(0, 64).Any(seed =>
        {
            DungeonLayout layout = generator.Generate(seed, 8, 12);
            RoomNode secret = layout.Rooms.Values.Single(roomNode => roomNode.Type == RoomType.Secret);
            int adjacentRooms = RoomDirectionUtility.All.Count(direction =>
                layout.TryGetRoom(secret.Coordinate.Offset(direction), out RoomNode neighbor) &&
                !RoomTypeUtility.IsHiddenRoom(neighbor.Type));
            return adjacentRooms < 2;
        });
        if (invalidSecretNeighborhood)
        {
            throw new InvalidOperationException("A secret room was generated with fewer than two adjacent rooms.");
        }

        bool invalidSuperSecret = Enumerable.Range(0, 256).Any(seed =>
        {
            DungeonLayout layout = generator.Generate(seed, 8, 12);
            RoomNode superSecret = layout.Rooms.Values.Single(roomNode => roomNode.Type == RoomType.SuperSecret);
            List<RoomNode> neighbors = RoomDirectionUtility.All
                .Select(direction => layout.TryGetRoom(superSecret.Coordinate.Offset(direction), out RoomNode neighbor) ? neighbor : null)
                .Where(neighbor => neighbor != null)
                .ToList();
            if (superSecret.Connections.Count != 0 || neighbors.Count != 1 || neighbors[0].Type != RoomType.Combat)
            {
                return true;
            }

            RoomNode boss = layout.VisibleRooms.Single(roomNode => roomNode.Type == RoomType.Boss);
            List<RoomCoordinate> validPositions = layout.VisibleRooms
                .Where(roomNode => roomNode.Type == RoomType.Combat)
                .SelectMany(roomNode => RoomDirectionUtility.All.Select(direction => roomNode.Coordinate.Offset(direction)))
                .Distinct()
                .Where(coordinate => coordinate == superSecret.Coordinate || !layout.TryGetRoom(coordinate, out _))
                .Where(coordinate =>
                {
                    List<RoomNode> adjacentRooms = RoomDirectionUtility.All
                        .Select(direction => layout.TryGetRoom(coordinate.Offset(direction), out RoomNode neighbor) ? neighbor : null)
                        .Where(neighbor => neighbor != null && neighbor != superSecret)
                        .ToList();
                    return adjacentRooms.Count == 1 && adjacentRooms[0].Type == RoomType.Combat;
                })
                .ToList();
            int nearestDistance = validPositions.Min(coordinate => coordinate.ManhattanDistance(boss.Coordinate));
            return superSecret.Coordinate.ManhattanDistance(boss.Coordinate) != nearestDistance;
        });
        if (invalidSuperSecret)
        {
            throw new InvalidOperationException("Super Secret room must be the nearest valid Boss-side terminal attached to Combat.");
        }

        bool adjacentSpecialRooms = Enumerable.Range(0, 64).Any(seed =>
        {
            DungeonLayout layout = generator.Generate(seed, 8, 12);
            return layout.VisibleRooms
                .Where(roomNode => RoomTypeUtility.IsVisibleSpecialRoom(roomNode.Type))
                .Any(roomNode => roomNode.Connections.Any(direction =>
                    layout.TryGetConnectedRoom(roomNode, direction, out RoomNode neighbor) &&
                    RoomTypeUtility.IsVisibleSpecialRoom(neighbor.Type)));
        });
        if (adjacentSpecialRooms)
        {
            throw new InvalidOperationException("DungeonGenerator connected two visible special rooms directly.");
        }

        RoomNode startRoom = firstLayout.Rooms[firstLayout.StartCoordinate];
        if (MinimapController.ShouldDisplayRoom(firstLayout, startRoom))
        {
            throw new InvalidOperationException("The minimap exposed an unvisited room before exploration began.");
        }
        startRoom.MarkVisited();
        int expectedDiscoveredRooms = 1 + startRoom.Connections.Count(direction =>
            firstLayout.TryGetConnectedRoom(startRoom, direction, out RoomNode neighbor) &&
            !RoomTypeUtility.IsHiddenRoom(neighbor.Type));
        int actualDiscoveredRooms = firstLayout.Rooms.Values.Count(roomNode =>
            MinimapController.ShouldDisplayRoom(firstLayout, roomNode));
        RoomNode hiddenRoom = firstLayout.Rooms.Values.Single(roomNode => roomNode.Type == RoomType.Secret);
        RoomNode superHiddenRoom = firstLayout.Rooms.Values.Single(roomNode => roomNode.Type == RoomType.SuperSecret);
        RoomNode discoveredNeighbor = startRoom.Connections
            .Select(direction => firstLayout.TryGetConnectedRoom(startRoom, direction, out RoomNode neighbor) ? neighbor : null)
            .First(roomNode => roomNode != null && !RoomTypeUtility.IsHiddenRoom(roomNode.Type));
        if (actualDiscoveredRooms != expectedDiscoveredRooms ||
            MinimapController.ShouldDisplayRoom(firstLayout, hiddenRoom) ||
            MinimapController.ShouldDisplayRoom(firstLayout, superHiddenRoom) ||
            !MinimapController.ShouldDisplayConnection(firstLayout, startRoom, discoveredNeighbor) ||
            MinimapController.GetRoomMarker(RoomType.Start) != "S" ||
            MinimapController.GetRoomMarker(RoomType.Item) != "I" ||
            MinimapController.GetRoomMarker(RoomType.Shop) != "$" ||
            MinimapController.GetRoomMarker(RoomType.Boss) != "B" ||
            MinimapController.GetRoomMarker(RoomType.SuperSecret) != "M")
        {
            throw new InvalidOperationException("Minimap exploration visibility or special-room markers are invalid.");
        }


        RoomDirection hiddenToSourceDirection = RoomDirectionUtility.All.First(direction =>
            firstLayout.TryGetRoom(hiddenRoom.Coordinate.Offset(direction), out RoomNode neighbor) &&
            !RoomTypeUtility.IsHiddenRoom(neighbor.Type));
        RoomNode secretSourceRoom = firstLayout.Rooms[hiddenRoom.Coordinate.Offset(hiddenToSourceDirection)];
        RoomDirection sourceToHiddenDirection = RoomDirectionUtility.Opposite(hiddenToSourceDirection);
        if (!firstLayout.IsSecretPassageCandidate(secretSourceRoom, sourceToHiddenDirection) ||
            !firstLayout.TryOpenSecretPassage(secretSourceRoom, sourceToHiddenDirection) ||
            !firstLayout.TryGetConnectedRoom(secretSourceRoom, sourceToHiddenDirection, out RoomNode openedSecretRoom) ||
            openedSecretRoom != hiddenRoom ||
            !hiddenRoom.HasConnection(hiddenToSourceDirection) ||
            MinimapController.ShouldDisplayRoom(firstLayout, hiddenRoom))
        {
            throw new InvalidOperationException("Secret passage opening or pre-entry minimap hiding is invalid.");
        }
        hiddenRoom.MarkVisited();
        if (!MinimapController.ShouldDisplayRoom(firstLayout, hiddenRoom) ||
            !MinimapController.ShouldDisplayConnection(firstLayout, secretSourceRoom, hiddenRoom))
        {
            throw new InvalidOperationException("An entered secret room did not persist on the minimap with its passage.");
        }

        RoomDirection superHiddenToSourceDirection = RoomDirectionUtility.All.Single(direction =>
            firstLayout.TryGetRoom(superHiddenRoom.Coordinate.Offset(direction), out RoomNode neighbor) &&
            !RoomTypeUtility.IsHiddenRoom(neighbor.Type));
        RoomNode superSecretSource = firstLayout.Rooms[superHiddenRoom.Coordinate.Offset(superHiddenToSourceDirection)];
        RoomDirection sourceToSuperHidden = RoomDirectionUtility.Opposite(superHiddenToSourceDirection);
        // In gameplay the wall can only be bombed from the current (therefore
        // visited) combat room. Mirror that precondition before testing the map.
        superSecretSource.MarkVisited();
        if (superSecretSource.Type != RoomType.Combat ||
            !firstLayout.TryOpenSecretPassage(superSecretSource, sourceToSuperHidden) ||
            !firstLayout.TryGetConnectedRoom(superSecretSource, sourceToSuperHidden, out RoomNode openedSuperSecret) ||
            openedSuperSecret != superHiddenRoom ||
            MinimapController.ShouldDisplayRoom(firstLayout, superHiddenRoom))
        {
            throw new InvalidOperationException("Super Secret passage or pre-entry minimap hiding is invalid.");
        }
        superHiddenRoom.MarkVisited();
        if (!MinimapController.ShouldDisplayRoom(firstLayout, superHiddenRoom) ||
            !MinimapController.ShouldDisplayConnection(firstLayout, superSecretSource, superHiddenRoom))
        {
            throw new InvalidOperationException("An entered Super Secret room did not persist on the minimap.");
        }

        SerializedObject hudObject = new SerializedObject(hud);
        string[] hudReferences = { "heartsRoot", "heartTemplate", "coinText", "bombText", "promptText", "dashText", "statsText", "deathPanel", "pauseMenu" };
        if (hudReferences.Any(property => hudObject.FindProperty(property).objectReferenceValue == null))
        {
            throw new InvalidOperationException("HUD prefab contains an unassigned serialized reference.");
        }

        StageAPauseMenuController pause = hud.GetComponent<StageAPauseMenuController>();
        SerializedObject pauseObject = new SerializedObject(pause);
        string[] pauseReferences = { "overlay", "tabContent", "itemsButton", "notebookButton", "logButton", "mainMenuButton" };
        if (pauseReferences.Any(property => pauseObject.FindProperty(property).objectReferenceValue == null))
        {
            throw new InvalidOperationException("Pause menu prefab contains an unassigned serialized reference.");
        }
        if (pauseObject.FindProperty("statusButton") != null || hud.transform.Find("Pause Menu/Status Tab") != null)
        {
            throw new InvalidOperationException("The removed pause-menu status page was generated again.");
        }

        SerializedObject minimapObject = new SerializedObject(minimap);
        if (minimapObject.FindProperty("nodesRoot").objectReferenceValue == null ||
            minimapObject.FindProperty("nodeTemplate").objectReferenceValue == null ||
            minimapObject.FindProperty("connectionTemplate").objectReferenceValue == null)
        {
            throw new InvalidOperationException("Minimap prefab contains an unassigned serialized reference.");
        }
        RectTransform minimapNodesRoot = minimapObject.FindProperty("nodesRoot").objectReferenceValue as RectTransform;
        RectTransform minimapPanel = minimapNodesRoot != null ? minimapNodesRoot.parent as RectTransform : null;
        if (minimapPanel == null || minimapPanel.anchorMin != Vector2.one || minimapPanel.anchorMax != Vector2.one ||
            minimapPanel.pivot != Vector2.one || Vector2.Distance(minimapPanel.anchoredPosition, new Vector2(-32f, -32f)) > 0.01f)
        {
            throw new InvalidOperationException("Minimap must remain fixed to the top-right corner.");
        }

        GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
        Canvas hudCanvas = hud.GetComponentInParent<Canvas>();
        if (Vector3.Distance(hudPrefab.transform.localScale, Vector3.one) > 0.0001f ||
            Vector3.Distance(hud.transform.localScale, Vector3.one) > 0.0001f ||
            hudCanvas == null || hud.transform.parent != hudCanvas.transform)
        {
            throw new InvalidOperationException("HUD prefab must have scale (1, 1, 1) and be a direct child of the scene Canvas.");
        }

        GameSaveData saveSample = new GameSaveData
        {
            dungeonSeed = 20260720,
            currentRoomX = 2,
            currentRoomY = -1,
            player = new PlayerSaveData
            {
                currentHealthUnits = 5,
                maxHealthUnits = 8,
                coins = 7,
                bombs = 2,
                moveSpeed = 6.25f,
                fireRate = 7f,
                damage = 2.5f,
                range = 9f,
                projectileSpeed = 14f,
                luck = 1f
            }
        };
        saveSample.rooms.Add(new RoomSaveData
        {
            x = 2,
            y = -1,
            visited = true,
            cleared = true,
            itemClaimed = true,
            purchasedShopSlots = new List<int> { 0, 2 },
            combatRewardType = 1,
            combatRewardCollected = true,
            collectedSecretRewards = new List<int> { 1 }
        });
        saveSample.rooms.Add(new RoomSaveData { x = 0, y = 0, visited = true, cleared = true });
        saveSample.rooms.Add(new RoomSaveData { x = 1, y = 0 });
        saveSample.rooms.Add(new RoomSaveData { x = 0, y = 1 });
        saveSample.openedSecretPassages.Add(new SecretPassageSaveData
        {
            sourceX = 2,
            sourceY = -1,
            direction = (int)RoomDirection.North
        });
        JsonGameSaveRepository saveRepository = new JsonGameSaveRepository();
        GameSaveData saveRoundTrip = saveRepository.Deserialize(saveRepository.Serialize(saveSample));
        if (!saveRepository.IsValid(saveRoundTrip) ||
            saveRoundTrip.dungeonSeed != saveSample.dungeonSeed ||
            saveRoundTrip.player.coins != 7 ||
            saveRoundTrip.rooms.Count != 4 ||
            saveRoundTrip.rooms[0].purchasedShopSlots.Count != 2 ||
            saveRoundTrip.openedSecretPassages.Count != 1)
        {
            throw new InvalidOperationException("Game save data failed its JSON round-trip validation.");
        }

        Scene menuScene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        MainMenuController menu = Object.FindObjectOfType<MainMenuController>();
        EventSystem menuEventSystem = Object.FindObjectOfType<EventSystem>();
        Text gameTitle = Object.FindObjectsOfType<Text>(true).FirstOrDefault(text => text.gameObject.name == "Game Title");
        int menuMissingScripts = menuScene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Sum(transform => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject));
        if (menu == null || menuEventSystem == null ||
            menuEventSystem.GetComponent<InputSystemUIInputModule>() == null ||
            gameTitle == null || gameTitle.text != "ErrorGame" || menuMissingScripts > 0)
        {
            throw new InvalidOperationException("Main menu scene is incomplete or its ErrorGame title is invalid.");
        }

        SerializedObject menuObject = new SerializedObject(menu);
        string[] menuReferences = { "startButton", "continueButton", "quitButton" };
        if (menuReferences.Any(property => menuObject.FindProperty(property).objectReferenceValue == null))
        {
            throw new InvalidOperationException("Main menu contains an unassigned button reference.");
        }

        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
        if (buildScenes.Length != 2 || !buildScenes[0].enabled || !buildScenes[1].enabled ||
            buildScenes[0].path != MainMenuScenePath || buildScenes[1].path != GameScenePath)
        {
            throw new InvalidOperationException("Build Settings must contain MainMenu first and Game second.");
        }

        Debug.Log("ErrorGame final resource validation passed: stages A-F, main menu, save JSON, continue flow, build scene order, and prefab references are valid.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Art");
        EnsureFolder("Assets/Art", "Generated");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets/Prefabs", "Player");
        EnsureFolder("Assets/Prefabs", "Combat");
        EnsureFolder("Assets/Prefabs", "Enemies");
        EnsureFolder("Assets/Prefabs", "Rooms");
        EnsureFolder("Assets/Prefabs", "Doors");
        EnsureFolder("Assets/Prefabs", "Pickups");
        EnsureFolder("Assets/Prefabs", "UI");
        EnsureFolder("Assets", "Scenes");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private static Sprite CreateOrLoadWhiteSprite()
    {
        return CreateOrLoadGeneratedSprite(WhiteSpritePath, "StageAWhite", (x, y) => Color.white);
    }

    private static Sprite CreateOrLoadHeartSprite()
    {
        return CreateOrLoadGeneratedSprite(HeartSpritePath, "StageAHeart", (x, y) =>
        {
            float px = (x + 0.5f) / 32f * 2f - 1f;
            float py = (y + 0.5f) / 32f * 2f - 1f;
            px *= 1.08f;
            py = py * 1.08f + 0.08f;
            float value = Mathf.Pow(px * px + py * py - 1f, 3f) - px * px * py * py * py;
            return value <= 0f ? Color.white : Color.clear;
        });
    }

    private static Sprite CreateOrLoadCircleSprite()
    {
        return CreateOrLoadGeneratedSprite(CircleSpritePath, "StageACircle", (x, y) =>
        {
            Vector2 point = new Vector2((x + 0.5f) / 32f - 0.5f, (y + 0.5f) / 32f - 0.5f);
            return point.sqrMagnitude <= 0.23f ? Color.white : Color.clear;
        });
    }

    private static Sprite CreateOrLoadGeneratedSprite(string path, string name, System.Func<int, int, Color> pixelFactory)
    {
        Sprite existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (existing != null) return existing;

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = name + "Texture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++) texture.SetPixel(x, y, pixelFactory(x, y));
        }
        texture.Apply();
        AssetDatabase.CreateAsset(texture, path);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.name = name + "Sprite";
        AssetDatabase.AddObjectToAsset(sprite, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
    }

    private static Projectile CreateProjectilePrefab(Sprite sprite)
    {
        GameObject root = new GameObject("PlayerProjectile");
        root.transform.localScale = new Vector3(0.14f, 0.3f, 1f);

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(1f, 0.83f, 0.18f);
        renderer.sortingOrder = 5;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.freezeRotation = true;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        root.AddComponent<Projectile>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ProjectilePrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<Projectile>();
    }

    private static EnemyProjectile CreateEnemyProjectilePrefab(Sprite sprite)
    {
        GameObject root = new GameObject("EnemyProjectile");
        root.transform.localScale = new Vector3(0.24f, 0.24f, 1f);

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0.78f, 0.32f, 1f);
        renderer.sortingOrder = 5;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.freezeRotation = true;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        root.AddComponent<EnemyProjectile>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, EnemyProjectilePrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<EnemyProjectile>();
    }

    private static Bomb CreateBombPrefab(Sprite circleSprite, Sprite whiteSprite)
    {
        GameObject root = new GameObject("PlayerBomb");
        SpriteRenderer body = root.AddComponent<SpriteRenderer>();
        body.sprite = circleSprite;
        body.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        body.sortingOrder = 4;
        body.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

        GameObject fuse = CreateWorldVisual("Fuse", root.transform, whiteSprite, new Vector2(0.22f, 0.34f), new Vector2(0.12f, 0.34f), new Color(1f, 0.55f, 0.12f), 5);
        fuse.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);

        Bomb bomb = root.AddComponent<Bomb>();
        bomb.Configure(body, 1.5f, 2.2f, 2);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, BombPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<Bomb>();
    }

    private static GameObject CreatePlayerPrefab(
        Sprite sprite,
        Projectile projectilePrefab,
        Bomb bombPrefab,
        InputActionAsset inputActions)
    {
        GameObject root = new GameObject("Player");

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.radius = 0.42f;

        root.AddComponent<PlayerStats>();
        root.AddComponent<PlayerHealth>();
        root.AddComponent<PlayerInventory>();
        GameInputReader inputReader = root.AddComponent<GameInputReader>();
        inputReader.Configure(inputActions);
        root.AddComponent<PlayerMovement>();
        PlayerAim aim = root.AddComponent<PlayerAim>();
        root.AddComponent<Player>();
        root.AddComponent<PlayerDash>();
        PlayerShooter shooter = root.AddComponent<PlayerShooter>();
        PlayerBombPlacer bombPlacer = root.AddComponent<PlayerBombPlacer>();
        root.AddComponent<PlayerInteractor>();

        GameObject visual = CreateWorldVisual("Body", root.transform, sprite, Vector2.zero, new Vector2(0.82f, 0.82f), new Color(0.24f, 0.72f, 1f), 3);
        visual.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

        GameObject aimPivot = new GameObject("Aim Pivot");
        aimPivot.transform.SetParent(root.transform, false);
        CreateWorldVisual("Gun", aimPivot.transform, sprite, new Vector2(0f, 0.48f), new Vector2(0.18f, 0.72f), new Color(0.95f, 0.95f, 1f), 4);
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(aimPivot.transform, false);
        muzzle.transform.localPosition = new Vector3(0f, 0.9f, 0f);

        aim.Configure(aimPivot.transform);
        shooter.Configure(muzzle.transform, projectilePrefab);
        bombPlacer.Configure(bombPrefab, 1f);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static MeleeEnemy CreateEnemyPrefab(Sprite sprite)
    {
        GameObject root = new GameObject("MeleeEnemy");

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.radius = 0.46f;

        root.AddComponent<EnemyHealth>();
        MeleeEnemy enemy = root.AddComponent<MeleeEnemy>();

        GameObject bodyVisual = CreateWorldVisual("Body", root.transform, sprite, Vector2.zero, new Vector2(0.9f, 0.9f), new Color(0.86f, 0.18f, 0.2f), 3);
        bodyVisual.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        CreateWorldVisual("Eye Left", root.transform, sprite, new Vector2(-0.17f, 0.13f), new Vector2(0.12f, 0.12f), Color.white, 4);
        CreateWorldVisual("Eye Right", root.transform, sprite, new Vector2(0.17f, 0.13f), new Vector2(0.12f, 0.12f), Color.white, 4);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<MeleeEnemy>();
    }

    private static RangedEnemy CreateRangedEnemyPrefab(
        Sprite circleSprite,
        Sprite detailSprite,
        EnemyProjectile projectilePrefab)
    {
        GameObject root = new GameObject("RangedEnemy");

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.radius = 0.48f;
        EnemyHealth health = root.AddComponent<EnemyHealth>();
        health.Configure(3);

        SpriteRenderer bodyVisual = CreateWorldVisual(
            "Body",
            root.transform,
            circleSprite,
            Vector2.zero,
            new Vector2(0.95f, 0.95f),
            new Color(0.42f, 0.2f, 0.72f),
            3).GetComponent<SpriteRenderer>();
        CreateWorldVisual("Core", root.transform, circleSprite, Vector2.zero, new Vector2(0.36f, 0.36f), new Color(0.9f, 0.62f, 1f), 4);
        CreateWorldVisual("Barrel", root.transform, detailSprite, new Vector2(0f, 0.48f), new Vector2(0.18f, 0.58f), new Color(0.84f, 0.72f, 1f), 4);
        Transform muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(root.transform, false);
        muzzle.localPosition = new Vector3(0f, 0.82f, 0f);

        RangedEnemy enemy = root.AddComponent<RangedEnemy>();
        enemy.Configure(projectilePrefab, muzzle, bodyVisual);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, RangedEnemyPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<RangedEnemy>();
    }

    private static BossEnemy CreateBossPrefab(Sprite circleSprite, Sprite detailSprite)
    {
        GameObject root = new GameObject("BossEnemy");

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.radius = 0.9f;
        EnemyHealth health = root.AddComponent<EnemyHealth>();
        health.Configure(24);

        SpriteRenderer warning = CreateWorldVisual(
            "Dash Warning",
            root.transform,
            circleSprite,
            Vector2.zero,
            new Vector2(2.7f, 2.7f),
            new Color(1f, 0.04f, 0.03f, 0.5f),
            2).GetComponent<SpriteRenderer>();
        SpriteRenderer bossBody = CreateWorldVisual(
            "Body",
            root.transform,
            circleSprite,
            Vector2.zero,
            new Vector2(1.8f, 1.8f),
            new Color(0.5f, 0.12f, 0.16f),
            3).GetComponent<SpriteRenderer>();
        CreateWorldVisual("Eye Left", root.transform, detailSprite, new Vector2(-0.32f, 0.2f), new Vector2(0.22f, 0.22f), Color.white, 4);
        CreateWorldVisual("Eye Right", root.transform, detailSprite, new Vector2(0.32f, 0.2f), new Vector2(0.22f, 0.22f), Color.white, 4);

        BossEnemy boss = root.AddComponent<BossEnemy>();
        boss.Configure(bossBody, warning);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, BossPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<BossEnemy>();
    }

    private static GameObject CreateDoorPrefab(Sprite sprite)
    {
        GameObject root = new GameObject("CombatDoor");
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 2;

        BoxCollider2D blocker = root.AddComponent<BoxCollider2D>();
        BoxCollider2D transitionTrigger = root.AddComponent<BoxCollider2D>();
        transitionTrigger.isTrigger = true;
        DoorController door = root.AddComponent<DoorController>();
        door.Configure(RoomDirection.North, blocker, transitionTrigger, renderer);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, DoorPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static BasicResourcePickup CreatePickupPrefab(
        string path,
        string name,
        Sprite sprite,
        Color color,
        BasicResourceType resourceType,
        int amount = 1)
    {
        GameObject root = new GameObject(name);
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.48f;

        BasicResourcePickup pickup = root.AddComponent<BasicResourcePickup>();
        pickup.Configure(resourceType, amount);
        CreateWorldVisual("Visual", root.transform, sprite, Vector2.zero, new Vector2(0.72f, 0.72f), color, 3);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<BasicResourcePickup>();
    }

    private static NormalUpgradePickup CreateNormalUpgradePrefab(Sprite sprite)
    {
        GameObject root = new GameObject("NormalUpgradePickup");
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.85f;

        SpriteRenderer visual = CreateWorldVisual("Visual", root.transform, sprite, Vector2.zero, new Vector2(1.1f, 1.1f), Color.white, 4).GetComponent<SpriteRenderer>();
        TextMesh label = CreateWorldLabel("Label", root.transform, "UPGRADE\nFREE", new Vector2(0f, 1.25f), 0.11f, Color.white, 6);
        NormalUpgradePickup pickup = root.AddComponent<NormalUpgradePickup>();
        pickup.Configure(visual, label);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, NormalUpgradePrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<NormalUpgradePickup>();
    }

    private static SuperMushroomPickup CreateSuperMushroomPrefab(Sprite circleSprite, Sprite whiteSprite)
    {
        GameObject root = new GameObject("SuperMushroom");
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1.35f;

        Transform visualRoot = new GameObject("Mushroom Visual").transform;
        visualRoot.SetParent(root.transform, false);
        CreateWorldVisual("Stem", visualRoot, whiteSprite, new Vector2(0f, -0.55f), new Vector2(0.9f, 1.45f), new Color(0.96f, 0.86f, 0.66f), 4);
        CreateWorldVisual("Cap", visualRoot, circleSprite, new Vector2(0f, 0.35f), new Vector2(2.8f, 1.55f), new Color(0.12f, 0.84f, 0.34f), 5);
        CreateWorldVisual("Spot Left", visualRoot, circleSprite, new Vector2(-0.72f, 0.43f), new Vector2(0.42f, 0.42f), Color.white, 6);
        CreateWorldVisual("Spot Center", visualRoot, circleSprite, new Vector2(0f, 0.72f), new Vector2(0.5f, 0.5f), Color.white, 6);
        CreateWorldVisual("Spot Right", visualRoot, circleSprite, new Vector2(0.72f, 0.38f), new Vector2(0.36f, 0.36f), Color.white, 6);
        TextMesh label = CreateWorldLabel("Label", root.transform, "SUPER MUSHROOM\nMAX HEARTS +1\nOTHER STATS +50%", new Vector2(0f, 1.95f), 0.075f, new Color(0.42f, 1f, 0.62f), 7);
        SuperMushroomPickup pickup = root.AddComponent<SuperMushroomPickup>();
        pickup.Configure(visualRoot, label);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SuperMushroomPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<SuperMushroomPickup>();
    }

    private static ShopProduct CreateShopProductPrefab(Sprite sprite)
    {
        GameObject root = new GameObject("ShopProduct");
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1.15f;

        SpriteRenderer visual = CreateWorldVisual("Visual", root.transform, sprite, Vector2.zero, new Vector2(1.05f, 1.05f), Color.white, 4).GetComponent<SpriteRenderer>();
        TextMesh label = CreateWorldLabel("Label", root.transform, "PRODUCT\n1 COIN\nF BUY", new Vector2(0f, 1.35f), 0.095f, Color.white, 6);
        ShopProduct product = root.AddComponent<ShopProduct>();
        product.Configure(visual, label);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ShopProductPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<ShopProduct>();
    }

    private static GameObject CreateDungeonRoomPrefab(
        Sprite sprite,
        GameObject doorPrefab,
        EnemySpawnTable enemySpawnTable,
        BossEnemy bossPrefab,
        BasicResourcePickup[] rewardPrefabs,
        NormalUpgradePickup upgradePickupPrefab,
        SuperMushroomPickup superMushroomPrefab,
        ShopProduct shopProductPrefab)
    {
        GameObject root = new GameObject("DungeonRoom");
        CombatEncounterController encounter = root.AddComponent<CombatEncounterController>();
        BossEncounterController bossEncounter = root.AddComponent<BossEncounterController>();
        ItemRoomController itemRoom = root.AddComponent<ItemRoomController>();
        ShopController shop = root.AddComponent<ShopController>();
        SecretRoomController secretRoom = root.AddComponent<SecretRoomController>();
        SuperSecretRoomController superSecretRoom = root.AddComponent<SuperSecretRoomController>();
        RoomController room = root.AddComponent<RoomController>();

        Transform environment = new GameObject("Environment").transform;
        environment.SetParent(root.transform, false);
        Color wallColor = new Color(0.28f, 0.31f, 0.38f);
        Color hallWallColor = new Color(0.22f, 0.25f, 0.31f);
        CreateBlock("Floor", environment, sprite, Vector2.zero, new Vector2(19f, 12f), new Color(0.12f, 0.14f, 0.18f), false, -10);
        SpriteRenderer typeMarker = CreateWorldVisual("Room Type Marker", environment, sprite, Vector2.zero, new Vector2(1.5f, 1.5f), Color.clear, -2).GetComponent<SpriteRenderer>();

        // Each wall is split around a real doorway. Short vestibules contain the
        // transition triggers; the room graph, not world adjacency, selects the destination.
        CreateBlock("Wall Top Left", environment, sprite, new Vector2(-5.525f, 6.1f), new Vector2(8.45f, 0.6f), wallColor, true, 0);
        CreateBlock("Wall Top Right", environment, sprite, new Vector2(5.525f, 6.1f), new Vector2(8.45f, 0.6f), wallColor, true, 0);
        CreateBlock("Wall Bottom Left", environment, sprite, new Vector2(-5.525f, -6.1f), new Vector2(8.45f, 0.6f), wallColor, true, 0);
        CreateBlock("Wall Bottom Right", environment, sprite, new Vector2(5.525f, -6.1f), new Vector2(8.45f, 0.6f), wallColor, true, 0);
        CreateBlock("Wall Left Top", environment, sprite, new Vector2(-9.6f, 3.65f), new Vector2(0.6f, 4.7f), wallColor, true, 0);
        CreateBlock("Wall Left Bottom", environment, sprite, new Vector2(-9.6f, -3.65f), new Vector2(0.6f, 4.7f), wallColor, true, 0);
        CreateBlock("Wall Right Top", environment, sprite, new Vector2(9.6f, 3.65f), new Vector2(0.6f, 4.7f), wallColor, true, 0);
        CreateBlock("Wall Right Bottom", environment, sprite, new Vector2(9.6f, -3.65f), new Vector2(0.6f, 4.7f), wallColor, true, 0);

        CreateVestibules(environment, sprite, hallWallColor);

        DoorController[] doors =
        {
            CreateDoorInstance(doorPrefab, root.transform, "Door North", RoomDirection.North, new Vector2(0f, 6.1f), new Vector2(2.6f, 0.6f)),
            CreateDoorInstance(doorPrefab, root.transform, "Door South", RoomDirection.South, new Vector2(0f, -6.1f), new Vector2(2.6f, 0.6f)),
            CreateDoorInstance(doorPrefab, root.transform, "Door West", RoomDirection.West, new Vector2(-9.6f, 0f), new Vector2(0.6f, 2.6f)),
            CreateDoorInstance(doorPrefab, root.transform, "Door East", RoomDirection.East, new Vector2(9.6f, 0f), new Vector2(0.6f, 2.6f))
        };

        Transform enemiesRoot = new GameObject("Enemies").transform;
        enemiesRoot.SetParent(root.transform, false);
        Transform spawnRoot = new GameObject("Enemy Spawn Points").transform;
        spawnRoot.SetParent(root.transform, false);
        Transform[] spawnPoints =
        {
            CreateMarker("Spawn 1", spawnRoot, new Vector2(-5.2f, 2.6f)),
            CreateMarker("Spawn 2", spawnRoot, new Vector2(4.8f, 2.4f)),
            CreateMarker("Spawn 3", spawnRoot, new Vector2(1.2f, -1.2f))
        };
        Transform rewardPoint = CreateMarker("Reward Spawn Point", root.transform, Vector2.zero);
        Transform bossSpawnPoint = CreateMarker("Boss Spawn Point", root.transform, new Vector2(0f, 1.2f));

        Transform itemSpawnPoint = CreateMarker("Item Spawn Point", root.transform, Vector2.zero);
        Transform mushroomSpawnPoint = CreateMarker("Super Mushroom Spawn Point", root.transform, Vector2.zero);
        Transform shopSlotRoot = new GameObject("Shop Slot Points").transform;
        shopSlotRoot.SetParent(root.transform, false);
        Transform[] shopSlotPoints =
        {
            CreateMarker("Shop Slot 1", shopSlotRoot, new Vector2(-4f, 0f)),
            CreateMarker("Shop Slot 2", shopSlotRoot, Vector2.zero),
            CreateMarker("Shop Slot 3", shopSlotRoot, new Vector2(4f, 0f))
        };

        Transform secretRewardRoot = new GameObject("Secret Reward Points").transform;
        secretRewardRoot.SetParent(root.transform, false);
        Transform[] secretRewardPoints =
        {
            CreateMarker("Secret Coin", secretRewardRoot, new Vector2(-3f, 0f)),
            CreateMarker("Secret Heart", secretRewardRoot, Vector2.zero),
            CreateMarker("Secret Bomb", secretRewardRoot, new Vector2(3f, 0f))
        };

        encounter.Configure(
            enemiesRoot,
            spawnPoints,
            rewardPoint,
            enemySpawnTable,
            rewardPrefabs);
        bossEncounter.Configure(bossSpawnPoint, bossPrefab, rewardPoint, upgradePickupPrefab);
        itemRoom.Configure(upgradePickupPrefab, itemSpawnPoint);
        shop.Configure(shopProductPrefab, shopSlotPoints);
        secretRoom.Configure(rewardPrefabs, secretRewardPoints);
        superSecretRoom.Configure(superMushroomPrefab, mushroomSpawnPoint);
        room.Configure(
            doors,
            new MonoBehaviour[] { encounter, bossEncounter, itemRoom, shop, secretRoom, superSecretRoom },
            typeMarker);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, DungeonRoomPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void CreateVestibules(Transform parent, Sprite sprite, Color wallColor)
    {
        Color floorColor = new Color(0.1f, 0.12f, 0.16f);
        CreateBlock("North Vestibule Floor", parent, sprite, new Vector2(0f, 6.8f), new Vector2(2.6f, 1.4f), floorColor, false, -10);
        CreateBlock("North Vestibule Left", parent, sprite, new Vector2(-1.45f, 6.8f), new Vector2(0.3f, 1.4f), wallColor, true, 0);
        CreateBlock("North Vestibule Right", parent, sprite, new Vector2(1.45f, 6.8f), new Vector2(0.3f, 1.4f), wallColor, true, 0);
        CreateBlock("North Vestibule Cap", parent, sprite, new Vector2(0f, 7.5f), new Vector2(3.2f, 0.4f), wallColor, true, 0);

        CreateBlock("South Vestibule Floor", parent, sprite, new Vector2(0f, -6.8f), new Vector2(2.6f, 1.4f), floorColor, false, -10);
        CreateBlock("South Vestibule Left", parent, sprite, new Vector2(-1.45f, -6.8f), new Vector2(0.3f, 1.4f), wallColor, true, 0);
        CreateBlock("South Vestibule Right", parent, sprite, new Vector2(1.45f, -6.8f), new Vector2(0.3f, 1.4f), wallColor, true, 0);
        CreateBlock("South Vestibule Cap", parent, sprite, new Vector2(0f, -7.5f), new Vector2(3.2f, 0.4f), wallColor, true, 0);

        CreateBlock("West Vestibule Floor", parent, sprite, new Vector2(-10.3f, 0f), new Vector2(1.4f, 2.6f), floorColor, false, -10);
        CreateBlock("West Vestibule Top", parent, sprite, new Vector2(-10.3f, 1.45f), new Vector2(1.4f, 0.3f), wallColor, true, 0);
        CreateBlock("West Vestibule Bottom", parent, sprite, new Vector2(-10.3f, -1.45f), new Vector2(1.4f, 0.3f), wallColor, true, 0);
        CreateBlock("West Vestibule Cap", parent, sprite, new Vector2(-11f, 0f), new Vector2(0.4f, 3.2f), wallColor, true, 0);

        CreateBlock("East Vestibule Floor", parent, sprite, new Vector2(10.3f, 0f), new Vector2(1.4f, 2.6f), floorColor, false, -10);
        CreateBlock("East Vestibule Top", parent, sprite, new Vector2(10.3f, 1.45f), new Vector2(1.4f, 0.3f), wallColor, true, 0);
        CreateBlock("East Vestibule Bottom", parent, sprite, new Vector2(10.3f, -1.45f), new Vector2(1.4f, 0.3f), wallColor, true, 0);
        CreateBlock("East Vestibule Cap", parent, sprite, new Vector2(11f, 0f), new Vector2(0.4f, 3.2f), wallColor, true, 0);
    }

    private static DoorController CreateDoorInstance(
        GameObject prefab,
        Transform parent,
        string name,
        RoomDirection direction,
        Vector2 position,
        Vector2 size)
    {
        GameObject doorObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        doorObject.name = name;
        doorObject.transform.localPosition = position;
        doorObject.transform.localScale = new Vector3(size.x, size.y, 1f);
        DoorController door = doorObject.GetComponent<DoorController>();
        BoxCollider2D[] colliders = doorObject.GetComponents<BoxCollider2D>();
        door.Configure(direction, colliders.First(collider => !collider.isTrigger), colliders.First(collider => collider.isTrigger), doorObject.GetComponent<SpriteRenderer>());
        door.SetLocked(false);
        return door;
    }

    private static Transform CreateMarker(string name, Transform parent, Vector2 localPosition)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent, false);
        marker.transform.localPosition = localPosition;
        return marker.transform;
    }

    private static GameObject CreateHudPrefab(Sprite sprite, Sprite heartSprite, Sprite circleSprite)
    {
        GameObject root = new GameObject("StageAHud", typeof(RectTransform));
        root.layer = LayerMask.NameToLayer("UI");
        root.transform.localScale = Vector3.one;
        StageAHudController hud = root.AddComponent<StageAHudController>();
        MinimapController minimap = root.AddComponent<MinimapController>();
        StageAPauseMenuController pause = root.AddComponent<StageAPauseMenuController>();

        Image rightPanel = CreateImage("Health and Resources", root.transform, sprite, new Color(0.035f, 0.045f, 0.065f, 0.86f));
        SetAnchoredRect(rightPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -32f), new Vector2(390f, 220f));

        RectTransform heartsRoot = CreateUiObject("Hearts", rightPanel.transform).GetComponent<RectTransform>();
        SetAnchoredRect(heartsRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -22f), new Vector2(346f, 58f));
        HorizontalLayoutGroup heartLayout = heartsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        heartLayout.spacing = 8f;
        heartLayout.childAlignment = TextAnchor.MiddleLeft;
        heartLayout.childControlWidth = false;
        heartLayout.childControlHeight = false;
        heartLayout.childForceExpandWidth = false;
        heartLayout.childForceExpandHeight = false;

        Image heartTemplateImage = CreateImage("Heart Template", heartsRoot, heartSprite, new Color(0.22f, 0.24f, 0.28f, 1f));
        heartTemplateImage.rectTransform.sizeDelta = new Vector2(48f, 48f);
        LayoutElement layoutElement = heartTemplateImage.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 48f;
        layoutElement.preferredHeight = 48f;
        Image heartFill = CreateImage("Fill", heartTemplateImage.transform, heartSprite, new Color(0.95f, 0.15f, 0.22f, 1f));
        Stretch(heartFill.rectTransform, 3f, 3f, 3f, 3f);
        heartFill.type = Image.Type.Filled;
        heartFill.fillMethod = Image.FillMethod.Horizontal;
        heartFill.fillOrigin = 0;
        heartTemplateImage.gameObject.SetActive(false);

        Image coinIcon = CreateImage("Coin Icon", rightPanel.transform, circleSprite, new Color(1f, 0.76f, 0.12f));
        SetAnchoredRect(coinIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -90f), new Vector2(32f, 32f));
        Text coinText = CreateText("Coin Count", rightPanel.transform, "0", 25, TextAnchor.MiddleLeft, Color.white);
        SetAnchoredRect(coinText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(66f, -90f), new Vector2(100f, 34f));

        Image bombIcon = CreateImage("Bomb Icon", rightPanel.transform, circleSprite, new Color(0.45f, 0.48f, 0.56f));
        SetAnchoredRect(bombIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -138f), new Vector2(32f, 32f));
        Text bombText = CreateText("Bomb Count", rightPanel.transform, "3", 25, TextAnchor.MiddleLeft, Color.white);
        SetAnchoredRect(bombText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(66f, -138f), new Vector2(100f, 34f));

        Text dashText = CreateText("Dash State", rightPanel.transform, "Dash READY", 21, TextAnchor.UpperRight, new Color(0.4f, 0.82f, 1f));
        SetAnchoredRect(dashText.rectTransform, Vector2.one, Vector2.one, new Vector2(-20f, -112f), new Vector2(160f, 50f));

        Image statsPanel = CreateImage("Player Stats", root.transform, sprite, new Color(0.035f, 0.045f, 0.065f, 0.5f));
        SetAnchoredRect(statsPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -270f), new Vector2(390f, 310f));
        Text statsText = CreateText("Stats", statsPanel.transform, string.Empty, 24, TextAnchor.UpperLeft, Color.white);
        Stretch(statsText.rectTransform, 26f, 24f, 22f, 22f);

        Image minimapPanel = CreateImage("Minimap", root.transform, sprite, new Color(0.035f, 0.045f, 0.065f, 0.78f));
        SetAnchoredRect(minimapPanel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -32f), new Vector2(430f, 250f));
        Text minimapTitle = CreateText("Title", minimapPanel.transform, "MAP    S START   I ITEM   $ SHOP   B BOSS", 16, TextAnchor.MiddleLeft, new Color(0.68f, 0.76f, 0.88f));
        SetAnchoredRect(minimapTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -10f), new Vector2(390f, 30f));
        RectTransform minimapNodes = CreateUiObject("Room Nodes", minimapPanel.transform).GetComponent<RectTransform>();
        SetAnchoredRect(minimapNodes, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(390f, 185f));
        Image minimapConnectionTemplate = CreateImage("Room Connection Template", minimapNodes, sprite, new Color(0.48f, 0.72f, 0.88f, 0.85f));
        SetAnchoredRect(minimapConnectionTemplate.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 5f));
        minimapConnectionTemplate.gameObject.SetActive(false);
        Image minimapNodeTemplate = CreateImage("Room Node Template", minimapNodes, sprite, new Color(0.58f, 0.68f, 0.82f, 0.9f));
        SetAnchoredRect(minimapNodeTemplate.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 22f));
        Text minimapNodeMarker = CreateText("Marker", minimapNodeTemplate.transform, string.Empty, 15, TextAnchor.MiddleCenter, Color.white);
        Stretch(minimapNodeMarker.rectTransform, 0f, 0f, 0f, 0f);
        minimapNodeTemplate.gameObject.SetActive(false);

        Text controls = CreateText("Controls", root.transform, "WASD Move    Mouse Aim    LMB Fire    Space Dash    E Bomb    F Buy    Esc Pause", 23, TextAnchor.MiddleCenter, new Color(0.88f, 0.91f, 0.96f));
        SetAnchoredRect(controls.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(1000f, 42f));
        Text roomLabel = CreateText("Room Label", root.transform, "ERRORGAME DUNGEON     Defeat the boss and discover the hidden room", 20, TextAnchor.MiddleCenter, new Color(0.62f, 0.68f, 0.78f));
        SetAnchoredRect(roomLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(900f, 40f));
        Text prompt = CreateText("Interaction Prompt", root.transform, string.Empty, 27, TextAnchor.MiddleCenter, new Color(0.42f, 1f, 0.58f));
        SetAnchoredRect(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(0f, -150f), new Vector2(500f, 48f));

        Image death = CreateImage("Death Panel", root.transform, sprite, new Color(0.07f, 0.015f, 0.02f, 0.9f));
        Stretch(death.rectTransform, 0f, 0f, 0f, 0f);
        Text deathTitle = CreateText("Title", death.transform, "YOU DIED\nPress R to restart", 54, TextAnchor.MiddleCenter, new Color(1f, 0.35f, 0.38f));
        Stretch(deathTitle.rectTransform, 0f, 0f, 0f, 0f);
        death.gameObject.SetActive(false);

        Image pauseOverlay = CreateImage("Pause Menu", root.transform, sprite, new Color(0.025f, 0.035f, 0.055f, 0.96f));
        Stretch(pauseOverlay.rectTransform, 0f, 0f, 0f, 0f);
        Text pauseTitle = CreateText("Pause Title", pauseOverlay.transform, "PAUSED", 50, TextAnchor.MiddleCenter, Color.white);
        SetAnchoredRect(pauseTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(500f, 70f));

        Button items = CreateButton("Items Tab", pauseOverlay.transform, sprite, "1  ITEMS");
        Button notebook = CreateButton("Notebook Tab", pauseOverlay.transform, sprite, "2  NOTEBOOK");
        Button log = CreateButton("Log Tab", pauseOverlay.transform, sprite, "3  LOG");
        SetAnchoredRect(items.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-260f, -135f), new Vector2(230f, 58f));
        SetAnchoredRect(notebook.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -135f), new Vector2(230f, 58f));
        SetAnchoredRect(log.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(260f, -135f), new Vector2(230f, 58f));

        Image contentPanel = CreateImage("Tab Content", pauseOverlay.transform, sprite, new Color(0.075f, 0.09f, 0.13f, 1f));
        SetAnchoredRect(contentPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1100f, 550f));
        Text tabContent = CreateText("Content", contentPanel.transform, string.Empty, 30, TextAnchor.MiddleCenter, new Color(0.75f, 0.8f, 0.9f));
        Stretch(tabContent.rectTransform, 40f, 40f, 40f, 40f);

        Text pauseHint = CreateText("Pause Hint", pauseOverlay.transform, "Esc  Resume", 23, TextAnchor.MiddleRight, new Color(0.65f, 0.72f, 0.82f));
        SetAnchoredRect(pauseHint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-35f, 28f), new Vector2(300f, 50f));
        Button mainMenu = CreateButton("Return To Main Menu", pauseOverlay.transform, sprite, "MAIN MENU");
        SetAnchoredRect(mainMenu.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(35f, 28f), new Vector2(260f, 58f));
        pauseOverlay.gameObject.SetActive(false);

        pause.Configure(pauseOverlay.gameObject, tabContent, items, notebook, log, mainMenu);
        hud.Configure(heartsRoot, heartTemplateImage.rectTransform, coinText, bombText, prompt, dashText, statsText, death.gameObject, pause);
        minimap.Configure(minimapNodes, minimapNodeTemplate, minimapConnectionTemplate);

        // The HUD prefab contains content only. A real scene Canvas drives screen
        // dimensions, while this root simply stretches to that parent Canvas.
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.localScale = Vector3.one;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void CreateGameScene(GameObject playerPrefab, GameObject dungeonRoomPrefab, GameObject hudPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Game";

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 8f;
        camera.backgroundColor = new Color(0.055f, 0.065f, 0.085f);
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<UniversalAdditionalCameraData>();

        GameObject lightObject = new GameObject("Global Light 2D");
        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;

        GameObject playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        playerObject.transform.position = Vector3.zero;

        Transform roomRoot = new GameObject("Dungeon Rooms").transform;

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.localScale = Vector3.one;

        GameObject hudObject = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, canvasObject.transform);
        RectTransform hudRect = hudObject.GetComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.pivot = new Vector2(0.5f, 0.5f);
        hudRect.anchoredPosition = Vector2.zero;
        hudRect.sizeDelta = Vector2.zero;
        hudRect.localScale = Vector3.one;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();

        GameObject gameSessionObject = new GameObject("Game Session");
        GameSession gameSession = gameSessionObject.AddComponent<GameSession>();
        gameSession.Configure(
            playerObject.GetComponent<Player>(),
            hudObject.GetComponent<StageAHudController>(),
            hudObject.GetComponent<MinimapController>(),
            camera,
            dungeonRoomPrefab.GetComponent<RoomController>(),
            roomRoot,
            false,
            20260720);

        EditorSceneManager.SaveScene(scene, GameScenePath);
        EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
    }

    private static void CreateMainMenuScene(Sprite sprite)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.backgroundColor = new Color(0.018f, 0.024f, 0.038f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Image background = CreateImage("Background", canvasObject.transform, sprite, new Color(0.018f, 0.024f, 0.038f, 1f));
        Stretch(background.rectTransform, 0f, 0f, 0f, 0f);
        Image accent = CreateImage("Accent", background.transform, sprite, new Color(0.12f, 0.58f, 0.78f, 0.22f));
        SetAnchoredRect(accent.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 185f), new Vector2(760f, 8f));

        Text title = CreateText("Game Title", background.transform, "ErrorGame", 92, TextAnchor.MiddleCenter, new Color(0.86f, 0.94f, 1f));
        SetAnchoredRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 285f), new Vector2(900f, 130f));
        Text subtitle = CreateText("Subtitle", background.transform, "SINGLE-FLOOR DUNGEON", 22, TextAnchor.MiddleCenter, new Color(0.42f, 0.66f, 0.82f));
        SetAnchoredRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(600f, 44f));

        Button startButton = CreateButton("Start Game", background.transform, sprite, "START GAME");
        Button continueButton = CreateButton("Continue", background.transform, sprite, "CONTINUE");
        Button quitButton = CreateButton("Quit Game", background.transform, sprite, "QUIT GAME");
        SetAnchoredRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(420f, 76f));
        SetAnchoredRect(continueButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(420f, 76f));
        SetAnchoredRect(quitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(420f, 76f));

        Text saveHint = CreateText("Save Hint", background.transform, "Progress is saved automatically", 19, TextAnchor.MiddleCenter, new Color(0.48f, 0.54f, 0.64f));
        SetAnchoredRect(saveHint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 45f), new Vector2(600f, 36f));

        MainMenuController menu = canvasObject.AddComponent<MainMenuController>();
        menu.Configure(startButton, continueButton, quitButton);

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
    }

    private static void ConfigureBuildScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
    }

    private static GameObject CreateBlock(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, Color color, bool addCollider, int sortingOrder)
    {
        GameObject block = CreateWorldVisual(name, parent, sprite, position, size, color, sortingOrder);
        if (addCollider) block.AddComponent<BoxCollider2D>();
        return block;
    }

    private static GameObject CreateWorldVisual(string name, Transform parent, Sprite sprite, Vector2 localPosition, Vector2 size, Color color, int sortingOrder)
    {
        GameObject visual = new GameObject(name);
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return visual;
    }

    private static TextMesh CreateWorldLabel(
        string name,
        Transform parent,
        string value,
        Vector2 localPosition,
        float characterSize,
        Color color,
        int sortingOrder)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = localPosition;
        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = value;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = characterSize;
        label.fontSize = 64;
        label.color = color;
        labelObject.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return label;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = parent.gameObject.layer;
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localScale = Vector3.one;
        return gameObject;
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject gameObject = CreateUiObject(name, parent);
        Image image = gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor alignment, Color color)
    {
        Text text = CreateUiObject(name, parent).AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.text = value;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, Sprite sprite, string label)
    {
        Image image = CreateImage(name, parent, sprite, new Color(0.18f, 0.21f, 0.28f, 0.98f));
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.28f, 0.5f, 0.72f, 1f);
        colors.pressedColor = new Color(0.16f, 0.4f, 0.64f, 1f);
        button.colors = colors;
        Text text = CreateText("Label", button.transform, label, 22, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
        return button;
    }

    private static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
