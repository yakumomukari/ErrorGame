using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
[DisallowMultipleComponent]
public sealed class MeleeEnemy : MonoBehaviour, ICombatEnemy
{
    [SerializeField, Min(0f)] private float moveSpeed = 2.4f;
    [SerializeField, Min(1)] private int contactDamageUnits = 1;
    [SerializeField, Min(0f)] private float chaseActivationDelay = 0.3f;

    private Rigidbody2D body;
    private Player target;
    private float chaseActivationTime;

    public EnemyHealth Health { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        Health = GetComponent<EnemyHealth>();
    }

    public void Initialize(Player playerTarget)
    {
        target = playerTarget;
        chaseActivationTime = Time.time + chaseActivationDelay;
    }

    private void FixedUpdate()
    {
        if (target == null || target.Health.IsDead || Time.time < chaseActivationTime) return;

        Vector2 direction = ((Vector2)target.transform.position - body.position).normalized;
        body.MovePosition(body.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Player hitPlayer = collision.collider.GetComponentInParent<Player>();
        if (hitPlayer == null) return;

        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();
        damageable?.TakeDamage(contactDamageUnits);
    }
}
