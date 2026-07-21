using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageAPauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject overlay;
    [SerializeField] private Text tabContent;
    [SerializeField] private Button itemsButton;
    [SerializeField] private Button notebookButton;
    [SerializeField] private Button logButton;
    [SerializeField] private Button mainMenuButton;

    private IPlayerHudModel hudModel;
    private IPlayerInput input;
    private ISceneNavigator sceneNavigator;
    private bool listenersRegistered;

    public bool IsPaused { get; private set; }

    public void Bind(Player playerReference)
    {
        Bind(new PlayerHudModel(playerReference));
    }

    public void Bind(IPlayerHudModel model)
    {
        hudModel = model ?? throw new ArgumentNullException(nameof(model));
        input = hudModel.Input;
        if (sceneNavigator == null) sceneNavigator = new UnitySceneNavigator();
        if (!listenersRegistered)
        {
            itemsButton.onClick.AddListener(ShowItems);
            notebookButton.onClick.AddListener(ShowNotebook);
            logButton.onClick.AddListener(ShowLog);
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            listenersRegistered = true;
        }

        ShowItems();
        ForceClose();
    }

    public void SetSceneNavigator(ISceneNavigator navigator)
    {
        sceneNavigator = navigator;
    }

    public void Configure(
        GameObject pauseOverlay,
        Text content,
        Button items,
        Button notebook,
        Button log,
        Button returnToMainMenu)
    {
        overlay = pauseOverlay;
        tabContent = content;
        itemsButton = items;
        notebookButton = notebook;
        logButton = log;
        mainMenuButton = returnToMainMenu;
    }

    private void Update()
    {
        if (hudModel == null || hudModel.IsDead) return;
        if (input.PausePressedThisFrame) TogglePause();

        if (!IsPaused) return;
        if (input.ItemsTabPressedThisFrame) ShowItems();
        if (input.NotebookTabPressedThisFrame) ShowNotebook();
        if (input.LogTabPressedThisFrame) ShowLog();
    }

    public void TogglePause()
    {
        if (IsPaused) Resume(); else Pause();
    }

    public void ForceClose()
    {
        IsPaused = false;
        if (overlay != null) overlay.SetActive(false);
    }

    private void Pause()
    {
        IsPaused = true;
        overlay.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        IsPaused = false;
        overlay.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ReturnToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        sceneNavigator.LoadMainMenu();
    }

    private void ShowItems() => SelectTab("ITEMS", "Item collection is not implemented yet");
    private void ShowNotebook() => SelectTab("NOTEBOOK", "Notebook is not implemented yet");
    private void ShowLog() => SelectTab("LOG", "Log is not implemented yet");

    private void SelectTab(string title, string message)
    {
        tabContent.text = $"{title}\n\n{message}";
    }

    private void OnDestroy()
    {
        if (listenersRegistered && mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        }
        if (IsPaused) Time.timeScale = 1f;
    }
}
