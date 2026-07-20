using System;
using UnityEngine;

public enum BasicResourceType
{
    Coin,
    HalfHeart,
    Bomb
}

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class BasicResourcePickup : MonoBehaviour
{
    [SerializeField] private BasicResourceType resourceType;
    [SerializeField, Min(1)] private int amount = 1;

    public BasicResourceType ResourceType => resourceType;
    public event Action Collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.GetComponentInParent<Player>());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // A full-health player may stand on a heart. Keeping the overlap check
        // lets that heart be collected immediately if the player is then hurt.
        TryCollect(other.GetComponentInParent<Player>());
    }

    public void Configure(BasicResourceType type, int resourceAmount)
    {
        resourceType = type;
        amount = Mathf.Max(1, resourceAmount);
    }

    private void TryCollect(Player player)
    {
        if (player == null || player.Health.IsDead) return;
        if (resourceType == BasicResourceType.HalfHeart &&
            player.Health.CurrentHealthUnits >= player.Health.MaxHealthUnits) return;

        bool collected = true;
        switch (resourceType)
        {
            case BasicResourceType.Coin:
                player.Inventory.AddCoins(amount);
                break;
            case BasicResourceType.HalfHeart:
                collected = player.Health.Heal(amount) > 0;
                break;
            case BasicResourceType.Bomb:
                player.Inventory.AddBombs(amount);
                break;
        }

        if (collected)
        {
            Collected?.Invoke();
            Destroy(gameObject);
        }
    }
}
