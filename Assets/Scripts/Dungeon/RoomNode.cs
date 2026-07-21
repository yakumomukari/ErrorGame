using System;
using System.Collections.Generic;

public enum RoomType
{
    Start,
    Combat,
    Item,
    Shop,
    Boss,
    Secret,
    SuperSecret
}

public static class RoomTypeUtility
{
    public static bool IsVisibleSpecialRoom(RoomType type)
    {
        return type == RoomType.Item || type == RoomType.Shop || type == RoomType.Boss;
    }

    public static bool IsHiddenRoom(RoomType type)
    {
        return type == RoomType.Secret || type == RoomType.SuperSecret;
    }
}

public sealed class RoomNode
{
    private readonly HashSet<RoomDirection> connections = new HashSet<RoomDirection>();

    public RoomCoordinate Coordinate { get; }
    public RoomType Type { get; private set; }
    public RoomLifecycleState Lifecycle { get; }
    public RoomRewardState Rewards { get; }
    public RoomShopState Shop { get; }
    public bool IsVisited => Lifecycle.IsVisited;
    public bool IsCleared => Lifecycle.IsCleared;
    public bool IsItemClaimed => Rewards.IsItemClaimed;
    public int CombatRewardType => Rewards.CombatRewardType;
    public bool IsCombatRewardCollected => Rewards.IsCombatRewardCollected;
    public string RoomVariantId { get; private set; }
    public IReadOnlyCollection<RoomDirection> Connections => connections;
    public IReadOnlyCollection<int> PurchasedShopSlots => Shop.PurchasedSlots;
    public IReadOnlyCollection<int> CollectedSecretRewards => Rewards.CollectedSecretRewards;

    public event Action StateChanged;

    public RoomNode(RoomCoordinate coordinate, RoomType type)
    {
        Coordinate = coordinate;
        Type = type;
        Lifecycle = new RoomLifecycleState(type == RoomType.Start, NotifyStateChanged);
        Rewards = new RoomRewardState(NotifyStateChanged);
        Shop = new RoomShopState(NotifyStateChanged);
    }

    public bool HasConnection(RoomDirection direction) => connections.Contains(direction);
    public bool IsShopSlotPurchased(int slotIndex) => Shop.IsSlotPurchased(slotIndex);
    public bool IsSecretRewardCollected(int rewardIndex) => Rewards.IsSecretRewardCollected(rewardIndex);

    public void MarkVisited()
    {
        Lifecycle.MarkVisited();
    }

    public void MarkCleared()
    {
        Lifecycle.MarkCleared();
    }

    public void MarkItemClaimed()
    {
        Rewards.MarkItemClaimed();
    }

    public void MarkShopSlotPurchased(int slotIndex)
    {
        Shop.MarkSlotPurchased(slotIndex);
    }

    public void SetCombatReward(int rewardType)
    {
        Rewards.SetCombatReward(rewardType);
    }

    public void MarkCombatRewardCollected()
    {
        Rewards.MarkCombatRewardCollected();
    }

    public void MarkSecretRewardCollected(int rewardIndex)
    {
        Rewards.MarkSecretRewardCollected(rewardIndex);
    }

    public void AssignRoomVariant(string stableId)
    {
        string normalized = stableId != null ? stableId.Trim() : string.Empty;
        if (RoomVariantId == normalized) return;
        RoomVariantId = normalized;
        NotifyStateChanged();
    }

    public void RestoreState(
        bool visited,
        bool cleared,
        bool itemClaimed,
        IEnumerable<int> purchasedSlots,
        int combatRewardType,
        bool combatRewardCollected,
        IEnumerable<int> secretRewards,
        string roomVariantId = null)
    {
        Lifecycle.Restore(visited, cleared);
        Rewards.Restore(itemClaimed, combatRewardType, combatRewardCollected, secretRewards);
        Shop.Restore(purchasedSlots);
        RoomVariantId = roomVariantId != null ? roomVariantId.Trim() : string.Empty;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    internal void AddConnection(RoomDirection direction) => connections.Add(direction);
    internal void SetType(RoomType type) => Type = type;
}
