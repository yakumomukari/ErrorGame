using UnityEngine;

public enum BossActionState
{
    Waiting,
    Chasing,
    Telegraphing,
    Dashing
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
[DisallowMultipleComponent]
public sealed class BossEnemy : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 1.7f;
    [SerializeField, Min(1)] private int contactDamageUnits = 2;
    [SerializeField, Min(0f)] private float activationDelay = 0.3f;
    [SerializeField, Min(0.1f)] private float dashCooldown = 2.5f;
    [SerializeField, Min(0.1f)] private float telegraphDuration = 0.75f;
    [SerializeField, Min(0.05f)] private float dashDuration = 0.42f;
    [SerializeField, Min(0.1f)] private float dashSpeed = 13f;
    [SerializeField] private SpriteRenderer bodyVisual;
    [SerializeField] private SpriteRenderer warningVisual;
    [SerializeField] private Color normalColor = new Color(0.5f, 0.12f, 0.16f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, 0.04f, 0.03f, 1f);

    private Rigidbody2D body;
    private Player target;
    private float activationTime;
    private float nextDashTime;
    private float stateEndTime;
    private Vector2 dashDirection;

    public EnemyHealth Health { get; private set; }
    public BossActionState State { get; private set; } = BossActionState.Waiting;
    public float TelegraphDuration => telegraphDuration;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        Health = GetComponent<EnemyHealth>();
        SetWarningVisible(false);
    }

    public void Configure(SpriteRenderer bossBody, SpriteRenderer redWarning)
    {
        bodyVisual = bossBody;
        warningVisual = redWarning;
        SetWarningVisible(false);
    }

    public void Initialize(Player playerTarget)
    {
        target = playerTarget;
        activationTime = Time.time + activationDelay;
        nextDashTime = activationTime + dashCooldown;
        State = BossActionState.Waiting;
    }

    private void Update()
    {
        if (State != BossActionState.Telegraphing || warningVisual == null) return;
        float pulse = 0.72f + Mathf.PingPong(Time.time * 3.5f, 0.28f);
        Color color = warningColor;
        color.a = pulse * 0.62f;
        warningVisual.color = color;
        warningVisual.transform.localScale = Vector3.one * Mathf.Lerp(2.25f, 2.8f, pulse);
    }

    private void FixedUpdate()
    {
        if (target == null || target.Health.IsDead) return;
        if (Time.time < activationTime) return;

        if (State == BossActionState.Waiting) State = BossActionState.Chasing;
        switch (State)
        {
            case BossActionState.Chasing:
                ChaseTarget();
                if (Time.time >= nextDashTime) BeginTelegraph();
                break;
            case BossActionState.Telegraphing:
                if (Time.time >= stateEndTime) BeginDash();
                break;
            case BossActionState.Dashing:
                body.MovePosition(body.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
                if (Time.time >= stateEndTime) FinishDash();
                break;
        }
    }

    private void ChaseTarget()
    {
        Vector2 direction = ((Vector2)target.transform.position - body.position).normalized;
        body.MovePosition(body.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    private void BeginTelegraph()
    {
        dashDirection = ((Vector2)target.transform.position - body.position).normalized;
        if (dashDirection.sqrMagnitude < 0.01f) dashDirection = Vector2.down;
        State = BossActionState.Telegraphing;
        stateEndTime = Time.time + telegraphDuration;
        SetWarningVisible(true);
    }

    private void BeginDash()
    {
        State = BossActionState.Dashing;
        stateEndTime = Time.time + dashDuration;
        SetWarningVisible(false);
    }

    private void FinishDash()
    {
        State = BossActionState.Chasing;
        nextDashTime = Time.time + dashCooldown;
    }

    private void SetWarningVisible(bool visible)
    {
        if (bodyVisual != null) bodyVisual.color = visible ? warningColor : normalColor;
        if (warningVisual != null) warningVisual.gameObject.SetActive(visible);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Player hitPlayer = collision.collider.GetComponentInParent<Player>();
        if (hitPlayer == null) return;
        hitPlayer.Health.TakeDamage(contactDamageUnits);
    }
}
