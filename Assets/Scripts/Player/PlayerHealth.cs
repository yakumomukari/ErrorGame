using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealthUnits = 6;
    [SerializeField, Min(0f)] private float hurtInvulnerabilityDuration = 0.8f;

    private bool hurtInvulnerable;
    private bool dashInvulnerable;

    public int CurrentHealthUnits { get; private set; }
    public int MaxHealthUnits => maxHealthUnits;
    public bool IsDead => CurrentHealthUnits <= 0;
    public bool IsInvulnerable => hurtInvulnerable || dashInvulnerable;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        CurrentHealthUnits = maxHealthUnits;
    }

    private void Start()
    {
        HealthChanged?.Invoke(CurrentHealthUnits, maxHealthUnits);
    }

    public bool TakeDamage(int damageUnits)
    {
        if (damageUnits <= 0 || IsDead || IsInvulnerable) return false;

        CurrentHealthUnits = Mathf.Max(0, CurrentHealthUnits - damageUnits);
        HealthChanged?.Invoke(CurrentHealthUnits, maxHealthUnits);
        if (CurrentHealthUnits == 0)
        {
            Died?.Invoke();
            return true;
        }

        StartCoroutine(HurtInvulnerabilityRoutine());
        return true;
    }

    public int Heal(int healthUnits)
    {
        if (healthUnits <= 0 || IsDead) return 0;
        int previous = CurrentHealthUnits;
        CurrentHealthUnits = Mathf.Min(maxHealthUnits, CurrentHealthUnits + healthUnits);
        HealthChanged?.Invoke(CurrentHealthUnits, maxHealthUnits);
        return CurrentHealthUnits - previous;
    }

    public void IncreaseMaxHealth(int healthUnits, bool healAddedUnits = true)
    {
        if (healthUnits <= 0) return;
        maxHealthUnits += healthUnits;
        if (healAddedUnits) CurrentHealthUnits += healthUnits;
        HealthChanged?.Invoke(CurrentHealthUnits, maxHealthUnits);
    }

    public void Restore(int currentHealth, int maximumHealth)
    {
        maxHealthUnits = Mathf.Max(1, maximumHealth);
        CurrentHealthUnits = Mathf.Clamp(currentHealth, 1, maxHealthUnits);
        hurtInvulnerable = false;
        dashInvulnerable = false;
        HealthChanged?.Invoke(CurrentHealthUnits, maxHealthUnits);
    }

    public void SetDashInvulnerable(bool value)
    {
        dashInvulnerable = value;
    }

    private IEnumerator HurtInvulnerabilityRoutine()
    {
        hurtInvulnerable = true;
        yield return new WaitForSeconds(hurtInvulnerabilityDuration);
        hurtInvulnerable = false;
    }
}
