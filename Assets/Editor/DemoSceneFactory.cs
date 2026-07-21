using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DemoSceneFactory
{
    public const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    public const string GameScenePath = "Assets/Scenes/Game.unity";

    public static bool AreGeneratedScenesAvailable()
    {
        return File.Exists(MainMenuScenePath) && File.Exists(GameScenePath);
    }

    public static void CreateOrUpdateScenes(
        GameObject playerPrefab,
        GameObject dungeonRoomPrefab,
        RoomPrefabCatalog roomPrefabCatalog,
        GameObject hudPrefab,
        Sprite uiSprite)
    {
        CreateGameScene(playerPrefab, dungeonRoomPrefab, roomPrefabCatalog, hudPrefab);
        CreateMainMenuScene(uiSprite);
        ConfigureBuildScenes();
    }

    public static void CreateGameScene(
        GameObject playerPrefab,
        GameObject dungeonRoomPrefab,
        RoomPrefabCatalog roomPrefabCatalog,
        GameObject hudPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Game";

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 8f;
        camera.backgroundColor = new Color(0.055f, 0.065f, 0.085f);
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<UniversalAdditionalCameraData>();

        GameObject lightObject = new GameObject("Global Light 2D");
        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;

        GameObject playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        playerObject.transform.position = Vector3.zero;

        Transform roomRoot = new GameObject("Dungeon Rooms").transform;

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.localScale = Vector3.one;

        GameObject hudObject = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, canvasObject.transform);
        RectTransform hudRect = hudObject.GetComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.pivot = new Vector2(0.5f, 0.5f);
        hudRect.anchoredPosition = Vector2.zero;
        hudRect.sizeDelta = Vector2.zero;
        hudRect.localScale = Vector3.one;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();

        GameObject gameSessionObject = new GameObject("Game Session");
        GameSession gameSession = gameSessionObject.AddComponent<GameSession>();
        gameSession.Configure(
            playerObject.GetComponent<Player>(),
            hudObject.GetComponent<StageAHudController>(),
            hudObject.GetComponent<MinimapController>(),
            camera,
            dungeonRoomPrefab.GetComponent<RoomController>(),
            roomPrefabCatalog,
            roomRoot,
            false,
            20260720);

        EditorSceneManager.SaveScene(scene, GameScenePath);
        EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
    }

    public static void CreateMainMenuScene(Sprite sprite)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.backgroundColor = new Color(0.018f, 0.024f, 0.038f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Image background = DemoUiFactory.CreateImage("Background", canvasObject.transform, sprite, new Color(0.018f, 0.024f, 0.038f, 1f));
        DemoUiFactory.Stretch(background.rectTransform, 0f, 0f, 0f, 0f);
        Image accent = DemoUiFactory.CreateImage("Accent", background.transform, sprite, new Color(0.12f, 0.58f, 0.78f, 0.22f));
        DemoUiFactory.SetAnchoredRect(accent.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 185f), new Vector2(760f, 8f));

        Text title = DemoUiFactory.CreateText("Game Title", background.transform, "ErrorGame", 92, TextAnchor.MiddleCenter, new Color(0.86f, 0.94f, 1f));
        DemoUiFactory.SetAnchoredRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 285f), new Vector2(900f, 130f));
        Text subtitle = DemoUiFactory.CreateText("Subtitle", background.transform, "MULTI-FLOOR DUNGEON", 22, TextAnchor.MiddleCenter, new Color(0.42f, 0.66f, 0.82f));
        DemoUiFactory.SetAnchoredRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(600f, 44f));

        Button startButton = DemoUiFactory.CreateButton("Start Game", background.transform, sprite, "START GAME");
        Button continueButton = DemoUiFactory.CreateButton("Continue", background.transform, sprite, "CONTINUE");
        Button quitButton = DemoUiFactory.CreateButton("Quit Game", background.transform, sprite, "QUIT GAME");
        DemoUiFactory.SetAnchoredRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(420f, 76f));
        DemoUiFactory.SetAnchoredRect(continueButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(420f, 76f));
        DemoUiFactory.SetAnchoredRect(quitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(420f, 76f));

        Text saveHint = DemoUiFactory.CreateText("Save Hint", background.transform, "Progress is saved automatically", 19, TextAnchor.MiddleCenter, new Color(0.48f, 0.54f, 0.64f));
        DemoUiFactory.SetAnchoredRect(saveHint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 45f), new Vector2(600f, 36f));

        MainMenuController menu = canvasObject.AddComponent<MainMenuController>();
        menu.Configure(startButton, continueButton, quitButton);

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
    }

    public static void ConfigureBuildScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
    }
}
