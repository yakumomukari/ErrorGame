using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class StageAHudController : MonoBehaviour
{
    [Header("HUD References")]
    [SerializeField] private RectTransform heartsRoot;
    [SerializeField] private RectTransform heartTemplate;
    [SerializeField] private Text coinText;
    [SerializeField] private Text bombText;
    [SerializeField] private Text promptText;
    [SerializeField] private Text dashText;
    [SerializeField] private Text statsText;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private StageAPauseMenuController pauseMenu;

    private readonly List<Image> heartFills = new List<Image>();
    private Player player;
    private PlayerInteractor interactor;
    private PlayerDash dash;
    private GameInputReader input;

    public void Bind(Player playerReference)
    {
        if (player != null) Unsubscribe();

        player = playerReference;
        interactor = player.GetComponent<PlayerInteractor>();
        dash = player.GetComponent<PlayerDash>();
        input = player.Input;

        player.Health.HealthChanged += OnHealthChanged;
        player.Health.Died += OnPlayerDied;
        player.Inventory.ResourcesChanged += RefreshResources;
        player.Stats.Changed += RefreshStats;
        interactor.PromptChanged += OnPromptChanged;

        pauseMenu.Bind(player);
        deathPanel.SetActive(false);
        BuildHearts();
        RefreshHearts();
        RefreshResources();
        RefreshStats();
        OnPromptChanged(string.Empty);
    }

    public void Configure(
        RectTransform heartsContainer,
        RectTransform heartSlotTemplate,
        Text coins,
        Text bombs,
        Text prompt,
        Text dashState,
        Text playerStats,
        GameObject deathOverlay,
        StageAPauseMenuController pauseController)
    {
        heartsRoot = heartsContainer;
        heartTemplate = heartSlotTemplate;
        coinText = coins;
        bombText = bombs;
        promptText = prompt;
        dashText = dashState;
        statsText = playerStats;
        deathPanel = deathOverlay;
        pauseMenu = pauseController;
    }

    private void Update()
    {
        if (player == null) return;

        dashText.text = dash.IsDashing
            ? "DASH"
            : dash.CooldownRemaining > 0f ? $"Dash {dash.CooldownRemaining:0.0}s" : "Dash READY";

        if (player.Health.IsDead && input.RestartPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void BuildHearts()
    {
        foreach (Image fill in heartFills)
        {
            if (fill != null) Destroy(fill.transform.parent.gameObject);
        }
        heartFills.Clear();

        int heartCount = Mathf.CeilToInt(player.Health.MaxHealthUnits / 2f);
        for (int i = 0; i < heartCount; i++)
        {
            RectTransform slot = Instantiate(heartTemplate, heartsRoot);
            slot.name = $"Heart {i + 1}";
            slot.gameObject.SetActive(true);
            Image fill = slot.Find("Fill").GetComponent<Image>();
            heartFills.Add(fill);
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (Mathf.CeilToInt(max / 2f) != heartFills.Count) BuildHearts();
        RefreshHearts();
    }

    private void RefreshHearts()
    {
        for (int i = 0; i < heartFills.Count; i++)
        {
            int units = Mathf.Clamp(player.Health.CurrentHealthUnits - i * 2, 0, 2);
            heartFills[i].fillAmount = units / 2f;
        }
    }

    private void RefreshResources()
    {
        coinText.text = player.Inventory.Coins.ToString();
        bombText.text = player.Inventory.Bombs.ToString();
    }

    private void RefreshStats()
    {
        if (player == null) return;

        PlayerStats stats = player.Stats;
        statsText.text =
            "玩家属性\n\n" +
            $"移速      {stats.MoveSpeed:0.00}\n" +
            $"射速      {stats.FireRate:0.00}/秒\n" +
            $"伤害      {stats.Damage:0.00}\n" +
            $"射程      {stats.Range:0.00}\n" +
            $"弹速      {stats.ProjectileSpeed:0.00}\n" +
            $"幸运      {stats.Luck:0.00}";
    }

    private void OnPromptChanged(string prompt)
    {
        promptText.text = prompt;
    }

    private void OnPlayerDied()
    {
        pauseMenu.ForceClose();
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Unsubscribe()
    {
        player.Health.HealthChanged -= OnHealthChanged;
        player.Health.Died -= OnPlayerDied;
        player.Inventory.ResourcesChanged -= RefreshResources;
        player.Stats.Changed -= RefreshStats;
        if (interactor != null) interactor.PromptChanged -= OnPromptChanged;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        if (player != null) Unsubscribe();
    }
}
