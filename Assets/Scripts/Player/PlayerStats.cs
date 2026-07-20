using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerStats : MonoBehaviour
{
    [Header("Stage A Base Stats")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0.01f)] private float fireRate = 5f;
    [SerializeField, Min(0f)] private float damage = 1f;
    [SerializeField, Min(0.1f)] private float range = 8f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 12f;
    [SerializeField] private float luck;

    public float MoveSpeed => moveSpeed;
    public float FireRate => fireRate;
    public float Damage => damage;
    public float Range => range;
    public float ProjectileSpeed => projectileSpeed;
    public float Luck => luck;
    public float ShotInterval => 1f / Mathf.Max(0.01f, fireRate);

    public event Action Changed;

    public void AddMoveSpeed(float amount) { moveSpeed = Mathf.Max(0f, moveSpeed + amount); NotifyChanged(); }
    public void AddFireRate(float amount) { fireRate = Mathf.Max(0.01f, fireRate + amount); NotifyChanged(); }
    public void AddDamage(float amount) { damage = Mathf.Max(0f, damage + amount); NotifyChanged(); }
    public void AddRange(float amount) { range = Mathf.Max(0.1f, range + amount); NotifyChanged(); }
    public void AddProjectileSpeed(float amount) { projectileSpeed = Mathf.Max(0.1f, projectileSpeed + amount); NotifyChanged(); }
    public void AddLuck(float amount) { luck += amount; NotifyChanged(); }

    public void MultiplyAll(float multiplier)
    {
        multiplier = Mathf.Max(0f, multiplier);
        moveSpeed = Mathf.Max(0f, moveSpeed * multiplier);
        fireRate = Mathf.Max(0.01f, fireRate * multiplier);
        damage = Mathf.Max(0f, damage * multiplier);
        range = Mathf.Max(0.1f, range * multiplier);
        projectileSpeed = Mathf.Max(0.1f, projectileSpeed * multiplier);
        luck *= multiplier;
        NotifyChanged();
    }

    public void Restore(
        float savedMoveSpeed,
        float savedFireRate,
        float savedDamage,
        float savedRange,
        float savedProjectileSpeed,
        float savedLuck)
    {
        moveSpeed = Mathf.Max(0f, savedMoveSpeed);
        fireRate = Mathf.Max(0.01f, savedFireRate);
        damage = Mathf.Max(0f, savedDamage);
        range = Mathf.Max(0.1f, savedRange);
        projectileSpeed = Mathf.Max(0.1f, savedProjectileSpeed);
        luck = savedLuck;
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
