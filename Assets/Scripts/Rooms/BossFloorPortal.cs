using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossFloorPortal : MonoBehaviour, IRoomFeature
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Collider2D portalTrigger;
    [SerializeField] private TextMesh label;
    [SerializeField, Min(0f)] private float pulseSpeed = 2.5f;
    [SerializeField, Range(0f, 0.25f)] private float pulseAmount = 0.08f;

    private IRoomRuntimeContext runtimeContext;
    private RoomNode roomNode;
    private Vector3 visualBaseScale;
    private bool transitionRequested;

    public SpriteRenderer Visual => visual;
    public Collider2D PortalTrigger => portalTrigger;

    public void Configure(SpriteRenderer portalVisual, Collider2D trigger, TextMesh portalLabel)
    {
        visual = portalVisual;
        portalTrigger = trigger;
        label = portalLabel;
        CacheVisualScale();
    }

    public void Initialize(IRoomRuntimeContext context, RoomNode node)
    {
        runtimeContext = context;
        roomNode = node;
        transitionRequested = false;
        CacheVisualScale();
        if (roomNode != null) roomNode.StateChanged += RefreshAvailability;
        RefreshAvailability();
    }

    public void OnRoomEntered()
    {
        transitionRequested = false;
        RefreshAvailability();
    }

    private void Update()
    {
        if (visual == null) return;
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        visual.transform.localScale = new Vector3(
            visualBaseScale.x * pulse,
            visualBaseScale.y * pulse,
            visualBaseScale.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (transitionRequested || roomNode == null || !roomNode.IsCleared) return;
        Player player = other.GetComponentInParent<Player>();
        if (player == null || runtimeContext == null || player != runtimeContext.Player) return;

        transitionRequested = runtimeContext.TryAdvanceToNextFloor(roomNode);
    }

    private void RefreshAvailability()
    {
        bool available = roomNode != null && roomNode.Type == RoomType.Boss && roomNode.IsCleared;
        if (gameObject.activeSelf != available) gameObject.SetActive(available);
    }

    private void CacheVisualScale()
    {
        if (visual != null) visualBaseScale = visual.transform.localScale;
    }

    private void OnDestroy()
    {
        if (roomNode != null) roomNode.StateChanged -= RefreshAvailability;
    }
}
