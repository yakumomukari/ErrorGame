using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInventory : MonoBehaviour
{
    [SerializeField, Min(0)] private int startingCoins;
    [SerializeField, Min(0)] private int startingBombs = 3;

    public int Coins { get; private set; }
    public int Bombs { get; private set; }

    public event Action ResourcesChanged;

    private void Awake()
    {
        Coins = startingCoins;
        Bombs = startingBombs;
    }

    private void Start()
    {
        ResourcesChanged?.Invoke();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        ResourcesChanged?.Invoke();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0 || Coins < amount) return false;
        Coins -= amount;
        ResourcesChanged?.Invoke();
        return true;
    }

    public void AddBombs(int amount)
    {
        if (amount <= 0) return;
        Bombs += amount;
        ResourcesChanged?.Invoke();
    }

    public bool SpendBomb()
    {
        if (Bombs <= 0) return false;
        Bombs--;
        ResourcesChanged?.Invoke();
        return true;
    }

    public void Restore(int coins, int bombs)
    {
        Coins = Mathf.Max(0, coins);
        Bombs = Mathf.Max(0, bombs);
        ResourcesChanged?.Invoke();
    }
}
