using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class Projectile : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector2 origin;
    private float maxRange;
    private int damageUnits;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float speed, float range, float damage)
    {
        origin = transform.position;
        maxRange = range;
        damageUnits = Mathf.Max(1, Mathf.RoundToInt(damage));
        body.velocity = direction.normalized * speed;
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
        if (other.GetComponentInParent<Player>() != null) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damageUnits);
        }
        else if (other.isTrigger)
        {
            return;
        }

        Destroy(gameObject);
    }
}
