using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class DemoDataAssetFactory
{
    private const string GeneratedArtFolder = "Assets/Art/Generated";
    private const string DataFolder = "Assets/Data";
    private const string CombatDataFolder = DataFolder + "/Combat";
    private const string EffectsDataFolder = DataFolder + "/Effects";
    private const string NormalEffectsFolder = EffectsDataFolder + "/Normal";
    private const string ShopEffectsFolder = EffectsDataFolder + "/Shop";
    private const string ShopDataFolder = DataFolder + "/Shop";
    private const string ShopProductsFolder = ShopDataFolder + "/Products";
    private const string RoomsDataFolder = DataFolder + "/Rooms";
    private const string WhiteSpritePath = GeneratedArtFolder + "/StageAWhite.asset";
    private const string HeartSpritePath = GeneratedArtFolder + "/StageAHeart.asset";
    private const string CircleSpritePath = GeneratedArtFolder + "/StageACircle.asset";

    public const string EnemySpawnCatalogPath = CombatDataFolder + "/DefaultEnemySpawnCatalog.asset";
    public const string NormalUpgradeSetPath = EffectsDataFolder + "/DefaultNormalUpgradeSet.asset";
    public const string ShopProductSetPath = ShopDataFolder + "/DefaultShopProductSet.asset";
    public const string RoomPrefabCatalogPath = RoomsDataFolder + "/DefaultRoomPrefabCatalog.asset";

    public static void EnsureProjectFolders()
    {
        EnsureFolder("Assets", "Art");
        EnsureFolder("Assets/Art", "Generated");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets/Prefabs", "Player");
        EnsureFolder("Assets/Prefabs", "Combat");
        EnsureFolder("Assets/Prefabs", "Enemies");
        EnsureFolder("Assets/Prefabs", "Rooms");
        EnsureFolder("Assets/Prefabs/Rooms", "Variants");
        EnsureFolder("Assets/Prefabs", "Doors");
        EnsureFolder("Assets/Prefabs", "Pickups");
        EnsureFolder("Assets/Prefabs", "UI");
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Data");
        EnsureFolder("Assets/Data", "Combat");
        EnsureFolder("Assets/Data", "Effects");
        EnsureFolder("Assets/Data/Effects", "Normal");
        EnsureFolder("Assets/Data/Effects", "Shop");
        EnsureFolder("Assets/Data", "Shop");
        EnsureFolder("Assets/Data/Shop", "Products");
        EnsureFolder("Assets/Data", "Rooms");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    public static Sprite CreateOrLoadWhiteSprite()
    {
        return CreateOrLoadGeneratedSprite(WhiteSpritePath, "StageAWhite", (x, y) => Color.white);
    }

    public static EnemySpawnCatalog CreateOrUpdateEnemySpawnCatalog(
        IEnumerable<EnemySpawnEntry> entries)
    {
        EnemySpawnCatalog catalog = AssetDatabase.LoadAssetAtPath<EnemySpawnCatalog>(EnemySpawnCatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<EnemySpawnCatalog>();
            AssetDatabase.CreateAsset(catalog, EnemySpawnCatalogPath);
        }

        catalog.Configure(entries);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    public static PlayerEffectSet CreateOrUpdateNormalUpgradeSet()
    {
        PlayerEffectDefinition[] definitions =
        {
            CreateOrUpdatePlayerEffect(
                NormalEffectsFolder + "/MaxHealth.asset",
                "max_health",
                "MAX HEALTH +1",
                NormalUpgradeCatalog.GetColor(NormalUpgradeType.MaxHealth),
                new PlayerEffectModifier(PlayerEffectOperation.IncreaseMaxHealthUnits, 2f)),
            CreateOrUpdatePlayerEffect(
                NormalEffectsFolder + "/Damage.asset",
                "damage",
                "DAMAGE +0.5",
                NormalUpgradeCatalog.GetColor(NormalUpgradeType.Damage),
                new PlayerEffectModifier(PlayerEffectOperation.AddDamage, 0.5f)),
            CreateOrUpdatePlayerEffect(
                NormalEffectsFolder + "/FireRate.asset",
                "fire_rate",
                "FIRE RATE +1",
                NormalUpgradeCatalog.GetColor(NormalUpgradeType.FireRate),
                new PlayerEffectModifier(PlayerEffectOperation.AddFireRate, 1f)),
            CreateOrUpdatePlayerEffect(
                NormalEffectsFolder + "/MoveSpeed.asset",
                "move_speed",
                "MOVE SPEED +0.75",
                NormalUpgradeCatalog.GetColor(NormalUpgradeType.MoveSpeed),
                new PlayerEffectModifier(PlayerEffectOperation.AddMoveSpeed, 0.75f)),
            CreateOrUpdatePlayerEffect(
                NormalEffectsFolder + "/Range.asset",
                "range",
                "RANGE +1.5",
                NormalUpgradeCatalog.GetColor(NormalUpgradeType.Range),
                new PlayerEffectModifier(PlayerEffectOperation.AddRange, 1.5f)),
            CreateOrUpdatePlayerEffect(
                NormalEffectsFolder + "/ProjectileSpeed.asset",
                "projectile_speed",
                "SHOT SPEED +2",
                NormalUpgradeCatalog.GetColor(NormalUpgradeType.ProjectileSpeed),
                new PlayerEffectModifier(PlayerEffectOperation.AddProjectileSpeed, 2f))
        };

        PlayerEffectSet set = AssetDatabase.LoadAssetAtPath<PlayerEffectSet>(NormalUpgradeSetPath);
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<PlayerEffectSet>();
            AssetDatabase.CreateAsset(set, NormalUpgradeSetPath);
        }
        set.Configure(definitions);
        EditorUtility.SetDirty(set);
        return set;
    }

    private static PlayerEffectDefinition CreateOrUpdatePlayerEffect(
        string path,
        string stableId,
        string displayName,
        Color color,
        params PlayerEffectModifier[] modifiers)
    {
        PlayerEffectDefinition definition = AssetDatabase.LoadAssetAtPath<PlayerEffectDefinition>(path);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<PlayerEffectDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }
        definition.Configure(stableId, displayName, color, modifiers);
        EditorUtility.SetDirty(definition);
        return definition;
    }

    public static ShopProductSet CreateOrUpdateShopProductSet(PlayerEffectSet normalUpgradeSet)
    {
        PlayerEffectDefinition halfHeart = CreateOrUpdatePlayerEffect(
            ShopEffectsFolder + "/HealHalfHeart.asset",
            "heal_half_heart",
            "HALF HEART",
            ShopProductCatalog.GetColor(ShopProductType.HalfHeart),
            new PlayerEffectModifier(PlayerEffectOperation.HealHealthUnits, 1f));
        PlayerEffectDefinition fullHeart = CreateOrUpdatePlayerEffect(
            ShopEffectsFolder + "/HealFullHeart.asset",
            "heal_full_heart",
            "FULL HEART",
            ShopProductCatalog.GetColor(ShopProductType.FullHeart),
            new PlayerEffectModifier(PlayerEffectOperation.HealHealthUnits, 2f));
        PlayerEffectDefinition bomb = CreateOrUpdatePlayerEffect(
            ShopEffectsFolder + "/AddBomb.asset",
            "add_bomb",
            "BOMB +1",
            ShopProductCatalog.GetColor(ShopProductType.Bomb),
            new PlayerEffectModifier(PlayerEffectOperation.AddBombs, 1f));

        PlayerEffectDefinition FindNormal(string stableId)
        {
            return normalUpgradeSet.Effects.First(effect => effect != null && effect.StableId == stableId);
        }

        ShopProductDefinition[] definitions =
        {
            CreateOrUpdateShopProduct(ShopProductType.HalfHeart, halfHeart),
            CreateOrUpdateShopProduct(ShopProductType.FullHeart, fullHeart),
            CreateOrUpdateShopProduct(ShopProductType.MaxHealth, FindNormal("max_health")),
            CreateOrUpdateShopProduct(ShopProductType.Damage, FindNormal("damage")),
            CreateOrUpdateShopProduct(ShopProductType.FireRate, FindNormal("fire_rate")),
            CreateOrUpdateShopProduct(ShopProductType.MoveSpeed, FindNormal("move_speed")),
            CreateOrUpdateShopProduct(ShopProductType.Bomb, bomb)
        };

        ShopProductSet set = AssetDatabase.LoadAssetAtPath<ShopProductSet>(ShopProductSetPath);
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<ShopProductSet>();
            AssetDatabase.CreateAsset(set, ShopProductSetPath);
        }
        set.Configure(definitions);
        EditorUtility.SetDirty(set);
        return set;
    }

    public static RoomPrefabCatalog CreateOrUpdateRoomPrefabCatalog(
        IEnumerable<RoomPrefabEntry> defaultEntries)
    {
        RoomPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<RoomPrefabCatalog>(RoomPrefabCatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<RoomPrefabCatalog>();
            AssetDatabase.CreateAsset(catalog, RoomPrefabCatalogPath);
        }

        List<RoomPrefabEntry> mergedEntries = catalog.Entries
            .Where(entry => entry != null)
            .ToList();
        if (defaultEntries != null)
        {
            foreach (RoomPrefabEntry defaultEntry in defaultEntries)
            {
                if (defaultEntry == null) continue;

                int existingIndex = mergedEntries.FindIndex(entry =>
                    entry.RoomType == defaultEntry.RoomType &&
                    entry.StableId == defaultEntry.StableId);
                if (existingIndex >= 0)
                {
                    RoomPrefabEntry existing = mergedEntries[existingIndex];
                    if (existing.Prefab == null)
                    {
                        mergedEntries[existingIndex] = new RoomPrefabEntry(
                            defaultEntry.StableId,
                            defaultEntry.RoomType,
                            defaultEntry.Prefab,
                            existing.Weight);
                    }
                    continue;
                }
                mergedEntries.Add(defaultEntry);
            }
        }

        catalog.Configure(mergedEntries);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static ShopProductDefinition CreateOrUpdateShopProduct(
        ShopProductType type,
        PlayerEffectDefinition effect)
    {
        string path = ShopProductsFolder + "/" + type + ".asset";
        ShopProductDefinition definition = AssetDatabase.LoadAssetAtPath<ShopProductDefinition>(path);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<ShopProductDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }
        definition.Configure(
            GetShopProductStableId(type),
            ShopProductCatalog.GetDisplayName(type),
            ShopProductCatalog.GetPrice(type),
            ShopProductCatalog.GetColor(type),
            effect);
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static string GetShopProductStableId(ShopProductType type)
    {
        switch (type)
        {
            case ShopProductType.HalfHeart: return "half_heart";
            case ShopProductType.FullHeart: return "full_heart";
            case ShopProductType.MaxHealth: return "max_health";
            case ShopProductType.Damage: return "damage";
            case ShopProductType.FireRate: return "fire_rate";
            case ShopProductType.MoveSpeed: return "move_speed";
            case ShopProductType.Bomb: return "bomb";
            default: return type.ToString();
        }
    }

    public static Sprite CreateOrLoadHeartSprite()
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

    public static Sprite CreateOrLoadCircleSprite()
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
}
