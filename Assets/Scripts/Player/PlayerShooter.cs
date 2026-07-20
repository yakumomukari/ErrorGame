using UnityEngine;

[RequireComponent(typeof(PlayerStats), typeof(PlayerAim))]
[RequireComponent(typeof(GameInputReader))]
[DisallowMultipleComponent]
public sealed class PlayerShooter : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private Projectile projectilePrefab;
    private PlayerStats stats;
    private PlayerAim aim;
    private GameInputReader input;
    private float nextShotTime;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        aim = GetComponent<PlayerAim>();
        input = GetComponent<GameInputReader>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (input.FireHeld && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + stats.ShotInterval;
        }
    }

    public void Configure(Transform muzzleTransform, Projectile prefab)
    {
        muzzle = muzzleTransform;
        projectilePrefab = prefab;
    }

    private void Shoot()
    {
        Vector2 spawnPosition = muzzle != null
            ? muzzle.position
            : (Vector2)transform.position + aim.AimDirection * 0.65f;

        if (projectilePrefab == null)
        {
            Debug.LogError("PlayerShooter requires a Projectile prefab.", this);
            return;
        }

        Projectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        projectile.Initialize(aim.AimDirection, stats.ProjectileSpeed, stats.Range, stats.Damage);
    }
}
