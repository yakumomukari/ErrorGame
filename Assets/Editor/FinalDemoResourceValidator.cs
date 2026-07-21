using System.Collections.Generic;
using System.Linq;
using InvalidOperationException = System.InvalidOperationException;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class FinalDemoResourceValidator
{
    private const string BombPrefabPath = "Assets/Prefabs/Combat/PlayerBomb.prefab";
    private const string EnemyProjectilePrefabPath = "Assets/Prefabs/Combat/EnemyProjectile.prefab";
    private const string EnemyPrefabPath = "Assets/Prefabs/Enemies/MeleeEnemy.prefab";
    private const string RangedEnemyPrefabPath = "Assets/Prefabs/Enemies/RangedEnemy.prefab";
    private const string BossPrefabPath = "Assets/Prefabs/Enemies/BossEnemy.prefab";
    private const string DungeonRoomPrefabPath = "Assets/Prefabs/Rooms/DungeonRoom.prefab";
    private const string CoinPickupPrefabPath = "Assets/Prefabs/Pickups/CoinPickup.prefab";
    private const string HeartPickupPrefabPath = "Assets/Prefabs/Pickups/HeartPickup.prefab";
    private const string BombPickupPrefabPath = "Assets/Prefabs/Pickups/BombPickup.prefab";
    private const string NormalUpgradePrefabPath = "Assets/Prefabs/Pickups/NormalUpgradePickup.prefab";
    private const string SuperMushroomPrefabPath = "Assets/Prefabs/Pickups/SuperMushroom.prefab";
    private const string ShopProductPrefabPath = "Assets/Prefabs/Pickups/ShopProduct.prefab";
    private const string HudPrefabPath = "Assets/Prefabs/UI/StageAHud.prefab";
    private const string MainMenuScenePath = DemoSceneFactory.MainMenuScenePath;
    private const string GameScenePath = DemoSceneFactory.GameScenePath;

    [MenuItem("Tools/Error Game/Validate Final Demo Resources")]
    public static void ValidateFromMenu()
    {
        ValidateGeneratedResources();
    }

    public static void ValidateGeneratedResources()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException("Exit Play Mode before validating project resources.");
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            throw new InvalidOperationException("Resource validation was cancelled to preserve unsaved scene changes.");
        }

        SceneSetup[] previousSceneSetup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            ValidateGeneratedResourcesCore();
        }
        finally
        {
            if (previousSceneSetup != null && previousSceneSetup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSceneSetup);
            }
        }
    }

    private static void ValidateGeneratedResourcesCore()
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

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            throw new InvalidOperationException("Player prefab is missing PlayerStats.");
        }
        SerializedObject statsObject = new SerializedObject(playerStats);
        if (Mathf.Abs(statsObject.FindProperty("fireRate").floatValue - PlayerStats.DefaultFireRate) > 0.001f ||
            Mathf.Abs(statsObject.FindProperty("maximumFireRate").floatValue - PlayerStats.DefaultMaximumFireRate) > 0.001f)
        {
            throw new InvalidOperationException("Player fire rate must start at 2 and be capped at 6.");
        }

        GameObject statsProbeObject = new GameObject("Player Stats Validation Probe");
        PlayerStats statsProbe = statsProbeObject.AddComponent<PlayerStats>();
        statsProbe.ConfigureFireRate(PlayerStats.DefaultFireRate, PlayerStats.DefaultMaximumFireRate);
        statsProbe.AddFireRate(100f);
        bool fireRateCapFailed = Mathf.Abs(statsProbe.FireRate - PlayerStats.DefaultMaximumFireRate) > 0.001f;
        statsProbe.Restore(5f, 100f, 1f, 8f, 12f, 0f);
        fireRateCapFailed |= Mathf.Abs(statsProbe.FireRate - PlayerStats.DefaultMaximumFireRate) > 0.001f;
        Object.DestroyImmediate(statsProbeObject);
        if (fireRateCapFailed)
        {
            throw new InvalidOperationException("PlayerStats did not enforce the fire-rate cap during upgrades or restore.");
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
        string[] sessionReferences =
        {
            "player", "hud", "minimap", "gameCamera", "roomPrefab", "roomPrefabCatalog", "roomRoot"
        };
        if (sessionReferences.Any(property => sessionObject.FindProperty(property).objectReferenceValue == null) ||
            sessionObject.FindProperty("useFixedSeed").boolValue)
        {
            throw new InvalidOperationException("GameSession references are incomplete or the generated scene is not configured for random runs.");
        }

        RoomPrefabCatalog roomPrefabCatalog = AssetDatabase.LoadAssetAtPath<RoomPrefabCatalog>(
            DemoDataAssetFactory.RoomPrefabCatalogPath);
        if (roomPrefabCatalog == null || session.RoomPrefabs != roomPrefabCatalog)
        {
            throw new InvalidOperationException("GameSession is missing the generated room prefab catalog.");
        }
        ValidateRoomPrefabCatalog(roomPrefabCatalog);

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
            encounter.SpawnCatalog == null ||
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
            bossEncounterObject.FindProperty("rewardPrefab").objectReferenceValue == null ||
            bossEncounter.RewardSet == null)
        {
            throw new InvalidOperationException("Boss encounter prefab references are incomplete.");
        }

        BossFloorPortal floorPortal = dungeonRoomPrefab.GetComponentInChildren<BossFloorPortal>(true);
        if (floorPortal == null || floorPortal.Visual == null || floorPortal.PortalTrigger == null ||
            !floorPortal.PortalTrigger.isTrigger || floorPortal.transform.localPosition.y <= 0f)
        {
            throw new InvalidOperationException(
                "Dungeon room prefab requires a configured next-floor portal above the Boss arena.");
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
        PlayerEffectSet normalUpgradeSet = itemRoom.UpgradeSet;
        if (itemRoomObject.FindProperty("pickupPrefab").objectReferenceValue == null ||
            itemRoomObject.FindProperty("spawnPoint").objectReferenceValue == null ||
            normalUpgradeSet == null || normalUpgradeSet.Count != 6 ||
            bossEncounter.RewardSet != normalUpgradeSet ||
            normalUpgradeSet.Effects.Any(effect => effect == null ||
                string.IsNullOrWhiteSpace(effect.StableId) || effect.Modifiers.Count == 0) ||
            shopObject.FindProperty("productPrefab").objectReferenceValue == null ||
            shop.ProductSet == null || shop.ProductSet.Count != 7 ||
            shop.ProductSet.Products.Any(product => product == null ||
                string.IsNullOrWhiteSpace(product.StableId) || product.Price < 0 || product.Effect == null) ||
            !shop.ProductSet.Products.Select(product => product.Price)
                .SequenceEqual(new[] { 1, 2, 5, 4, 4, 3, 2 }) ||
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

        bool invalidVisibleSpecialTerminal = Enumerable.Range(0, 256).Any(seed =>
        {
            DungeonLayout layout = generator.Generate(seed, 8, 12);
            return layout.VisibleRooms
                .Where(roomNode => RoomTypeUtility.IsVisibleSpecialRoom(roomNode.Type))
                .Any(roomNode =>
                {
                    if (roomNode.Connections.Count != 1) return true;
                    RoomDirection entrance = roomNode.Connections.Single();
                    return !layout.TryGetConnectedRoom(roomNode, entrance, out RoomNode neighbor) ||
                           neighbor.Type != RoomType.Combat;
                });
        });
        if (invalidVisibleSpecialTerminal)
        {
            throw new InvalidOperationException(
                "Every visible special room must be a one-door terminal connected to Combat.");
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
            floorNumber = 3,
            currentRoomX = 2,
            currentRoomY = -1,
            player = new PlayerSaveData
            {
                currentHealthUnits = 5,
                maxHealthUnits = 8,
                coins = 7,
                bombs = 2,
                moveSpeed = 6.25f,
                fireRate = 6f,
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
            collectedSecretRewards = new List<int> { 1 },
            roomVariantId = "combat_default"
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
            saveRoundTrip.floorNumber != 3 ||
            saveRoundTrip.player.coins != 7 ||
            saveRoundTrip.rooms.Count != 4 ||
            saveRoundTrip.rooms[0].purchasedShopSlots.Count != 2 ||
            saveRoundTrip.rooms[0].roomVariantId != "combat_default" ||
            saveRoundTrip.openedSecretPassages.Count != 1)
        {
            throw new InvalidOperationException("Game save data failed its JSON round-trip validation.");
        }

        GameSaveData pendingFloorSave = new GameSaveData
        {
            dungeonSeed = 20260721,
            floorNumber = 4,
            beginsAtFloorStart = true,
            player = saveSample.player
        };
        if (!saveRepository.IsValid(saveRepository.Deserialize(saveRepository.Serialize(pendingFloorSave))))
        {
            throw new InvalidOperationException("Next-floor save data failed validation.");
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

    private static void ValidateRoomPrefabCatalog(RoomPrefabCatalog catalog)
    {
        List<RoomPrefabEntry> entries = catalog.Entries.Where(entry => entry != null).ToList();
        if (entries.Count == 0 || entries.Any(entry =>
                string.IsNullOrWhiteSpace(entry.StableId) || entry.Prefab == null || entry.Weight <= 0))
        {
            throw new InvalidOperationException("Room prefab catalog contains an invalid entry.");
        }

        HashSet<string> uniqueIds = new HashSet<string>();
        foreach (RoomPrefabEntry entry in entries)
        {
            string key = ((int)entry.RoomType) + ":" + entry.StableId;
            if (!uniqueIds.Add(key))
            {
                throw new InvalidOperationException($"Room prefab catalog contains duplicate ID '{entry.StableId}' for {entry.RoomType}.");
            }
        }

        foreach (RoomType roomType in System.Enum.GetValues(typeof(RoomType)))
        {
            if (!entries.Any(entry => entry.RoomType == roomType) ||
                !catalog.TrySelect(roomType, 20260721, out RoomPrefabEntry firstSelection) ||
                !catalog.TrySelect(roomType, 20260721, out RoomPrefabEntry repeatedSelection) ||
                firstSelection.StableId != repeatedSelection.StableId)
            {
                throw new InvalidOperationException(
                    $"Room prefab catalog has no deterministic prefab selection for {roomType} rooms.");
            }
        }

        if (entries
            .Where(entry => entry.RoomType == RoomType.Boss)
            .Any(entry => entry.Prefab.GetComponentInChildren<BossFloorPortal>(true) == null))
        {
            throw new InvalidOperationException(
                "Every configured Boss-room prefab must contain a next-floor portal.");
        }

        string[] initialSpecialRoomPaths =
        {
            DemoPrefabFactory.ItemRoomDefaultPrefabPath,
            DemoPrefabFactory.ShopRoomDefaultPrefabPath,
            DemoPrefabFactory.BossRoomDefaultPrefabPath,
            DemoPrefabFactory.SecretRoomDefaultPrefabPath,
            DemoPrefabFactory.SuperSecretRoomDefaultPrefabPath
        };
        if (initialSpecialRoomPaths.Any(path =>
                AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<RoomController>() == null))
        {
            throw new InvalidOperationException("One or more initial special-room prefabs are missing RoomController.");
        }
    }
}
