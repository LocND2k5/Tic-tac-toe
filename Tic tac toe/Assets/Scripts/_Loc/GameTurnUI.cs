using System;
using UnityEngine;
using TMPro;
using TicTacToe.Multiplayer;

public class GameTurnUI : MonoBehaviour
{
    [Header("Turn Arrow Indicators")]
    [SerializeField] private GameObject crossArrow;
    [SerializeField] private GameObject circleArrow;

    [Header("You Text Indicator")]
    [SerializeField] private RectTransform youTextRect;
    [SerializeField] private float crossYouPosX = 60f;
    [SerializeField] private float circleYouPosX = -40f;

    private void Start()
    {
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnRematch += GameManager_OnRematch;

        SetupNames();
        UpdateTurnUI();
    }

    // Multiplayer: chữ "YOU" được thay bằng tên người chơi đang tới lượt
    private string crossName, circleName;
    private bool nameInYouText;
    private TMP_Text youText;

    private void SetupNames()
    {
        if (GameManager.Instance.IsNetworkActive())
        {
            bool isHost = Unity.Netcode.NetworkManager.Singleton.IsServer;
            string opponent = string.IsNullOrEmpty(PlayerProfile.OpponentName) ? "Opponent" : PlayerProfile.OpponentName;
            crossName = isHost ? PlayerProfile.Name : opponent;
            circleName = isHost ? opponent : PlayerProfile.Name;

            if (youTextRect != null) youText = youTextRect.GetComponentInChildren<TMP_Text>(true);
            if (youText != null)
            {
                nameInYouText = true;
                youText.textWrappingMode = TextWrappingModes.NoWrap;
                youText.overflowMode = TextOverflowModes.Overflow;
                youText.enableAutoSizing = true;
                youText.fontSizeMin = 8;
                youText.fontSizeMax = youText.fontSize;
            }
        }
        else if (Khang.Core.GameConfig.SelectedMode == Khang.Core.GameMode.AI)
        {
            CreateNamesLabel("Computer");
        }
    }

    // Training: hiển thị tên 2 người chơi bên dưới khung lượt chơi
    private void CreateNamesLabel(string opponentName)
    {
        crossName = PlayerProfile.Name;
        circleName = opponentName;

        // Đặt tên ngay bên dưới khung lượt chơi (panel chứa mũi tên và chữ YOU) để không đè lên nó
        RectTransform panel = null;
        if (youTextRect != null) panel = youTextRect.parent as RectTransform;
        if (panel == null) panel = transform as RectTransform;

        GameObject go = new GameObject("PlayerNamesLabel", typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        if (panel != null)
        {
            go.transform.SetParent(panel, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -6);
        }
        else
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) { Destroy(go); return; }
            go.transform.SetParent(canvas.rootCanvas.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -90);
        }
        rt.sizeDelta = new Vector2(500, 36);

        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.text = $"X: {crossName}  vs  O: {circleName}";
        label.fontSize = 20;
        label.enableAutoSizing = true;
        label.fontSizeMin = 10;
        label.fontSizeMax = 20;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrentPlayablePlayerTypeChanged -= GameManager_OnCurrentPlayablePlayerTypeChanged;
            GameManager.Instance.OnRematch -= GameManager_OnRematch;
        }
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateTurnUI();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        UpdateTurnUI();
    }

    private void UpdateTurnUI()
    {
        GameManager.PlayerType currentPlayerType = GameManager.Instance.GetCurrentPlayablePlayerType();

        // Toggling Active State of two arrows
        if (crossArrow != null && circleArrow != null)
        {
            crossArrow.SetActive(currentPlayerType == GameManager.PlayerType.Cross);
            circleArrow.SetActive(currentPlayerType == GameManager.PlayerType.Circle);
        }

        // Position the "YOU" text badge to clearly identify local player role
        if (youTextRect != null)
        {
            Vector2 currentPos = youTextRect.anchoredPosition;
            GameManager.PlayerType localPlayer = currentPlayerType; // Theo lượt hiện tại thay vì vị trí người chơi
            if (localPlayer == GameManager.PlayerType.Cross)
            {
                currentPos.x = crossYouPosX;
            }
            else if (localPlayer == GameManager.PlayerType.Circle)
            {
                currentPos.x = circleYouPosX;
            }
            youTextRect.anchoredPosition = currentPos;
        }

        if (nameInYouText)
        {
            youText.text = currentPlayerType == GameManager.PlayerType.Circle ? circleName : crossName;
        }
    }
}
