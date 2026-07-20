using System;
using System.Collections.Generic;

public sealed class RoomLifecycleState
{
    private readonly Action changed;
    private readonly bool startsCleared;

    public bool IsVisited { get; private set; }
    public bool IsCleared { get; private set; }

    internal RoomLifecycleState(bool initiallyCleared, Action stateChanged)
    {
        startsCleared = initiallyCleared;
        IsCleared = initiallyCleared;
        changed = stateChanged;
    }

    internal void MarkVisited()
    {
        if (IsVisited) return;
        IsVisited = true;
        changed?.Invoke();
    }

    internal void MarkCleared()
    {
        if (IsCleared) return;
        IsCleared = true;
        changed?.Invoke();
    }

    internal void Restore(bool visited, bool cleared)
    {
        IsVisited = visited;
        IsCleared = cleared || startsCleared;
    }
}

public sealed class RoomRewardState
{
    private readonly Action changed;
    private readonly HashSet<int> collectedSecretRewards = new HashSet<int>();

    public bool IsItemClaimed { get; private set; }
    public int CombatRewardType { get; private set; } = -2;
    public bool IsCombatRewardCollected { get; private set; }
    public IReadOnlyCollection<int> CollectedSecretRewards => collectedSecretRewards;

    internal RoomRewardState(Action stateChanged)
    {
        changed = stateChanged;
    }

    public bool IsSecretRewardCollected(int rewardIndex)
    {
        return collectedSecretRewards.Contains(rewardIndex);
    }

    internal void MarkItemClaimed()
    {
        if (IsItemClaimed) return;
        IsItemClaimed = true;
        changed?.Invoke();
    }

    internal void SetCombatReward(int rewardType)
    {
        if (CombatRewardType != -2) return;
        CombatRewardType = rewardType;
        changed?.Invoke();
    }

    internal void MarkCombatRewardCollected()
    {
        if (IsCombatRewardCollected) return;
        IsCombatRewardCollected = true;
        changed?.Invoke();
    }

    internal void MarkSecretRewardCollected(int rewardIndex)
    {
        if (collectedSecretRewards.Add(rewardIndex)) changed?.Invoke();
    }

    internal void Restore(
        bool itemClaimed,
        int combatRewardType,
        bool combatRewardCollected,
        IEnumerable<int> secretRewards)
    {
        IsItemClaimed = itemClaimed;
        CombatRewardType = combatRewardType;
        IsCombatRewardCollected = combatRewardCollected;
        collectedSecretRewards.Clear();
        if (secretRewards != null) collectedSecretRewards.UnionWith(secretRewards);
    }
}

public sealed class RoomShopState
{
    private readonly Action changed;
    private readonly HashSet<int> purchasedSlots = new HashSet<int>();

    public IReadOnlyCollection<int> PurchasedSlots => purchasedSlots;

    internal RoomShopState(Action stateChanged)
    {
        changed = stateChanged;
    }

    public bool IsSlotPurchased(int slotIndex)
    {
        return purchasedSlots.Contains(slotIndex);
    }

    internal void MarkSlotPurchased(int slotIndex)
    {
        if (purchasedSlots.Add(slotIndex)) changed?.Invoke();
    }

    internal void Restore(IEnumerable<int> slots)
    {
        purchasedSlots.Clear();
        if (slots != null) purchasedSlots.UnionWith(slots);
    }
}
