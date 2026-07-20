using UnityEngine;

[DisallowMultipleComponent]
public sealed class DoorController : MonoBehaviour, IExplosionReceiver
{
    [SerializeField] private RoomDirection direction;
    [SerializeField] private Collider2D blocker;
    [SerializeField] private BoxCollider2D transitionTrigger;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Color lockedColor = new Color(0.8f, 0.16f, 0.12f, 1f);
    [SerializeField] private Color openColor = new Color(0.18f, 0.62f, 0.72f, 0.18f);
    [SerializeField] private Color sealedColor = new Color(0.28f, 0.31f, 0.38f, 1f);

    private RoomController owner;
    private bool hasConnection;
    private bool bombOpenableSecretPassage;

    public RoomDirection Direction => direction;
    public bool IsLocked { get; private set; }
    public bool HasConnection => hasConnection;

    public void Configure(
        RoomDirection doorDirection,
        Collider2D blockingCollider,
        BoxCollider2D doorwayTrigger,
        SpriteRenderer doorVisual)
    {
        direction = doorDirection;
        blocker = blockingCollider;
        transitionTrigger = doorwayTrigger;
        visual = doorVisual;
        ConfigureTriggerShape();
        RefreshState();
    }

    public void Bind(RoomController roomOwner, bool connectionAvailable, bool canBombOpenSecretPassage)
    {
        owner = roomOwner;
        hasConnection = connectionAvailable;
        bombOpenableSecretPassage = canBombOpenSecretPassage;
        IsLocked = false;
        RefreshState();
    }

    public void SetConnectionAvailable(bool connectionAvailable)
    {
        hasConnection = connectionAvailable;
        if (hasConnection) bombOpenableSecretPassage = false;
        RefreshState();
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
        RefreshState();
    }

    private void RefreshState()
    {
        bool blocksPassage = !hasConnection || IsLocked;
        if (blocker != null) blocker.enabled = blocksPassage;
        if (transitionTrigger != null) transitionTrigger.enabled = hasConnection && !IsLocked;
        if (visual != null) visual.color = !hasConnection ? sealedColor : IsLocked ? lockedColor : openColor;
    }

    private void ConfigureTriggerShape()
    {
        if (transitionTrigger == null) return;

        bool horizontalDoor = direction == RoomDirection.North || direction == RoomDirection.South;
        transitionTrigger.size = horizontalDoor ? new Vector2(0.8f, 1f) : new Vector2(1f, 0.8f);
        switch (direction)
        {
            case RoomDirection.North: transitionTrigger.offset = new Vector2(0f, 1.55f); break;
            case RoomDirection.South: transitionTrigger.offset = new Vector2(0f, -1.55f); break;
            case RoomDirection.West: transitionTrigger.offset = new Vector2(-1.55f, 0f); break;
            case RoomDirection.East: transitionTrigger.offset = new Vector2(1.55f, 0f); break;
        }
    }

    public void ReceiveExplosion(Vector2 explosionOrigin, float explosionRadius)
    {
        if (hasConnection || !bombOpenableSecretPassage || owner == null) return;
        if (owner.TryOpenSecretPassage(direction))
        {
            bombOpenableSecretPassage = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasConnection || IsLocked || owner == null) return;
        Player player = other.GetComponentInParent<Player>();
        if (player != null) owner.TryUseDoor(direction, player);
    }
}
