using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class DemoPrefabFactory
{
    private const string PrefabPlayerFolder = "Assets/Prefabs/Player";
    private const string PrefabCombatFolder = "Assets/Prefabs/Combat";
    private const string PrefabEnemyFolder = "Assets/Prefabs/Enemies";
    private const string PrefabRoomFolder = "Assets/Prefabs/Rooms";
    private const string PrefabDoorFolder = "Assets/Prefabs/Doors";
    private const string PrefabPickupFolder = "Assets/Prefabs/Pickups";
    private const string PrefabUiFolder = "Assets/Prefabs/UI";
    private const string PrefabRoomVariantFolder = PrefabRoomFolder + "/Variants";

    public const string ProjectilePrefabPath = PrefabCombatFolder + "/PlayerProjectile.prefab";
    public const string EnemyProjectilePrefabPath = PrefabCombatFolder + "/EnemyProjectile.prefab";
    public const string BombPrefabPath = PrefabCombatFolder + "/PlayerBomb.prefab";
    public const string PlayerPrefabPath = PrefabPlayerFolder + "/Player.prefab";
    public const string EnemyPrefabPath = PrefabEnemyFolder + "/MeleeEnemy.prefab";
    public const string RangedEnemyPrefabPath = PrefabEnemyFolder + "/RangedEnemy.prefab";
    public const string BossPrefabPath = PrefabEnemyFolder + "/BossEnemy.prefab";
    public const string DoorPrefabPath = PrefabDoorFolder + "/CombatDoor.prefab";
    public const string DungeonRoomPrefabPath = PrefabRoomFolder + "/DungeonRoom.prefab";
    public const string CoinPickupPrefabPath = PrefabPickupFolder + "/CoinPickup.prefab";
    public const string HeartPickupPrefabPath = PrefabPickupFolder + "/HeartPickup.prefab";
    public const string BombPickupPrefabPath = PrefabPickupFolder + "/BombPickup.prefab";
    public const string NormalUpgradePrefabPath = PrefabPickupFolder + "/NormalUpgradePickup.prefab";
    public const string SuperMushroomPrefabPath = PrefabPickupFolder + "/SuperMushroom.prefab";
    public const string ShopProductPrefabPath = PrefabPickupFolder + "/ShopProduct.prefab";
    public const string HudPrefabPath = PrefabUiFolder + "/StageAHud.prefab";
    public const string ItemRoomDefaultPrefabPath = PrefabRoomVariantFolder + "/ItemRoom_Default.prefab";
    public const string ShopRoomDefaultPrefabPath = PrefabRoomVariantFolder + "/ShopRoom_Default.prefab";
    public const string BossRoomDefaultPrefabPath = PrefabRoomVariantFolder + "/BossRoom_Default.prefab";
    public const string SecretRoomDefaultPrefabPath = PrefabRoomVariantFolder + "/SecretRoom_Default.prefab";
    public const string SuperSecretRoomDefaultPrefabPath = PrefabRoomVariantFolder + "/SuperSecretRoom_Default.prefab";

    private static readonly string[] GeneratedPrefabPaths =
    {
        ProjectilePrefabPath,
        EnemyProjectilePrefabPath,
        BombPrefabPath,
        PlayerPrefabPath,
        EnemyPrefabPath,
        RangedEnemyPrefabPath,
        BossPrefabPath,
        DoorPrefabPath,
        DungeonRoomPrefabPath,
        CoinPickupPrefabPath,
        HeartPickupPrefabPath,
        BombPickupPrefabPath,
        NormalUpgradePrefabPath,
        SuperMushroomPrefabPath,
        ShopProductPrefabPath,
        HudPrefabPath,
        ItemRoomDefaultPrefabPath,
        ShopRoomDefaultPrefabPath,
        BossRoomDefaultPrefabPath,
        SecretRoomDefaultPrefabPath,
        SuperSecretRoomDefaultPrefabPath
    };

    public static bool AreGeneratedPrefabsAvailable()
    {
        return GeneratedPrefabPaths.All(path => AssetDatabase.LoadAssetAtPath<GameObject>(path) != null);
    }

    public static Projectile CreateProjectilePrefab(Sprite sprite)
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

    public static EnemyProjectile CreateEnemyProjectilePrefab(Sprite sprite)
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

    public static Bomb CreateBombPrefab(Sprite circleSprite, Sprite whiteSprite)
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

    public static GameObject CreatePlayerPrefab(
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

        PlayerStats playerStats = root.AddComponent<PlayerStats>();
        playerStats.ConfigureFireRate(PlayerStats.DefaultFireRate, PlayerStats.DefaultMaximumFireRate);
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

    public static MeleeEnemy CreateEnemyPrefab(Sprite sprite)
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

    public static RangedEnemy CreateRangedEnemyPrefab(
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

    public static BossEnemy CreateBossPrefab(Sprite circleSprite, Sprite detailSprite)
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

    public static GameObject CreateDoorPrefab(Sprite sprite)
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

    public static BasicResourcePickup CreatePickupPrefab(
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
    public static NormalUpgradePickup CreateNormalUpgradePrefab(Sprite sprite)
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

    public static SuperMushroomPickup CreateSuperMushroomPrefab(Sprite circleSprite, Sprite whiteSprite)
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

    public static ShopProduct CreateShopProductPrefab(Sprite sprite)
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

    public static GameObject CreateDungeonRoomPrefab(
        Sprite sprite,
        Sprite portalSprite,
        GameObject doorPrefab,
        EnemySpawnTable enemySpawnTable,
        EnemySpawnCatalog enemySpawnCatalog,
        BossEnemy bossPrefab,
        BasicResourcePickup[] rewardPrefabs,
        NormalUpgradePickup upgradePickupPrefab,
        PlayerEffectSet normalUpgradeSet,
        SuperMushroomPickup superMushroomPrefab,
        ShopProduct shopProductPrefab,
        ShopProductSet shopProductSet)
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
        CreateBossFloorPortal(root.transform, portalSprite != null ? portalSprite : sprite);

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
        encounter.SetSpawnCatalog(enemySpawnCatalog);
        bossEncounter.Configure(bossSpawnPoint, bossPrefab, rewardPoint, upgradePickupPrefab);
        bossEncounter.SetRewardSet(normalUpgradeSet);
        itemRoom.Configure(upgradePickupPrefab, itemSpawnPoint);
        itemRoom.SetUpgradeSet(normalUpgradeSet);
        shop.Configure(shopProductPrefab, shopSlotPoints);
        shop.SetProductSet(shopProductSet);
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

    public static RoomPrefabEntry[] CreateOrLoadInitialRoomPrefabs(GameObject baseRoomPrefab)
    {
        RoomController baseRoom = baseRoomPrefab != null
            ? baseRoomPrefab.GetComponent<RoomController>()
            : null;
        if (baseRoom == null)
        {
            throw new System.InvalidOperationException("The base dungeon room prefab is missing RoomController.");
        }

        return new[]
        {
            new RoomPrefabEntry("start_default", RoomType.Start, baseRoom, 1),
            new RoomPrefabEntry("combat_default", RoomType.Combat, baseRoom, 1),
            new RoomPrefabEntry(
                "item_default",
                RoomType.Item,
                CreateOrLoadRoomVariant(baseRoomPrefab, ItemRoomDefaultPrefabPath, "ItemRoom_Default"),
                1),
            new RoomPrefabEntry(
                "shop_default",
                RoomType.Shop,
                CreateOrLoadRoomVariant(baseRoomPrefab, ShopRoomDefaultPrefabPath, "ShopRoom_Default"),
                1),
            new RoomPrefabEntry(
                "boss_default",
                RoomType.Boss,
                CreateOrLoadRoomVariant(baseRoomPrefab, BossRoomDefaultPrefabPath, "BossRoom_Default"),
                1),
            new RoomPrefabEntry(
                "secret_default",
                RoomType.Secret,
                CreateOrLoadRoomVariant(baseRoomPrefab, SecretRoomDefaultPrefabPath, "SecretRoom_Default"),
                1),
            new RoomPrefabEntry(
                "super_secret_default",
                RoomType.SuperSecret,
                CreateOrLoadRoomVariant(baseRoomPrefab, SuperSecretRoomDefaultPrefabPath, "SuperSecretRoom_Default"),
                1)
        };
    }

    public static void EnsureBossFloorPortals(Sprite portalSprite, RoomPrefabCatalog catalog)
    {
        HashSet<string> prefabPaths = new HashSet<string> { DungeonRoomPrefabPath };
        if (catalog != null)
        {
            foreach (RoomPrefabEntry entry in catalog.Entries)
            {
                if (entry == null || entry.RoomType != RoomType.Boss || entry.Prefab == null) continue;
                string path = AssetDatabase.GetAssetPath(entry.Prefab.gameObject);
                if (!string.IsNullOrWhiteSpace(path)) prefabPaths.Add(path);
            }
        }

        foreach (string path in prefabPaths) EnsureBossFloorPortal(path, portalSprite);
    }

    public static bool AreBossFloorPortalsInstalled(RoomPrefabCatalog catalog)
    {
        GameObject baseRoom = AssetDatabase.LoadAssetAtPath<GameObject>(DungeonRoomPrefabPath);
        if (baseRoom == null || baseRoom.GetComponentInChildren<BossFloorPortal>(true) == null) return false;
        if (catalog == null) return false;

        return catalog.Entries
            .Where(entry => entry != null && entry.RoomType == RoomType.Boss && entry.Prefab != null)
            .All(entry => entry.Prefab.GetComponentInChildren<BossFloorPortal>(true) != null);
    }

    private static void EnsureBossFloorPortal(string prefabPath, Sprite portalSprite)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null || prefab.GetComponentInChildren<BossFloorPortal>(true) != null) return;

        GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            CreateBossFloorPortal(contents.transform, portalSprite);
            PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static BossFloorPortal CreateBossFloorPortal(Transform parent, Sprite portalSprite)
    {
        GameObject portalRoot = new GameObject("Next Floor Portal");
        portalRoot.transform.SetParent(parent, false);
        portalRoot.transform.localPosition = new Vector3(0f, 4.25f, 0f);

        SpriteRenderer visual = CreateWorldVisual(
            "Portal Visual",
            portalRoot.transform,
            portalSprite,
            Vector2.zero,
            new Vector2(2.7f, 0.9f),
            new Color(0.12f, 0.82f, 1f, 0.82f),
            3).GetComponent<SpriteRenderer>();
        CreateWorldVisual(
            "Portal Core",
            portalRoot.transform,
            portalSprite,
            Vector2.zero,
            new Vector2(1.75f, 0.46f),
            new Color(0.82f, 0.98f, 1f, 0.9f),
            4);
        TextMesh label = CreateWorldLabel(
            "Portal Label",
            portalRoot.transform,
            "NEXT FLOOR",
            new Vector2(0f, 0.95f),
            0.075f,
            new Color(0.7f, 0.94f, 1f),
            5);

        BoxCollider2D trigger = portalRoot.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(2.8f, 1.35f);
        BossFloorPortal portal = portalRoot.AddComponent<BossFloorPortal>();
        portal.Configure(visual, trigger, label);
        return portal;
    }

    private static RoomController CreateOrLoadRoomVariant(
        GameObject baseRoomPrefab,
        string path,
        string rootName)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            RoomController existingRoom = existing.GetComponent<RoomController>();
            if (existingRoom == null)
            {
                throw new System.InvalidOperationException($"Room prefab at {path} is missing RoomController.");
            }
            return existingRoom;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(baseRoomPrefab);
        instance.name = rootName;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab.GetComponent<RoomController>();
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
    public static GameObject CreateHudPrefab(Sprite sprite, Sprite heartSprite, Sprite circleSprite)
    {
        GameObject root = new GameObject("StageAHud", typeof(RectTransform));
        root.layer = LayerMask.NameToLayer("UI");
        root.transform.localScale = Vector3.one;
        StageAHudController hud = root.AddComponent<StageAHudController>();
        MinimapController minimap = root.AddComponent<MinimapController>();
        StageAPauseMenuController pause = root.AddComponent<StageAPauseMenuController>();

        Image rightPanel = DemoUiFactory.CreateImage("Health and Resources", root.transform, sprite, new Color(0.035f, 0.045f, 0.065f, 0.86f));
        DemoUiFactory.SetAnchoredRect(rightPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -32f), new Vector2(390f, 220f));

        RectTransform heartsRoot = DemoUiFactory.CreateUiObject("Hearts", rightPanel.transform).GetComponent<RectTransform>();
        DemoUiFactory.SetAnchoredRect(heartsRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -22f), new Vector2(346f, 58f));
        HorizontalLayoutGroup heartLayout = heartsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        heartLayout.spacing = 8f;
        heartLayout.childAlignment = TextAnchor.MiddleLeft;
        heartLayout.childControlWidth = false;
        heartLayout.childControlHeight = false;
        heartLayout.childForceExpandWidth = false;
        heartLayout.childForceExpandHeight = false;

        Image heartTemplateImage = DemoUiFactory.CreateImage("Heart Template", heartsRoot, heartSprite, new Color(0.22f, 0.24f, 0.28f, 1f));
        heartTemplateImage.rectTransform.sizeDelta = new Vector2(48f, 48f);
        LayoutElement layoutElement = heartTemplateImage.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 48f;
        layoutElement.preferredHeight = 48f;
        Image heartFill = DemoUiFactory.CreateImage("Fill", heartTemplateImage.transform, heartSprite, new Color(0.95f, 0.15f, 0.22f, 1f));
        DemoUiFactory.Stretch(heartFill.rectTransform, 3f, 3f, 3f, 3f);
        heartFill.type = Image.Type.Filled;
        heartFill.fillMethod = Image.FillMethod.Horizontal;
        heartFill.fillOrigin = 0;
        heartTemplateImage.gameObject.SetActive(false);

        Image coinIcon = DemoUiFactory.CreateImage("Coin Icon", rightPanel.transform, circleSprite, new Color(1f, 0.76f, 0.12f));
        DemoUiFactory.SetAnchoredRect(coinIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -90f), new Vector2(32f, 32f));
        Text coinText = DemoUiFactory.CreateText("Coin Count", rightPanel.transform, "0", 25, TextAnchor.MiddleLeft, Color.white);
        DemoUiFactory.SetAnchoredRect(coinText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(66f, -90f), new Vector2(100f, 34f));

        Image bombIcon = DemoUiFactory.CreateImage("Bomb Icon", rightPanel.transform, circleSprite, new Color(0.45f, 0.48f, 0.56f));
        DemoUiFactory.SetAnchoredRect(bombIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -138f), new Vector2(32f, 32f));
        Text bombText = DemoUiFactory.CreateText("Bomb Count", rightPanel.transform, "3", 25, TextAnchor.MiddleLeft, Color.white);
        DemoUiFactory.SetAnchoredRect(bombText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(66f, -138f), new Vector2(100f, 34f));

        Text dashText = DemoUiFactory.CreateText("Dash State", rightPanel.transform, "Dash READY", 21, TextAnchor.UpperRight, new Color(0.4f, 0.82f, 1f));
        DemoUiFactory.SetAnchoredRect(dashText.rectTransform, Vector2.one, Vector2.one, new Vector2(-20f, -112f), new Vector2(160f, 50f));

        Image statsPanel = DemoUiFactory.CreateImage("Player Stats", root.transform, sprite, new Color(0.035f, 0.045f, 0.065f, 0.5f));
        DemoUiFactory.SetAnchoredRect(statsPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -270f), new Vector2(390f, 310f));
        Text statsText = DemoUiFactory.CreateText("Stats", statsPanel.transform, string.Empty, 24, TextAnchor.UpperLeft, Color.white);
        DemoUiFactory.Stretch(statsText.rectTransform, 26f, 24f, 22f, 22f);

        Image minimapPanel = DemoUiFactory.CreateImage("Minimap", root.transform, sprite, new Color(0.035f, 0.045f, 0.065f, 0.78f));
        DemoUiFactory.SetAnchoredRect(minimapPanel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -32f), new Vector2(430f, 250f));
        Text minimapTitle = DemoUiFactory.CreateText("Title", minimapPanel.transform, "MAP    S START   I ITEM   $ SHOP   B BOSS", 16, TextAnchor.MiddleLeft, new Color(0.68f, 0.76f, 0.88f));
        DemoUiFactory.SetAnchoredRect(minimapTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -10f), new Vector2(390f, 30f));
        RectTransform minimapNodes = DemoUiFactory.CreateUiObject("Room Nodes", minimapPanel.transform).GetComponent<RectTransform>();
        DemoUiFactory.SetAnchoredRect(minimapNodes, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(390f, 185f));
        Image minimapConnectionTemplate = DemoUiFactory.CreateImage("Room Connection Template", minimapNodes, sprite, new Color(0.48f, 0.72f, 0.88f, 0.85f));
        DemoUiFactory.SetAnchoredRect(minimapConnectionTemplate.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 5f));
        minimapConnectionTemplate.gameObject.SetActive(false);
        Image minimapNodeTemplate = DemoUiFactory.CreateImage("Room Node Template", minimapNodes, sprite, new Color(0.58f, 0.68f, 0.82f, 0.9f));
        DemoUiFactory.SetAnchoredRect(minimapNodeTemplate.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 22f));
        Text minimapNodeMarker = DemoUiFactory.CreateText("Marker", minimapNodeTemplate.transform, string.Empty, 15, TextAnchor.MiddleCenter, Color.white);
        DemoUiFactory.Stretch(minimapNodeMarker.rectTransform, 0f, 0f, 0f, 0f);
        minimapNodeTemplate.gameObject.SetActive(false);

        Text controls = DemoUiFactory.CreateText("Controls", root.transform, "WASD Move    Mouse Aim    LMB Fire    Space Dash    E Bomb    F Buy    Esc Pause", 23, TextAnchor.MiddleCenter, new Color(0.88f, 0.91f, 0.96f));
        DemoUiFactory.SetAnchoredRect(controls.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(1000f, 42f));
        Text roomLabel = DemoUiFactory.CreateText("Room Label", root.transform, "ERRORGAME DUNGEON     Defeat the boss and discover the hidden room", 20, TextAnchor.MiddleCenter, new Color(0.62f, 0.68f, 0.78f));
        DemoUiFactory.SetAnchoredRect(roomLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(900f, 40f));
        Text prompt = DemoUiFactory.CreateText("Interaction Prompt", root.transform, string.Empty, 27, TextAnchor.MiddleCenter, new Color(0.42f, 1f, 0.58f));
        DemoUiFactory.SetAnchoredRect(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(0f, -150f), new Vector2(500f, 48f));

        Image death = DemoUiFactory.CreateImage("Death Panel", root.transform, sprite, new Color(0.07f, 0.015f, 0.02f, 0.9f));
        DemoUiFactory.Stretch(death.rectTransform, 0f, 0f, 0f, 0f);
        Text deathTitle = DemoUiFactory.CreateText("Title", death.transform, "YOU DIED\nPress R to restart", 54, TextAnchor.MiddleCenter, new Color(1f, 0.35f, 0.38f));
        DemoUiFactory.Stretch(deathTitle.rectTransform, 0f, 0f, 0f, 0f);
        death.gameObject.SetActive(false);

        Image pauseOverlay = DemoUiFactory.CreateImage("Pause Menu", root.transform, sprite, new Color(0.025f, 0.035f, 0.055f, 0.96f));
        DemoUiFactory.Stretch(pauseOverlay.rectTransform, 0f, 0f, 0f, 0f);
        Text pauseTitle = DemoUiFactory.CreateText("Pause Title", pauseOverlay.transform, "PAUSED", 50, TextAnchor.MiddleCenter, Color.white);
        DemoUiFactory.SetAnchoredRect(pauseTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(500f, 70f));

        Button items = DemoUiFactory.CreateButton("Items Tab", pauseOverlay.transform, sprite, "1  ITEMS");
        Button notebook = DemoUiFactory.CreateButton("Notebook Tab", pauseOverlay.transform, sprite, "2  NOTEBOOK");
        Button log = DemoUiFactory.CreateButton("Log Tab", pauseOverlay.transform, sprite, "3  LOG");
        DemoUiFactory.SetAnchoredRect(items.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-260f, -135f), new Vector2(230f, 58f));
        DemoUiFactory.SetAnchoredRect(notebook.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -135f), new Vector2(230f, 58f));
        DemoUiFactory.SetAnchoredRect(log.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(260f, -135f), new Vector2(230f, 58f));

        Image contentPanel = DemoUiFactory.CreateImage("Tab Content", pauseOverlay.transform, sprite, new Color(0.075f, 0.09f, 0.13f, 1f));
        DemoUiFactory.SetAnchoredRect(contentPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1100f, 550f));
        Text tabContent = DemoUiFactory.CreateText("Content", contentPanel.transform, string.Empty, 30, TextAnchor.MiddleCenter, new Color(0.75f, 0.8f, 0.9f));
        DemoUiFactory.Stretch(tabContent.rectTransform, 40f, 40f, 40f, 40f);

        Text pauseHint = DemoUiFactory.CreateText("Pause Hint", pauseOverlay.transform, "Esc  Resume", 23, TextAnchor.MiddleRight, new Color(0.65f, 0.72f, 0.82f));
        DemoUiFactory.SetAnchoredRect(pauseHint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-35f, 28f), new Vector2(300f, 50f));
        Button mainMenu = DemoUiFactory.CreateButton("Return To Main Menu", pauseOverlay.transform, sprite, "MAIN MENU");
        DemoUiFactory.SetAnchoredRect(mainMenu.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(35f, 28f), new Vector2(260f, 58f));
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
}
