using UnityEngine;

public enum NormalUpgradeType
{
    MaxHealth,
    Damage,
    FireRate,
    MoveSpeed,
    Range,
    ProjectileSpeed
}

public static class NormalUpgradeCatalog
{
    public static void Apply(Player player, NormalUpgradeType type)
    {
        switch (type)
        {
            case NormalUpgradeType.MaxHealth: player.Health.IncreaseMaxHealth(2); break;
            case NormalUpgradeType.Damage: player.Stats.AddDamage(0.5f); break;
            case NormalUpgradeType.FireRate: player.Stats.AddFireRate(1f); break;
            case NormalUpgradeType.MoveSpeed: player.Stats.AddMoveSpeed(0.75f); break;
            case NormalUpgradeType.Range: player.Stats.AddRange(1.5f); break;
            case NormalUpgradeType.ProjectileSpeed: player.Stats.AddProjectileSpeed(2f); break;
        }
    }

    public static string GetDisplayName(NormalUpgradeType type)
    {
        switch (type)
        {
            case NormalUpgradeType.MaxHealth: return "MAX HEALTH +1";
            case NormalUpgradeType.Damage: return "DAMAGE +0.5";
            case NormalUpgradeType.FireRate: return "FIRE RATE +1";
            case NormalUpgradeType.MoveSpeed: return "MOVE SPEED +0.75";
            case NormalUpgradeType.Range: return "RANGE +1.5";
            case NormalUpgradeType.ProjectileSpeed: return "SHOT SPEED +2";
            default: return type.ToString();
        }
    }

    public static Color GetColor(NormalUpgradeType type)
    {
        float hue = ((int)type * 0.14f + 0.72f) % 1f;
        return Color.HSVToRGB(hue, 0.58f, 1f);
    }
}

public enum ShopProductType
{
    HalfHeart,
    FullHeart,
    MaxHealth,
    Damage,
    FireRate,
    MoveSpeed,
    Bomb
}

public static class ShopProductCatalog
{
    public static int GetPrice(ShopProductType type)
    {
        switch (type)
        {
            case ShopProductType.HalfHeart: return 1;
            case ShopProductType.FullHeart: return 2;
            case ShopProductType.Bomb: return 2;
            case ShopProductType.MoveSpeed: return 3;
            case ShopProductType.FireRate: return 4;
            case ShopProductType.Damage: return 4;
            case ShopProductType.MaxHealth: return 5;
            default: return 1;
        }
    }

    public static string GetDisplayName(ShopProductType type)
    {
        switch (type)
        {
            case ShopProductType.HalfHeart: return "HALF HEART";
            case ShopProductType.FullHeart: return "FULL HEART";
            case ShopProductType.MaxHealth: return "MAX HEALTH";
            case ShopProductType.Damage: return "DAMAGE UP";
            case ShopProductType.FireRate: return "FIRE RATE UP";
            case ShopProductType.MoveSpeed: return "MOVE SPEED UP";
            case ShopProductType.Bomb: return "BOMB +1";
            default: return type.ToString();
        }
    }

    public static bool CanApply(Player player, ShopProductType type)
    {
        if (player == null || player.Health.IsDead) return false;
        if (type == ShopProductType.HalfHeart || type == ShopProductType.FullHeart)
        {
            return player.Health.CurrentHealthUnits < player.Health.MaxHealthUnits;
        }
        if (type == ShopProductType.FireRate)
        {
            return player.Stats.FireRate < player.Stats.MaximumFireRate;
        }
        return true;
    }

    public static void Apply(Player player, ShopProductType type)
    {
        switch (type)
        {
            case ShopProductType.HalfHeart: player.Health.Heal(1); break;
            case ShopProductType.FullHeart: player.Health.Heal(2); break;
            case ShopProductType.MaxHealth: player.Health.IncreaseMaxHealth(2); break;
            case ShopProductType.Damage: player.Stats.AddDamage(0.5f); break;
            case ShopProductType.FireRate: player.Stats.AddFireRate(1f); break;
            case ShopProductType.MoveSpeed: player.Stats.AddMoveSpeed(0.75f); break;
            case ShopProductType.Bomb: player.Inventory.AddBombs(1); break;
        }
    }

    public static Color GetColor(ShopProductType type)
    {
        switch (type)
        {
            case ShopProductType.HalfHeart:
            case ShopProductType.FullHeart: return new Color(0.95f, 0.16f, 0.22f);
            case ShopProductType.Bomb: return new Color(0.38f, 0.44f, 0.55f);
            default: return new Color(1f, 0.72f, 0.12f);
        }
    }
}
