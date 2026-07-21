using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class SuperMushroomPickup : MonoBehaviour
{
    public const int MaxHealthIncreaseUnits = 2;
    public const float OtherStatMultiplier = 1.5f;

    [SerializeField] private Transform visualRoot;
    [SerializeField] private TextMesh label;

    private Player player;
    private RoomNode roomNode;
    private bool collected;
    private Vector3 baseScale;

    public void Configure(Transform mushroomVisualRoot, TextMesh mushroomLabel)
    {
        visualRoot = mushroomVisualRoot;
        label = mushroomLabel;
    }

    public void Initialize(Player playerReference, RoomNode rewardRoom)
    {
        player = playerReference;
        roomNode = rewardRoom;
        if (visualRoot != null) baseScale = visualRoot.localScale;
        if (label != null) label.text = "SUPER MUSHROOM\nMAX HEARTS +1\nOTHER STATS +50%";
    }

    private void Update()
    {
        if (visualRoot == null) return;
        float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.06f;
        visualRoot.localScale = baseScale * pulse;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || player == null || roomNode == null ||
            other.GetComponentInParent<Player>() != player)
        {
            return;
        }

        collected = true;
        // Two internal health units equal one complete heart in the HUD.
        player.Health.IncreaseMaxHealth(MaxHealthIncreaseUnits);
        player.Stats.MultiplyAll(OtherStatMultiplier);
        roomNode.MarkItemClaimed();
        Destroy(gameObject);
    }
}
