using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    private IGameSaveRepository saveRepository;
    private ISceneNavigator sceneNavigator;

    public void Configure(Button startGame, Button continueGame, Button quitGame)
    {
        startButton = startGame;
        continueButton = continueGame;
        quitButton = quitGame;
    }

    private void Start()
    {
        saveRepository = new JsonGameSaveRepository();
        if (sceneNavigator == null) sceneNavigator = new UnitySceneNavigator();
        startButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueGame);
        quitButton.onClick.AddListener(QuitGame);
        RefreshContinueButton();
    }

    public void StartNewGame()
    {
        saveRepository.Delete();
        GameLaunchContext.RequestNewGame();
        sceneNavigator.LoadGame();
    }

    public void ContinueGame()
    {
        if (!saveRepository.TryLoad(out _))
        {
            RefreshContinueButton();
            return;
        }

        GameLaunchContext.RequestContinue();
        sceneNavigator.LoadGame();
    }

    public void QuitGame()
    {
        sceneNavigator.QuitGame();
    }

    public void SetSceneNavigator(ISceneNavigator navigator)
    {
        sceneNavigator = navigator;
    }

    private void RefreshContinueButton()
    {
        continueButton.interactable = saveRepository.TryLoad(out _);
    }

    private void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(StartNewGame);
        if (continueButton != null) continueButton.onClick.RemoveListener(ContinueGame);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
    }
}
