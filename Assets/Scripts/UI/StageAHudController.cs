using System;
using System.Collections.Generic;
using UnityEngine;
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
    private IPlayerHudModel hudModel;
    private IPlayerInput input;
    private ISceneNavigator sceneNavigator;

    public void Bind(Player playerReference)
    {
        Bind(new PlayerHudModel(playerReference));
    }

    public void Bind(IPlayerHudModel model)
    {
        if (hudModel != null) Unsubscribe();

        hudModel = model ?? throw new ArgumentNullException(nameof(model));
        input = hudModel.Input;
        if (sceneNavigator == null) sceneNavigator = new UnitySceneNavigator();

        hudModel.HealthChanged += OnHealthChanged;
        hudModel.Died += OnPlayerDied;
        hudModel.ResourcesChanged += RefreshResources;
        hudModel.StatsChanged += RefreshStats;
        hudModel.PromptChanged += OnPromptChanged;

        pauseMenu.Bind(hudModel);
        deathPanel.SetActive(false);
        BuildHearts();
        RefreshHearts();
        RefreshResources();
        RefreshStats();
        OnPromptChanged(string.Empty);
    }

    public void SetSceneNavigator(ISceneNavigator navigator)
    {
        sceneNavigator = navigator;
        pauseMenu?.SetSceneNavigator(navigator);
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
        if (hudModel == null) return;

        dashText.text = hudModel.IsDashing
            ? "DASH"
            : hudModel.DashCooldownRemaining > 0f
                ? $"Dash {hudModel.DashCooldownRemaining:0.0}s"
                : "Dash READY";

        if (hudModel.IsDead && input.RestartPressedThisFrame)
        {
            Time.timeScale = 1f;
            sceneNavigator.ReloadCurrentScene();
        }
    }

    private void BuildHearts()
    {
        foreach (Image fill in heartFills)
        {
            if (fill != null) Destroy(fill.transform.parent.gameObject);
        }
        heartFills.Clear();

        int heartCount = Mathf.CeilToInt(hudModel.MaxHealthUnits / 2f);
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
            int units = Mathf.Clamp(hudModel.CurrentHealthUnits - i * 2, 0, 2);
            heartFills[i].fillAmount = units / 2f;
        }
    }

    private void RefreshResources()
    {
        coinText.text = hudModel.Coins.ToString();
        bombText.text = hudModel.Bombs.ToString();
    }

    private void RefreshStats()
    {
        if (hudModel == null) return;

        statsText.text =
            "玩家属性\n\n" +
            $"移速      {hudModel.MoveSpeed:0.00}\n" +
            $"射速      {hudModel.FireRate:0.00}/秒\n" +
            $"伤害      {hudModel.Damage:0.00}\n" +
            $"射程      {hudModel.Range:0.00}\n" +
            $"弹速      {hudModel.ProjectileSpeed:0.00}\n" +
            $"幸运      {hudModel.Luck:0.00}";
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
        hudModel.HealthChanged -= OnHealthChanged;
        hudModel.Died -= OnPlayerDied;
        hudModel.ResourcesChanged -= RefreshResources;
        hudModel.StatsChanged -= RefreshStats;
        hudModel.PromptChanged -= OnPromptChanged;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        if (hudModel != null) Unsubscribe();
    }
}
