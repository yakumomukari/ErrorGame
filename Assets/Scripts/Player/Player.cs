using UnityEngine;

[DisallowMultipleComponent]
public sealed class Player : MonoBehaviour
{
    public GameInputReader Input { get; private set; }
    public PlayerStats Stats { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerInventory Inventory { get; private set; }

    private void Awake()
    {
        Input = GetComponent<GameInputReader>();
        Stats = GetComponent<PlayerStats>();
        Movement = GetComponent<PlayerMovement>();
        Health = GetComponent<PlayerHealth>();
        Inventory = GetComponent<PlayerInventory>();
    }
}
