using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TurnUIBuilder : EditorWindow
{
    [MenuItem("TicTacToe/Generate Turn UI")]
    public static void GenerateUI()
    {
        // 1. Canvas setup
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // 2. Turn UI Container (Black Box)
        GameObject turnContainer = new GameObject("TurnUIContainer");
        turnContainer.transform.SetParent(canvas.transform, false);
        RectTransform containerRect = turnContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0, -50);
        containerRect.sizeDelta = new Vector2(300, 120);
        Image bgImage = turnContainer.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.9f);

        // Load Sprites
        Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
        Sprite crossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Cross.png");

        // 3. Circle (O) Setup
        GameObject circleGO = new GameObject("CircleIcon");
        circleGO.transform.SetParent(turnContainer.transform, false);
        RectTransform circleRect = circleGO.AddComponent<RectTransform>();
        circleRect.anchoredPosition = new Vector2(-40, 10);
        circleRect.sizeDelta = new Vector2(70, 70);
        Image circleImage = circleGO.AddComponent<Image>();
        if (circleSprite != null) circleImage.sprite = circleSprite;

        // 4. Cross (X) Setup
        GameObject crossGO = new GameObject("CrossIcon");
        crossGO.transform.SetParent(turnContainer.transform, false);
        RectTransform crossRect = crossGO.AddComponent<RectTransform>();
        crossRect.anchoredPosition = new Vector2(60, 10);
        crossRect.sizeDelta = new Vector2(70, 70);
        Image crossImage = crossGO.AddComponent<Image>();
        if (crossSprite != null) crossImage.sprite = crossSprite;

        // 5. Circle Arrow (Using Image instead of Text for Unity 6 compatibility)
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject circleArrowGO = new GameObject("CircleArrow");
        circleArrowGO.transform.SetParent(turnContainer.transform, false);
        RectTransform circleArrowRect = circleArrowGO.AddComponent<RectTransform>();
        circleArrowRect.anchoredPosition = new Vector2(-95, 10); // Left of Circle
        circleArrowRect.sizeDelta = new Vector2(30, 10); // A small horizontal dash/line
        Image circleArrowImage = circleArrowGO.AddComponent<Image>();
        circleArrowImage.color = Color.white;

        // 6. Cross Arrow (Using Image instead of Text)
        GameObject crossArrowGO = new GameObject("CrossArrow");
        crossArrowGO.transform.SetParent(turnContainer.transform, false);
        RectTransform crossArrowRect = crossArrowGO.AddComponent<RectTransform>();
        crossArrowRect.anchoredPosition = new Vector2(5, 10); // Left of Cross
        crossArrowRect.sizeDelta = new Vector2(30, 10);
        Image crossArrowImage = crossArrowGO.AddComponent<Image>();
        crossArrowImage.color = Color.white;

        // 7. "YOU" Text (Below Cross) - Try to use Text if possible, but it might fail in Unity 6 without TMP
        GameObject youTextGO = new GameObject("YouText");
        youTextGO.transform.SetParent(turnContainer.transform, false);
        RectTransform youRect = youTextGO.AddComponent<RectTransform>();
        youRect.anchoredPosition = new Vector2(60, -35); // Under Cross
        youRect.sizeDelta = new Vector2(100, 30);
        Text youText = youTextGO.AddComponent<Text>();
        youText.text = "YOU";
        if (defaultFont != null) youText.font = defaultFont;
        youText.color = Color.white;
        youText.fontSize = 24;
        youText.alignment = TextAnchor.MiddleCenter;
        youText.fontStyle = FontStyle.Bold;

        // 8. Attach Script and Assign References
        GameTurnUI turnUI = turnContainer.AddComponent<GameTurnUI>();
        SerializedObject so = new SerializedObject(turnUI);
        so.FindProperty("crossArrow").objectReferenceValue = crossArrowGO;
        so.FindProperty("circleArrow").objectReferenceValue = circleArrowGO;
        so.ApplyModifiedProperties();

        // 9. Initial state update
        circleArrowGO.SetActive(false); // Assume Cross goes first by default in GameManager, GameTurnUI will update it correctly on Start.

        // Ensure the scene is marked dirty so changes are saved, but only if not in play mode.
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        Debug.Log("Turn UI Successfully Generated! All scripts and objects are linked.");
    }
}
