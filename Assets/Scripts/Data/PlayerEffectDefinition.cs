using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerEffectOperation
{
    HealHealthUnits,
    IncreaseMaxHealthUnits,
    AddDamage,
    AddFireRate,
    AddMoveSpeed,
    AddRange,
    AddProjectileSpeed,
    AddLuck,
    AddBombs,
    MultiplyAllStats
}

[Serializable]
public sealed class PlayerEffectModifier
{
    [SerializeField] private PlayerEffectOperation operation;
    [SerializeField] private float amount;

    public PlayerEffectOperation Operation => operation;
    public float Amount => amount;

    public PlayerEffectModifier(PlayerEffectOperation effectOperation, float effectAmount)
    {
        operation = effectOperation;
        amount = effectAmount;
    }

    public bool CanApply(Player player)
    {
        if (player == null || player.Health.IsDead) return false;
        if (operation == PlayerEffectOperation.AddFireRate && amount > 0f)
        {
            return player.Stats.FireRate < player.Stats.MaximumFireRate;
        }
        return operation != PlayerEffectOperation.HealHealthUnits ||
               player.Health.CurrentHealthUnits < player.Health.MaxHealthUnits;
    }

    public void Apply(Player player)
    {
        switch (operation)
        {
            case PlayerEffectOperation.HealHealthUnits:
                player.Health.Heal(Mathf.Max(1, Mathf.RoundToInt(amount)));
                break;
            case PlayerEffectOperation.IncreaseMaxHealthUnits:
                player.Health.IncreaseMaxHealth(Mathf.Max(1, Mathf.RoundToInt(amount)));
                break;
            case PlayerEffectOperation.AddDamage:
                player.Stats.AddDamage(amount);
                break;
            case PlayerEffectOperation.AddFireRate:
                player.Stats.AddFireRate(amount);
                break;
            case PlayerEffectOperation.AddMoveSpeed:
                player.Stats.AddMoveSpeed(amount);
                break;
            case PlayerEffectOperation.AddRange:
                player.Stats.AddRange(amount);
                break;
            case PlayerEffectOperation.AddProjectileSpeed:
                player.Stats.AddProjectileSpeed(amount);
                break;
            case PlayerEffectOperation.AddLuck:
                player.Stats.AddLuck(amount);
                break;
            case PlayerEffectOperation.AddBombs:
                player.Inventory.AddBombs(Mathf.Max(1, Mathf.RoundToInt(amount)));
                break;
            case PlayerEffectOperation.MultiplyAllStats:
                player.Stats.MultiplyAll(amount);
                break;
        }
    }
}

[CreateAssetMenu(
    fileName = "PlayerEffect",
    menuName = "Error Game/Effects/Player Effect")]
public sealed class PlayerEffectDefinition : ScriptableObject
{
    [SerializeField] private string stableId;
    [SerializeField] private string displayName;
    [SerializeField] private Color displayColor = Color.white;
    [SerializeField] private List<PlayerEffectModifier> modifiers = new List<PlayerEffectModifier>();

    public string StableId => stableId;
    public string DisplayName => displayName;
    public Color DisplayColor => displayColor;
    public IReadOnlyList<PlayerEffectModifier> Modifiers => modifiers;

    public bool CanApply(Player player)
    {
        if (modifiers == null || modifiers.Count == 0) return false;
        foreach (PlayerEffectModifier modifier in modifiers)
        {
            if (modifier != null && modifier.CanApply(player)) return true;
        }
        return false;
    }

    public void Apply(Player player)
    {
        if (player == null || modifiers == null) return;
        foreach (PlayerEffectModifier modifier in modifiers)
        {
            if (modifier != null && modifier.CanApply(player)) modifier.Apply(player);
        }
    }

    public void Configure(
        string id,
        string effectDisplayName,
        Color color,
        IEnumerable<PlayerEffectModifier> effectModifiers)
    {
        stableId = id;
        displayName = effectDisplayName;
        displayColor = color;
        modifiers = effectModifiers != null
            ? new List<PlayerEffectModifier>(effectModifiers)
            : new List<PlayerEffectModifier>();
    }
}
