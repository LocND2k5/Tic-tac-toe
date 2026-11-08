using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using TicTacToe.Multiplayer;

public class RematchUI : MonoBehaviour
{
    [SerializeField] private Button rematchButton;
    [SerializeField] private TextMeshProUGUI rematchText;

    // Panel "đối thủ muốn đấu lại": chấp nhận / từ chối.
    // Tạo sẵn bằng menu Editor "TicTacToe/Build Rematch Panel" rồi tự chỉnh trong Canvas; nếu để trống sẽ được dựng bằng code.
    [Header("Rematch Request Panel")]
    [SerializeField] private GameObject requestPanel;
    [SerializeField] private TextMeshProUGUI requestPanelText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    private void Awake()
    {
        if (rematchButton == null)
        {
            rematchButton = GetComponentInChildren<Button>(true);
        }

        if (rematchText == null && rematchButton != null)
        {
            rematchText = rematchButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (requestPanel != null) WireRequestPanel();

        if (rematchButton != null)
        {
            rematchButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Rematch();

                    if (GameManager.Instance.IsNetworkActive())
                    {
                        // In online mode, wait for opponent to also accept
                        // Ẩn nền nút, chỉ còn lại dòng chữ chờ
                        rematchButton.interactable = false;
                        SetButtonBackgroundVisible(false);
                        if (rematchText != null)
                        {
                            rematchText.text = MultiplayerConstants.REMATCH_WAITING;
                        }
                    }
                }
            });
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin += GameManager_OnGameWin;
            GameManager.Instance.OnGameTied += GameManager_OnGameTied;
            GameManager.Instance.OnRematch += GameManager_OnRematch;
            GameManager.Instance.OnRematchRequested += GameManager_OnRematchRequested;
            GameManager.Instance.OnRematchDeclined += GameManager_OnRematchDeclined;
        }

        Hide();
        HideRequestPanel();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
            GameManager.Instance.OnGameTied -= GameManager_OnGameTied;
            GameManager.Instance.OnRematch -= GameManager_OnRematch;
            GameManager.Instance.OnRematchRequested -= GameManager_OnRematchRequested;
            GameManager.Instance.OnRematchDeclined -= GameManager_OnRematchDeclined;
        }
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        Show();
    }

    private void GameManager_OnGameTied(object sender, EventArgs e)
    {
        Show();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        Hide();
        HideRequestPanel();
        ResetButtonState();
    }

    private void GameManager_OnRematchRequested(object sender, GameManager.OnRematchRequestedEventArgs e)
    {
        if (GameManager.Instance == null) return;

        // Đối thủ muốn đấu lại và mình chưa bấm -> hiện panel chấp nhận / từ chối
        if (e.requestingPlayer != GameManager.Instance.GetLocalPlayerType())
        {
            if (rematchButton != null && rematchButton.interactable)
            {
                ShowRequestPanel();
            }
        }
    }

    private void GameManager_OnRematchDeclined(object sender, GameManager.OnRematchRequestedEventArgs e)
    {
        HideRequestPanel();
        Hide();

        // Người từ chối sẽ bị đưa ra khỏi phòng; người còn lại thấy hộp thoại từ GameManager.OnMatchInterrupted
        if (GameManager.Instance != null && e.requestingPlayer == GameManager.Instance.GetLocalPlayerType())
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            SceneManager.LoadScene(MultiplayerConstants.SCENE_MAIN_MENU);
        }
    }

    private void Show()
    {
        ResetButtonState();
        if (rematchButton != null)
        {
            rematchButton.gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        if (rematchButton != null)
        {
            rematchButton.gameObject.SetActive(false);
        }
    }

    private void ResetButtonState()
    {
        if (rematchButton != null)
        {
            rematchButton.interactable = true;
        }
        SetButtonBackgroundVisible(true);

        if (rematchText != null)
        {
            rematchText.text = MultiplayerConstants.REMATCH_DEFAULT;
        }
    }

    private void SetButtonBackgroundVisible(bool visible)
    {
        if (rematchButton != null && rematchButton.targetGraphic != null)
        {
            rematchButton.targetGraphic.enabled = visible;
        }
    }

    // ------------------------------------------------------------
    // Panel "đối thủ muốn đấu lại"
    // ------------------------------------------------------------
    private void ShowRequestPanel()
    {
        if (requestPanel == null) BuildRequestPanel();
        if (requestPanel == null) return;

        // Có panel chấp nhận / từ chối rồi thì ẩn nút Rematch
        Hide();

        string opponent = string.IsNullOrEmpty(PlayerProfile.OpponentName) ? "Your opponent" : PlayerProfile.OpponentName;
        requestPanelText.text = $"{opponent} wants a rematch!";
        requestPanel.transform.SetAsLastSibling();
        requestPanel.SetActive(true);
    }

    private void HideRequestPanel()
    {
        if (requestPanel != null) requestPanel.SetActive(false);
    }

    private void OnAcceptClicked()
    {
        HideRequestPanel();
        if (GameManager.Instance != null) GameManager.Instance.Rematch();
    }

    private void OnDeclineClicked()
    {
        HideRequestPanel();
        if (GameManager.Instance != null) GameManager.Instance.DeclineRematch();
    }

    private void BuildRequestPanel()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        requestPanel = CreateRequestPanel(canvas.rootCanvas.transform, out requestPanelText, out acceptButton, out declineButton);
        WireRequestPanel();
    }

    private void WireRequestPanel()
    {
        if (acceptButton != null) acceptButton.onClick.AddListener(OnAcceptClicked);
        if (declineButton != null) declineButton.onClick.AddListener(OnDeclineClicked);
    }

    // Tạo các object của panel (dùng lúc chạy game nếu chưa gán trong Inspector, và cho menu Editor "TicTacToe/Build Rematch Panel")
    public static GameObject CreateRequestPanel(Transform parent, out TextMeshProUGUI messageText, out Button accept, out Button decline)
    {
        GameObject panel = new GameObject("RematchRequestPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rt = (RectTransform)panel.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(600, 280);
        rt.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.05f, 0.03f, 0.22f, 0.96f);

        // Vẽ đè lên các quân X/O (là sprite trong thế giới game, không phải UI)
        Canvas panelCanvas = panel.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 1000;
        panel.AddComponent<GraphicRaycaster>();

        GameObject textGO = new GameObject("Message", typeof(RectTransform));
        textGO.transform.SetParent(panel.transform, false);
        RectTransform textRt = (RectTransform)textGO.transform;
        textRt.anchorMin = textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.anchoredPosition = new Vector2(0, 60);
        textRt.sizeDelta = new Vector2(540, 110);
        messageText = textGO.AddComponent<TextMeshProUGUI>();
        messageText.text = "Opponent wants a rematch!";
        messageText.fontSize = 38;
        messageText.enableAutoSizing = true;
        messageText.fontSizeMin = 20;
        messageText.fontSizeMax = 38;
        messageText.fontStyle = FontStyles.Bold;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = Color.white;
        messageText.raycastTarget = false;

        accept = CreatePanelButton(panel.transform, "ACCEPT", new Color(0.2f, 0.65f, 0.45f), new Vector2(-135, -70));
        decline = CreatePanelButton(panel.transform, "DECLINE", new Color(0.8f, 0.3f, 0.3f), new Vector2(135, -70));

        panel.SetActive(false);
        return panel;
    }

    private static Button CreatePanelButton(Transform parent, string label, Color bg, Vector2 pos)
    {
        GameObject go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        go.name = label + "Button";
        go.transform.SetParent(parent, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(210, 66);

        go.GetComponent<Image>().color = bg;

        TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 30;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return go.GetComponent<Button>();
    }
}
