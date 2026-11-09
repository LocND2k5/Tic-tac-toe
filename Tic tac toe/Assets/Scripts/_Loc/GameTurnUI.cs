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

    private RectTransform crossIconRect;
    private RectTransform circleIconRect;
    private float textOffsetY;
    private bool hasTextOffsetY;

    private float arrowOffsetY;
    private const float ArrowGap = 5f; // khoảng hở giữa mũi tên và icon

    // Mũi tên đứng ngay bên trái icon của nó, nên khi dời icon mũi tên đi theo
    private void PlaceArrowBesideIcon(GameObject arrow, RectTransform icon)
    {
        if (arrow == null || icon == null) return;

        RectTransform arrowRect = arrow.GetComponent<RectTransform>();
        if (arrowRect == null) return;

        float iconHalf = icon.rect.width * icon.lossyScale.x * 0.5f;
        float arrowHalf = arrowRect.rect.width * arrowRect.lossyScale.x * 0.5f;
        float gap = ArrowGap * icon.lossyScale.x;

        Vector3 pos = arrowRect.position;
        pos.x = icon.position.x - iconHalf - arrowHalf - gap;
        pos.y = icon.position.y + arrowOffsetY;
        arrowRect.position = pos;
    }

    private RectTransform FindChildRect(string childName)
    {
        foreach (RectTransform rt in GetComponentsInChildren<RectTransform>(true))
        {
            if (rt.name == childName) return rt;
        }
        return null;
    }

    private void Start()
    {
        crossIconRect = FindChildRect("CrossIcon");
        circleIconRect = FindChildRect("CircleIcon");

        // Khoảng cách dọc giữa mũi tên O và icon O (lấy làm chuẩn cho cả hai mũi tên)
        RectTransform circleArrowRect = circleArrow != null ? circleArrow.GetComponent<RectTransform>() : null;
        if (circleArrowRect != null && circleIconRect != null)
        {
            arrowOffsetY = circleArrowRect.position.y - circleIconRect.position.y;
        }

        // Ghi nhớ khoảng cách dọc giữa chữ và icon gần nó nhất, đúng như bạn đã đặt trong Editor
        if (youTextRect != null && crossIconRect != null && circleIconRect != null)
        {
            float toCross = Mathf.Abs(youTextRect.position.x - crossIconRect.position.x);
            float toCircle = Mathf.Abs(youTextRect.position.x - circleIconRect.position.x);
            RectTransform nearest = toCross <= toCircle ? crossIconRect : circleIconRect;
            textOffsetY = youTextRect.position.y - nearest.position.y;
            hasTextOffsetY = true;
        }

        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnRematch += GameManager_OnRematch;

        SetupNames();
        UpdateTurnUI();
    }

    // Multiplayer: chữ "YOU" được thay bằng tên người chơi đang tới lượt.
    // Training (đấu máy): "YOU" khi tới lượt bạn, "BOT" khi tới lượt máy.
    private string crossName, circleName;
    private bool nameInYouText;
    private TMP_Text youText;
    private UnityEngine.UI.Text youLegacyText;

    private void SetupNames()
    {
        if (GameManager.Instance.IsNetworkActive())
        {
            bool isHost = Unity.Netcode.NetworkManager.Singleton.IsServer;
            string opponent = string.IsNullOrEmpty(PlayerProfile.OpponentName) ? "Opponent" : PlayerProfile.OpponentName;
            crossName = isHost ? PlayerProfile.Name : opponent;
            circleName = isHost ? opponent : PlayerProfile.Name;
            Debug.Log($"[GameTurnUI] isHost={isHost} localName='{PlayerProfile.Name}' opponentName='{PlayerProfile.OpponentName}'");
            PrepareYouText();
        }
        else if (Khang.Core.GameConfig.SelectedMode == Khang.Core.GameMode.AI)
        {
            crossName = "YOU";
            circleName = "BOT";
            PrepareYouText();
        }
    }

    private void PrepareYouText()
    {
        if (youTextRect != null)
        {
            youText = youTextRect.GetComponentInChildren<TMP_Text>(true);
            youLegacyText = youTextRect.GetComponentInChildren<UnityEngine.UI.Text>(true);
        }

        if (youText != null)
        {
            nameInYouText = true;
            youText.textWrappingMode = TextWrappingModes.NoWrap;
            youText.overflowMode = TextOverflowModes.Overflow;
            youText.enableAutoSizing = true;
            youText.fontSizeMin = 8;
            youText.fontSizeMax = youText.fontSize;
        }
        else if (youLegacyText != null)
        {
            // Scene đang dùng UI Text cũ (không phải TextMeshPro)
            nameInYouText = true;
            youLegacyText.horizontalOverflow = HorizontalWrapMode.Overflow;
            youLegacyText.resizeTextForBestFit = true;
            youLegacyText.resizeTextMinSize = 8;
            youLegacyText.resizeTextMaxSize = youLegacyText.fontSize;
        }
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

            PlaceArrowBesideIcon(crossArrow, crossIconRect);
            PlaceArrowBesideIcon(circleArrow, circleIconRect);
        }

        // Position the "YOU" text badge to clearly identify local player role
        if (youTextRect != null)
        {
            // Căn chữ ngay dưới icon của người đang tới lượt (theo vị trí thật của icon trong scene)
            RectTransform targetIcon = currentPlayerType == GameManager.PlayerType.Cross ? crossIconRect
                : currentPlayerType == GameManager.PlayerType.Circle ? circleIconRect : null;

            if (targetIcon != null)
            {
                Vector3 worldPos = youTextRect.position;
                worldPos.x = targetIcon.position.x;
                if (hasTextOffsetY) worldPos.y = targetIcon.position.y + textOffsetY;
                youTextRect.position = worldPos;
            }
            else
            {
                // Không tìm thấy icon -> dùng vị trí cố định cũ
                Vector2 currentPos = youTextRect.anchoredPosition;
                if (currentPlayerType == GameManager.PlayerType.Cross) currentPos.x = crossYouPosX;
                else if (currentPlayerType == GameManager.PlayerType.Circle) currentPos.x = circleYouPosX;
                youTextRect.anchoredPosition = currentPos;
            }
        }

        if (nameInYouText)
        {
            string currentName = currentPlayerType == GameManager.PlayerType.Circle ? circleName : crossName;
            if (youText != null) youText.text = currentName;
            else youLegacyText.text = currentName;
        }
    }
}
