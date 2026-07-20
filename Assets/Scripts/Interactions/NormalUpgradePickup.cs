using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class NormalUpgradePickup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private TextMesh label;

    private Player player;
    private RoomRewardState rewardState;
    private NormalUpgradeType upgradeType;
    private bool collected;

    public void Configure(SpriteRenderer pickupVisual, TextMesh pickupLabel)
    {
        visual = pickupVisual;
        label = pickupLabel;
    }

    public void Initialize(Player playerReference, RoomRewardState roomRewards, NormalUpgradeType type)
    {
        player = playerReference;
        rewardState = roomRewards;
        upgradeType = type;
        if (visual != null) visual.color = NormalUpgradeCatalog.GetColor(type);
        if (label != null) label.text = NormalUpgradeCatalog.GetDisplayName(type) + "\nFREE";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponentInParent<Player>() != player) return;

        collected = true;
        NormalUpgradeCatalog.Apply(player, upgradeType);
        rewardState.MarkItemClaimed();
        Destroy(gameObject);
    }
}
