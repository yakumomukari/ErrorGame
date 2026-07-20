using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerHealth), typeof(PlayerAim))]
[RequireComponent(typeof(GameInputReader))]
[DisallowMultipleComponent]
public sealed class PlayerDash : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 0.18f;
    [SerializeField, Min(0.01f)] private float cooldown = 0.8f;
    [SerializeField, Min(0f)] private float speed = 14f;

    private PlayerMovement movement;
    private PlayerHealth health;
    private PlayerAim aim;
    private GameInputReader input;
    private float nextDashTime;

    public bool IsDashing { get; private set; }
    public float CooldownRemaining => Mathf.Max(0f, nextDashTime - Time.time);

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();
        aim = GetComponent<PlayerAim>();
        input = GetComponent<GameInputReader>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (!IsDashing && input.DashPressedThisFrame && Time.time >= nextDashTime)
        {
            // Movement input has priority. Standing dashes follow the current aim,
            // so the player never depends on a stale movement direction.
            Vector2 direction = movement.MoveInput.sqrMagnitude > 0.001f
                ? movement.MoveInput.normalized
                : aim.AimDirection;
            StartCoroutine(DashRoutine(direction));
        }
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        IsDashing = true;
        nextDashTime = Time.time + cooldown;
        movement.BeginDash(direction, speed);
        health.SetDashInvulnerable(true);

        yield return new WaitForSeconds(duration);

        movement.EndDash();
        health.SetDashInvulnerable(false);
        IsDashing = false;
    }

    private void OnDisable()
    {
        if (movement != null) movement.EndDash();
        if (health != null) health.SetDashInvulnerable(false);
        IsDashing = false;
    }
}
