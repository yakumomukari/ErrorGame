using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Bomb : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float fuseDuration = 1.5f;
    [SerializeField, Min(0.1f)] private float explosionRadius = 2.2f;
    [SerializeField, Min(1)] private int damageUnits = 2;
    [SerializeField] private SpriteRenderer visual;

    private readonly Collider2D[] overlapResults = new Collider2D[64];
    private bool exploded;

    public float FuseDuration => fuseDuration;
    public float ExplosionRadius => explosionRadius;

    public void Configure(SpriteRenderer bombVisual, float fuse, float radius, int damage)
    {
        visual = bombVisual;
        fuseDuration = Mathf.Max(0.01f, fuse);
        explosionRadius = Mathf.Max(0.1f, radius);
        damageUnits = Mathf.Max(1, damage);
    }

    private void Start()
    {
        StartCoroutine(FuseRoutine());
    }

    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseDuration);
        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        HashSet<IExplosionReceiver> notifiedReceivers = new HashSet<IExplosionReceiver>();
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, explosionRadius, overlapResults);
        for (int index = 0; index < hitCount; index++)
        {
            Collider2D hit = overlapResults[index];
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null && damagedTargets.Add(damageable))
            {
                damageable.TakeDamage(damageUnits);
            }

            IExplosionReceiver receiver = hit.GetComponentInParent<IExplosionReceiver>();
            if (receiver != null && notifiedReceivers.Add(receiver))
            {
                receiver.ReceiveExplosion(transform.position, explosionRadius);
            }
        }

        if (visual != null)
        {
            visual.color = new Color(1f, 0.5f, 0.08f, 0.72f);
            visual.transform.localScale = Vector3.one * (explosionRadius * 2f);
        }
        Destroy(gameObject, 0.12f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0.08f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
