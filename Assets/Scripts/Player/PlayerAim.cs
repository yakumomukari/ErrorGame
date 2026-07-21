using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerAim : MonoBehaviour
{
    [SerializeField] private Transform aimPivot;
    private Camera mainCamera;
    private IPlayerInput input;

    public Vector2 AimDirection { get; private set; } = Vector2.up;

    private void Awake()
    {
        mainCamera = Camera.main;
        input = PlayerInputResolver.Require(this);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(input.PointerScreenPosition);
        Vector2 direction = mouseWorld - transform.position;
        if (direction.sqrMagnitude < 0.0001f) return;

        AimDirection = direction.normalized;
        if (aimPivot != null)
        {
            aimPivot.up = AimDirection;
        }
    }

    public void SetAimPivot(Transform pivot)
    {
        aimPivot = pivot;
    }

    public void Configure(Transform pivot)
    {
        aimPivot = pivot;
    }
}
