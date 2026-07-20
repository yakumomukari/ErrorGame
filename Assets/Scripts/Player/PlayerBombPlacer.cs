using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(GameInputReader))]
[DisallowMultipleComponent]
public sealed class PlayerBombPlacer : MonoBehaviour
{
    [SerializeField] private Bomb bombPrefab;
    [SerializeField, Min(0f)] private float placementCooldown = 1f;

    private PlayerInventory inventory;
    private GameInputReader input;
    private float nextPlacementTime;

    public float CooldownRemaining => Mathf.Max(0f, nextPlacementTime - Time.time);

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        input = GetComponent<GameInputReader>();
    }

    public void Configure(Bomb prefab, float cooldown)
    {
        bombPrefab = prefab;
        placementCooldown = Mathf.Max(0f, cooldown);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (input.BombPressedThisFrame) TryPlaceBomb();
    }

    public bool TryPlaceBomb()
    {
        if (bombPrefab == null || Time.time < nextPlacementTime) return false;
        if (!inventory.SpendBomb()) return false;

        Instantiate(bombPrefab, transform.position, Quaternion.identity);
        nextPlacementTime = Time.time + placementCooldown;
        return true;
    }
}
