using InvalidOperationException = System.InvalidOperationException;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[InitializeOnLoad]
public static class StageAProjectBuilder
{
    private const string InputActionsPath = "Assets/Scripts/InputSystem.inputactions";

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

        bool missing = !DemoPrefabFactory.AreGeneratedPrefabsAvailable() ||
                       AssetDatabase.LoadAssetAtPath<EnemySpawnCatalog>(DemoDataAssetFactory.EnemySpawnCatalogPath) == null ||
                       AssetDatabase.LoadAssetAtPath<PlayerEffectSet>(DemoDataAssetFactory.NormalUpgradeSetPath) == null ||
                       AssetDatabase.LoadAssetAtPath<ShopProductSet>(DemoDataAssetFactory.ShopProductSetPath) == null ||
                       AssetDatabase.LoadAssetAtPath<RoomPrefabCatalog>(DemoDataAssetFactory.RoomPrefabCatalogPath) == null ||
                       !DemoSceneFactory.AreGeneratedScenesAvailable();
        if (missing)
        {
            BuildProject(false);
            return;
        }

        RoomPrefabCatalog roomCatalog = AssetDatabase.LoadAssetAtPath<RoomPrefabCatalog>(
            DemoDataAssetFactory.RoomPrefabCatalogPath);
        if (!DemoPrefabFactory.AreBossFloorPortalsInstalled(roomCatalog))
        {
            InstallBossFloorPortal(false);
        }
    }

    private static void BuildProject(bool showDialog)
    {
        PlayerSettings.productName = "ErrorGame";
        DemoDataAssetFactory.EnsureProjectFolders();
        Sprite whiteSprite = DemoDataAssetFactory.CreateOrLoadWhiteSprite();
        Sprite heartSprite = DemoDataAssetFactory.CreateOrLoadHeartSprite();
        Sprite circleSprite = DemoDataAssetFactory.CreateOrLoadCircleSprite();
        Projectile projectilePrefab = DemoPrefabFactory.CreateProjectilePrefab(whiteSprite);
        EnemyProjectile enemyProjectilePrefab = DemoPrefabFactory.CreateEnemyProjectilePrefab(circleSprite);
        Bomb bombPrefab = DemoPrefabFactory.CreateBombPrefab(circleSprite, whiteSprite);
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        if (inputActions == null) throw new InvalidOperationException("Input actions asset is missing.");
        GameObject playerPrefab = DemoPrefabFactory.CreatePlayerPrefab(whiteSprite, projectilePrefab, bombPrefab, inputActions);
        MeleeEnemy enemyPrefab = DemoPrefabFactory.CreateEnemyPrefab(whiteSprite);
        RangedEnemy rangedEnemyPrefab = DemoPrefabFactory.CreateRangedEnemyPrefab(circleSprite, whiteSprite, enemyProjectilePrefab);
        EnemySpawnTable combatEnemySpawnTable = new EnemySpawnTable(new[]
        {
            new EnemySpawnEntry(enemyPrefab, 2),
            new EnemySpawnEntry(rangedEnemyPrefab, 1)
        });
        EnemySpawnCatalog combatEnemySpawnCatalog = DemoDataAssetFactory.CreateOrUpdateEnemySpawnCatalog(combatEnemySpawnTable.Entries);
        PlayerEffectSet normalUpgradeSet = DemoDataAssetFactory.CreateOrUpdateNormalUpgradeSet();
        ShopProductSet shopProductSet = DemoDataAssetFactory.CreateOrUpdateShopProductSet(normalUpgradeSet);
        BossEnemy bossPrefab = DemoPrefabFactory.CreateBossPrefab(circleSprite, whiteSprite);
        GameObject doorPrefab = DemoPrefabFactory.CreateDoorPrefab(whiteSprite);
        BasicResourcePickup coinPickup = DemoPrefabFactory.CreatePickupPrefab(DemoPrefabFactory.CoinPickupPrefabPath, "Coin Pickup", circleSprite, new Color(1f, 0.76f, 0.12f), BasicResourceType.Coin, 3);
        BasicResourcePickup heartPickup = DemoPrefabFactory.CreatePickupPrefab(DemoPrefabFactory.HeartPickupPrefabPath, "Heart Pickup", heartSprite, new Color(0.95f, 0.15f, 0.22f), BasicResourceType.HalfHeart);
        BasicResourcePickup bombPickup = DemoPrefabFactory.CreatePickupPrefab(DemoPrefabFactory.BombPickupPrefabPath, "Bomb Pickup", circleSprite, new Color(0.4f, 0.44f, 0.52f), BasicResourceType.Bomb);
        NormalUpgradePickup upgradePickup = DemoPrefabFactory.CreateNormalUpgradePrefab(circleSprite);
        SuperMushroomPickup superMushroom = DemoPrefabFactory.CreateSuperMushroomPrefab(circleSprite, whiteSprite);
        ShopProduct shopProduct = DemoPrefabFactory.CreateShopProductPrefab(whiteSprite);
        GameObject dungeonRoomPrefab = DemoPrefabFactory.CreateDungeonRoomPrefab(
            whiteSprite,
            circleSprite,
            doorPrefab,
            combatEnemySpawnTable,
            combatEnemySpawnCatalog,
            bossPrefab,
            new[] { coinPickup, heartPickup, bombPickup },
            upgradePickup,
            normalUpgradeSet,
            superMushroom,
            shopProduct,
            shopProductSet);
        RoomPrefabEntry[] defaultRoomPrefabs = DemoPrefabFactory.CreateOrLoadInitialRoomPrefabs(dungeonRoomPrefab);
        RoomPrefabCatalog roomPrefabCatalog = DemoDataAssetFactory.CreateOrUpdateRoomPrefabCatalog(defaultRoomPrefabs);
        DemoPrefabFactory.EnsureBossFloorPortals(circleSprite, roomPrefabCatalog);
        GameObject hudPrefab = DemoPrefabFactory.CreateHudPrefab(whiteSprite, heartSprite, circleSprite);
        DemoSceneFactory.CreateOrUpdateScenes(
            playerPrefab,
            dungeonRoomPrefab,
            roomPrefabCatalog,
            hudPrefab,
            whiteSprite);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        FinalDemoResourceValidator.ValidateGeneratedResources();
        Debug.Log("ErrorGame final demo resources rebuilt: main menu, save/continue flow, gameplay systems, prefabs, and scenes.");
        if (showDialog)
        {
            EditorUtility.DisplayDialog("ErrorGame", "Final demo resources rebuilt successfully.", "OK");
        }
    }

    public static void InstallBossFloorPortalFromCommandLine()
    {
        InstallBossFloorPortal(true);
    }

    private static void InstallBossFloorPortal(bool validate)
    {
        DemoDataAssetFactory.EnsureProjectFolders();
        Sprite circleSprite = DemoDataAssetFactory.CreateOrLoadCircleSprite();
        RoomPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<RoomPrefabCatalog>(
            DemoDataAssetFactory.RoomPrefabCatalogPath);
        DemoPrefabFactory.EnsureBossFloorPortals(circleSprite, catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (validate) FinalDemoResourceValidator.ValidateGeneratedResources();
        Debug.Log(validate
            ? "Boss next-floor portal installed and validated."
            : "Boss next-floor portal installed; validation is available from the Tools/Error Game menu.");
    }
}
