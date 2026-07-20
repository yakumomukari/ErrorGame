using UnityEngine;

public sealed class StageAHazard : MonoBehaviour
{
    [SerializeField, Min(1)] private int contactDamageUnits = 1;

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(contactDamageUnits);
        }
    }
}
