using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TicTacToe.Multiplayer
{
    // Dựng sẵn các object UI của màn hình Multiplayer (Menu / Host / Join).
    // Dùng chung cho: tự dựng lúc chạy game (khi chưa gán trong Inspector) và menu Editor "Build Multiplayer UI".
    public static class MultiplayerMenuBuilder
    {
        public class Refs
        {
            public GameObject menuView, hostView, joinView;
            public TMP_InputField nameInput, codeInput;
            public Button createRoomButton, joinGameButton, backToMenuButton;
            public Button copyCodeButton, startGameButton, closeRoomButton;
            public Button joinConfirmButton, joinBackButton;
            public Button[] gridButtons = new Button[3];
            public TextMeshProUGUI roomCodeText, hostStatusText, selectedGridText, joinStatusText;
        }

        private static readonly Color TitleColor = new Color(1f, 0.82f, 0.2f);
        private static readonly Color BlueColor = new Color(0.16f, 0.55f, 0.88f);
        private static readonly Color GreenColor = new Color(0.2f, 0.65f, 0.45f);
        private static readonly Color GrayColor = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color RedColor = new Color(0.8f, 0.3f, 0.3f);
        private static readonly Color GridColor = new Color(0.25f, 0.30f, 0.40f, 0.9f);

        private static readonly int[] GridSizes = { 3, 5, 7 };

        // scale: nhân kích thước / vị trí / cỡ chữ (canvas dùng Constant Pixel Size nên cần thu nhỏ)
        public static Refs Build(Transform parent, float scale)
        {
            Refs r = new Refs();

            // ---------------- Menu ----------------
            r.menuView = CreateView(parent, "MenuView");
            Transform m = r.menuView.transform;
            CreateLabel(m, "Title", "ONLINE MULTIPLAYER", 48, TitleColor, new Vector2(0, 290), new Vector2(800, 80), scale);
            CreateLabel(m, "NameLabel", "YOUR NAME:", 26, Color.white, new Vector2(-215, 175), new Vector2(190, 50), scale);
            r.nameInput = CreateInputField(m, "NameInput", new Vector2(70, 175), new Vector2(320, 56), "Enter name", 30, PlayerProfile.MaxNameLength, false, scale);
            r.createRoomButton = CreateButton(m, "CreateRoomButton", "CREATE ROOM", BlueColor, Color.white, new Vector2(0, 50), new Vector2(360, 80), scale);
            r.joinGameButton = CreateButton(m, "JoinGameButton", "JOIN GAME", GreenColor, Color.white, new Vector2(0, -70), new Vector2(360, 80), scale);
            r.backToMenuButton = CreateButton(m, "BackToMenuButton", "BACK TO MENU", GrayColor, Color.black, new Vector2(0, -220), new Vector2(300, 70), scale);

            // ---------------- Host ----------------
            r.hostView = CreateView(parent, "HostView");
            Transform h = r.hostView.transform;
            CreateLabel(h, "Title", "YOUR ROOM CODE", 34, TitleColor, new Vector2(0, 310), new Vector2(800, 60), scale);
            r.roomCodeText = CreateLabel(h, "RoomCodeText", "------", 72, Color.white, new Vector2(0, 225), new Vector2(700, 100), scale);
            r.hostStatusText = CreateLabel(h, "HostStatusText", "", 26, Color.white, new Vector2(0, 140), new Vector2(900, 50), scale);
            r.copyCodeButton = CreateButton(h, "CopyCodeButton", "COPY CODE", BlueColor, Color.white, new Vector2(0, 60), new Vector2(260, 56), scale);
            r.selectedGridText = CreateLabel(h, "SelectedGridText", "Board Size: 3x3", 26, Color.white, new Vector2(0, -30), new Vector2(600, 44), scale);
            for (int i = 0; i < GridSizes.Length; i++)
            {
                int size = GridSizes[i];
                r.gridButtons[i] = CreateButton(h, "Size" + size + "x" + size + "Button", size + "x" + size, GridColor, Color.white,
                    new Vector2((i - 1) * 170, -105), new Vector2(130, 56), scale);
            }
            r.startGameButton = CreateButton(h, "StartGameButton", "START MATCH", GreenColor, Color.white, new Vector2(0, -210), new Vector2(320, 64), scale);
            r.closeRoomButton = CreateButton(h, "CloseRoomButton", "CLOSE ROOM", RedColor, Color.white, new Vector2(0, -310), new Vector2(240, 50), scale);

            // ---------------- Join ----------------
            r.joinView = CreateView(parent, "JoinView");
            Transform j = r.joinView.transform;
            CreateLabel(j, "Title", "JOIN GAME", 48, TitleColor, new Vector2(0, 260), new Vector2(800, 80), scale);
            CreateLabel(j, "CodeLabel", "Enter the room code:", 28, Color.white, new Vector2(0, 150), new Vector2(700, 50), scale);
            r.codeInput = CreateInputField(j, "RoomCodeInput", new Vector2(0, 70), new Vector2(360, 70), "CODE", 40, 12, true, scale);
            r.joinConfirmButton = CreateButton(j, "JoinButton", "JOIN", GreenColor, Color.white, new Vector2(0, -40), new Vector2(260, 70), scale);
            r.joinStatusText = CreateLabel(j, "JoinStatusText", "", 26, Color.white, new Vector2(0, -135), new Vector2(900, 50), scale);
            r.joinBackButton = CreateButton(j, "BackButton", "BACK", GrayColor, Color.black, new Vector2(0, -250), new Vector2(240, 60), scale);

            return r;
        }

        private static GameObject CreateView(Transform parent, string viewName)
        {
            GameObject go = new GameObject(viewName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string objName, string text, float fontSize, Color color, Vector2 pos, Vector2 size, float scale)
        {
            GameObject go = new GameObject(objName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos * scale;
            rt.sizeDelta = size * scale;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize * scale;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string objName, string label, Color bg, Color textColor, Vector2 pos, Vector2 size, float scale)
        {
            GameObject go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
            go.name = objName;
            go.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos * scale;
            rt.sizeDelta = size * scale;

            go.GetComponent<Image>().color = bg;

            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28 * scale;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;

            return go.GetComponent<Button>();
        }

        private static TMP_InputField CreateInputField(Transform parent, string objName, Vector2 pos, Vector2 size,
            string placeholderText, float fontSize, int charLimit, bool codeStyle, float scale)
        {
            GameObject go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            go.name = objName;
            go.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos * scale;
            rt.sizeDelta = size * scale;

            TMP_InputField input = go.GetComponent<TMP_InputField>();
            input.characterLimit = charLimit;
            if (codeStyle) input.contentType = TMP_InputField.ContentType.Alphanumeric;

            if (input.textComponent != null)
            {
                input.textComponent.fontSize = fontSize * scale;
                input.textComponent.alignment = TextAlignmentOptions.Center;
                input.textComponent.color = Color.black;
            }
            if (input.placeholder is TextMeshProUGUI placeholder)
            {
                placeholder.text = placeholderText;
                placeholder.fontSize = fontSize * 0.9f * scale;
                placeholder.alignment = TextAlignmentOptions.Center;
            }
            return input;
        }
    }
}
