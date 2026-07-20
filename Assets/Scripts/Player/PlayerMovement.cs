using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerStats))]
[RequireComponent(typeof(GameInputReader))]
[DisallowMultipleComponent]
public sealed class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private PlayerStats stats;
    private GameInputReader input;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;
    private bool isDashing;
    private Vector2 dashVelocity;

    public Vector2 MoveInput => moveInput;
    public Vector2 LastMoveDirection => lastMoveDirection;
    public bool IsDashing => isDashing;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        input = GetComponent<GameInputReader>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (isDashing)
        {
            return;
        }

        moveInput = input.Move;
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        if (moveInput.sqrMagnitude > 0.001f)
        {
            lastMoveDirection = moveInput.normalized;
        }
    }

    private void FixedUpdate()
    {
        Vector2 velocity = isDashing ? dashVelocity : moveInput * stats.MoveSpeed;
        body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
    }

    public void BeginDash(Vector2 direction, float speed)
    {
        isDashing = true;
        moveInput = Vector2.zero;
        dashVelocity = direction.normalized * speed;
    }

    public void EndDash()
    {
        isDashing = false;
        dashVelocity = Vector2.zero;
    }
}
