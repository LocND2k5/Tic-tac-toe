using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class MenuSceneBuilder : EditorWindow
{
    [MenuItem("TicTacToe/Generate Menu Scene")]
    public static void GenerateMenuScene()
    {
        // Ask for confirmation to discard current scene changes
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // Create new empty scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Setup Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // 2. Setup EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        // 3. Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvas.transform, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.95f, 0.95f, 0.95f);

        // 4. Title Text
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(canvas.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 300);
        titleRect.sizeDelta = new Vector2(800, 150);
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "CHOOSE A GRID";
        titleText.font = defaultFont;
        titleText.fontSize = 80;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.6f, 0f);
        titleText.fontStyle = FontStyle.Bold;

        // 5. Manager
        GameObject managerGO = new GameObject("MenuManager");
        managerGO.AddComponent<MainMenuUI>();

        // 6. Buttons
        CreateButton(canvas.transform, "Button 3x3", -350, 3, defaultFont, new Color(0f, 0.5f, 1f));
        CreateButton(canvas.transform, "Button 5x5", 0, 5, defaultFont, new Color(0.4f, 0.4f, 0.4f));
        CreateButton(canvas.transform, "Button 7x7", 350, 7, defaultFont, new Color(0.4f, 0.4f, 0.4f));

        // 7. Save Scene
        string scenePath = "Assets/Scenes/MenuScene.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);

        // 8. Add to Build Settings (MenuScene first, then SampleScene)
        EditorBuildSettingsScene menuBuildScene = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettingsScene gameBuildScene = new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true);
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[] { menuBuildScene, gameBuildScene };

        Debug.Log("Menu Scene Successfully Generated and Added to Build Settings!");
    }

    private static void CreateButton(Transform parent, string name, float posX, int size, Font font, Color color)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);
        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, 0);
        rect.sizeDelta = new Vector2(300, 350); 
        
        Image img = buttonGO.AddComponent<Image>();
        img.color = color;
        
        Button btn = buttonGO.AddComponent<Button>();
        
        // Add dynamic linking script
        LevelSelectButton lsb = buttonGO.AddComponent<LevelSelectButton>();
        lsb.boardSize = size;

        // Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, 100);
        textRect.sizeDelta = new Vector2(300, 100);
        
        Text txt = textGO.AddComponent<Text>();
        txt.text = size + "X" + size;
        txt.font = font;
        txt.fontSize = 70;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontStyle = FontStyle.Bold;
    }

    [MenuItem("TicTacToe/Auto Setup Game Scene (Run in SampleScene)")]
    public static void AutoSetupGameScene()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SampleScene")
        {
            Debug.LogError("LỖI: Bạn phải mở file SampleScene.unity lên trước khi ấn nút này!");
            return;
        }

        BoardGenerator bg = FindObjectOfType<BoardGenerator>();
        if (bg == null)
        {
            GameObject bgGO = new GameObject("DynamicBoard");
            bg = bgGO.AddComponent<BoardGenerator>();
        }

        Sprite lineSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Line.png");
        SerializedObject so = new SerializedObject(bg);
        so.FindProperty("lineSprite").objectReferenceValue = lineSprite;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Thành công! Đã tự động tạo và gắn Line.png vào BoardGenerator.");
    }
}
