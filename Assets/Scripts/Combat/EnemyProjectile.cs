using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector2 origin;
    private float maxRange;
    private int damageUnits;
    private RangedEnemy owner;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float speed, float range, int damage, RangedEnemy projectileOwner)
    {
        owner = projectileOwner;
        owner?.RegisterProjectile(this);
        origin = transform.position;
        maxRange = Mathf.Max(0.1f, range);
        damageUnits = Mathf.Max(1, damage);
        body.velocity = direction.normalized * Mathf.Max(0f, speed);
        transform.up = direction;
    }

    private void Update()
    {
        if (((Vector2)transform.position - origin).sqrMagnitude >= maxRange * maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy shots pass through enemies, but are stopped by room geometry.
        if (other.GetComponentInParent<EnemyHealth>() != null) return;

        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            player.Health.TakeDamage(damageUnits);
            Destroy(gameObject);
            return;
        }

        if (other.isTrigger) return;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        owner?.UnregisterProjectile(this);
        owner = null;
    }
}
