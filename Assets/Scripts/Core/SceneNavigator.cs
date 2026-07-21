using UnityEngine;
using UnityEngine.SceneManagement;

public interface ISceneNavigator
{
    void LoadGame();
    void LoadMainMenu();
    void ReloadCurrentScene();
    void QuitGame();
}

/// <summary>
/// Single Unity-specific scene boundary used by menu and HUD flow code.
/// </summary>
public sealed class UnitySceneNavigator : ISceneNavigator
{
    public const string GameSceneName = "Game";
    public const string MainMenuSceneName = "MainMenu";

    public void LoadGame()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
