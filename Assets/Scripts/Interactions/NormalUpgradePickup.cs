using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class NormalUpgradePickup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private TextMesh label;

    private Player player;
    private RoomNode roomNode;
    private PlayerEffectDefinition effectDefinition;
    private NormalUpgradeType upgradeType;
    private bool collected;

    public void Configure(SpriteRenderer pickupVisual, TextMesh pickupLabel)
    {
        visual = pickupVisual;
        label = pickupLabel;
    }

    public void Initialize(Player playerReference, RoomNode rewardRoom, NormalUpgradeType type)
    {
        player = playerReference;
        roomNode = rewardRoom;
        effectDefinition = null;
        upgradeType = type;
        if (visual != null) visual.color = NormalUpgradeCatalog.GetColor(type);
        if (label != null) label.text = NormalUpgradeCatalog.GetDisplayName(type) + "\nFREE";
    }

    public void Initialize(
        Player playerReference,
        RoomNode rewardRoom,
        PlayerEffectDefinition definition)
    {
        player = playerReference;
        roomNode = rewardRoom;
        effectDefinition = definition;
        if (visual != null && definition != null) visual.color = definition.DisplayColor;
        if (label != null)
        {
            label.text = definition != null ? definition.DisplayName + "\nFREE" : "INVALID EFFECT";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || player == null || roomNode == null ||
            other.GetComponentInParent<Player>() != player) return;

        collected = true;
        if (effectDefinition != null) effectDefinition.Apply(player);
        else NormalUpgradeCatalog.Apply(player, upgradeType);
        roomNode.MarkItemClaimed();
        Destroy(gameObject);
    }
}
