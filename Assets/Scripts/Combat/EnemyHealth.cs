using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealthUnits = 4;

    public int CurrentHealthUnits { get; private set; }
    public int MaxHealthUnits => maxHealthUnits;
    public bool IsDead => CurrentHealthUnits <= 0;

    public event Action<EnemyHealth> Died;

    private void Awake()
    {
        CurrentHealthUnits = maxHealthUnits;
    }

    public void Configure(int maximumHealth)
    {
        maxHealthUnits = Mathf.Max(1, maximumHealth);
        CurrentHealthUnits = maxHealthUnits;
    }

    public bool TakeDamage(int damageUnits)
    {
        if (damageUnits <= 0 || IsDead) return false;

        CurrentHealthUnits = Mathf.Max(0, CurrentHealthUnits - damageUnits);
        if (!IsDead) return true;

        Died?.Invoke(this);
        Destroy(gameObject);
        return true;
    }
}
