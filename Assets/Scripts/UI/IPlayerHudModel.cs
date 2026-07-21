using System;

public interface IPlayerHudModel
{
    IPlayerInput Input { get; }
    bool IsDead { get; }
    int CurrentHealthUnits { get; }
    int MaxHealthUnits { get; }
    int Coins { get; }
    int Bombs { get; }
    bool IsDashing { get; }
    float DashCooldownRemaining { get; }
    float MoveSpeed { get; }
    float FireRate { get; }
    float Damage { get; }
    float Range { get; }
    float ProjectileSpeed { get; }
    float Luck { get; }

    event Action<int, int> HealthChanged;
    event Action Died;
    event Action ResourcesChanged;
    event Action StatsChanged;
    event Action<string> PromptChanged;
}

/// <summary>
/// Adapts the gameplay aggregate to the small read-only surface required by UI.
/// </summary>
public sealed class PlayerHudModel : IPlayerHudModel
{
    private readonly Player player;
    private readonly PlayerDash dash;
    private readonly PlayerInteractor interactor;

    public PlayerHudModel(Player playerReference)
    {
        player = playerReference ?? throw new ArgumentNullException(nameof(playerReference));
        dash = player.GetComponent<PlayerDash>();
        interactor = player.GetComponent<PlayerInteractor>();
        if (dash == null || interactor == null)
        {
            throw new InvalidOperationException("Player HUD requires PlayerDash and PlayerInteractor components.");
        }
    }

    public IPlayerInput Input => player.Input;
    public bool IsDead => player.Health.IsDead;
    public int CurrentHealthUnits => player.Health.CurrentHealthUnits;
    public int MaxHealthUnits => player.Health.MaxHealthUnits;
    public int Coins => player.Inventory.Coins;
    public int Bombs => player.Inventory.Bombs;
    public bool IsDashing => dash.IsDashing;
    public float DashCooldownRemaining => dash.CooldownRemaining;
    public float MoveSpeed => player.Stats.MoveSpeed;
    public float FireRate => player.Stats.FireRate;
    public float Damage => player.Stats.Damage;
    public float Range => player.Stats.Range;
    public float ProjectileSpeed => player.Stats.ProjectileSpeed;
    public float Luck => player.Stats.Luck;

    public event Action<int, int> HealthChanged
    {
        add => player.Health.HealthChanged += value;
        remove => player.Health.HealthChanged -= value;
    }

    public event Action Died
    {
        add => player.Health.Died += value;
        remove => player.Health.Died -= value;
    }

    public event Action ResourcesChanged
    {
        add => player.Inventory.ResourcesChanged += value;
        remove => player.Inventory.ResourcesChanged -= value;
    }

    public event Action StatsChanged
    {
        add => player.Stats.Changed += value;
        remove => player.Stats.Changed -= value;
    }

    public event Action<string> PromptChanged
    {
        add => interactor.PromptChanged += value;
        remove => interactor.PromptChanged -= value;
    }
}
