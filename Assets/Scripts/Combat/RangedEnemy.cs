using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
[DisallowMultipleComponent]
public sealed class RangedEnemy : MonoBehaviour, ICombatEnemy
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 1.8f;
    [SerializeField, Min(0f)] private float minimumPreferredDistance = 4.5f;
    [SerializeField, Min(0f)] private float maximumPreferredDistance = 7f;
    [SerializeField, Min(0f)] private float activationDelay = 0.3f;

    [Header("Attack")]
    [SerializeField, Min(0.1f)] private float fireInterval = 1.4f;
    [SerializeField, Min(0f)] private float initialShotDelay = 0.65f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 7f;
    [SerializeField, Min(0.1f)] private float projectileRange = 12f;
    [SerializeField, Min(1)] private int projectileDamageUnits = 1;
    [SerializeField, Min(1)] private int contactDamageUnits = 1;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private SpriteRenderer bodyVisual;

    private Rigidbody2D body;
    private Player target;
    private float activationTime;
    private float nextShotTime;
    private float nextStrafeSwitchTime;
    private float strafeSign = 1f;
    private readonly HashSet<EnemyProjectile> activeProjectiles = new HashSet<EnemyProjectile>();

    public EnemyHealth Health { get; private set; }
    public float ActivationDelay => activationDelay;
    public float FireInterval => fireInterval;
    public EnemyProjectile ProjectilePrefab => projectilePrefab;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        Health = GetComponent<EnemyHealth>();
        Health.Died += OnDied;
        strafeSign = (GetInstanceID() & 1) == 0 ? 1f : -1f;
    }

    public void Configure(EnemyProjectile enemyProjectilePrefab, Transform muzzleTransform, SpriteRenderer visual)
    {
        projectilePrefab = enemyProjectilePrefab;
        muzzle = muzzleTransform;
        bodyVisual = visual;
    }

    public void Initialize(Player playerTarget)
    {
        target = playerTarget;
        activationTime = Time.time + activationDelay;
        nextShotTime = activationTime + initialShotDelay;
        nextStrafeSwitchTime = activationTime + 1.2f;
    }

    private void FixedUpdate()
    {
        if (target == null || target.Health.IsDead || Time.time < activationTime) return;

        Vector2 toTarget = (Vector2)target.transform.position - body.position;
        float distance = toTarget.magnitude;
        Vector2 direction = distance > 0.01f ? toTarget / distance : Vector2.down;
        transform.up = direction;

        if (Time.time >= nextStrafeSwitchTime)
        {
            strafeSign *= -1f;
            nextStrafeSwitchTime = Time.time + 1.2f;
        }

        Vector2 movementDirection;
        if (distance < minimumPreferredDistance)
        {
            movementDirection = -direction;
        }
        else if (distance > maximumPreferredDistance)
        {
            movementDirection = direction;
        }
        else
        {
            movementDirection = new Vector2(-direction.y, direction.x) * strafeSign;
        }
        body.MovePosition(body.position + movementDirection * moveSpeed * Time.fixedDeltaTime);

        if (Time.time >= nextShotTime)
        {
            Fire(direction);
            nextShotTime = Time.time + fireInterval;
        }
    }

    private void Fire(Vector2 direction)
    {
        if (projectilePrefab == null) return;
        Vector3 spawnPosition = muzzle != null
            ? muzzle.position
            : (Vector3)(body.position + direction * 0.75f);
        EnemyProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        projectile.Initialize(direction, projectileSpeed, projectileRange, projectileDamageUnits, this);
        if (bodyVisual != null) bodyVisual.color = new Color(0.72f, 0.42f, 1f);
        Invoke(nameof(RestoreBodyColor), 0.08f);
    }

    private void RestoreBodyColor()
    {
        if (bodyVisual != null) bodyVisual.color = new Color(0.42f, 0.2f, 0.72f);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Player hitPlayer = collision.collider.GetComponentInParent<Player>();
        if (hitPlayer != null) hitPlayer.Health.TakeDamage(contactDamageUnits);
    }

    internal void RegisterProjectile(EnemyProjectile projectile)
    {
        if (projectile != null) activeProjectiles.Add(projectile);
    }

    internal void UnregisterProjectile(EnemyProjectile projectile)
    {
        if (projectile != null) activeProjectiles.Remove(projectile);
    }

    private void OnDied(EnemyHealth health)
    {
        EnemyProjectile[] projectiles = new EnemyProjectile[activeProjectiles.Count];
        activeProjectiles.CopyTo(projectiles);
        activeProjectiles.Clear();
        foreach (EnemyProjectile projectile in projectiles)
        {
            if (projectile != null) Destroy(projectile.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Health != null) Health.Died -= OnDied;
    }
}
