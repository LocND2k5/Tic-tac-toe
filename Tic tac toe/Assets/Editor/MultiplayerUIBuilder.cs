using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TicTacToe.Multiplayer;

// Tạo sẵn các object UI Multiplayer trong scene để bạn tự chỉnh sửa trong Canvas,
// đồng thời gán luôn các tham chiếu vào component NetworkUI.
public static class MultiplayerUIBuilder
{
    private const float BuildScale = 0.7f;

    [MenuItem("TicTacToe/Build Multiplayer UI")]
    public static void Build()
    {
        NetworkUI[] found = Object.FindObjectsByType<NetworkUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (found.Length == 0)
        {
            EditorUtility.DisplayDialog("Build Multiplayer UI",
                "Không tìm thấy component NetworkUI trong scene đang mở.\nHãy mở MainMenuScene rồi chạy lại.", "OK");
            return;
        }

        NetworkUI networkUI = found[0];
        SerializedObject so = new SerializedObject(networkUI);

        // Đã có UI gán sẵn -> hỏi trước khi dựng lại
        GameObject existingMenu = so.FindProperty("_menuView").objectReferenceValue as GameObject;
        if (existingMenu != null)
        {
            if (!EditorUtility.DisplayDialog("Build Multiplayer UI",
                    "NetworkUI đã được gán UI. Dựng lại sẽ XÓA MenuView/HostView/JoinView hiện tại (kể cả phần bạn đã chỉnh). Tiếp tục?",
                    "Dựng lại", "Hủy"))
            {
                return;
            }

            foreach (string prop in new[] { "_menuView", "_hostView", "_joinView" })
            {
                GameObject view = so.FindProperty(prop).objectReferenceValue as GameObject;
                if (view != null) Undo.DestroyObjectImmediate(view);
            }
        }

        // Ẩn UI cũ (Host/Join/Grid selection) trong panel nhưng không xóa
        foreach (Transform child in networkUI.transform)
        {
            if (child.gameObject.activeSelf)
            {
                Undo.RecordObject(child.gameObject, "Hide old multiplayer UI");
                child.gameObject.SetActive(false);
            }
        }

        MultiplayerMenuBuilder.Refs r = MultiplayerMenuBuilder.Build(networkUI.transform, BuildScale);
        Undo.RegisterCreatedObjectUndo(r.menuView, "Build Multiplayer UI");
        Undo.RegisterCreatedObjectUndo(r.hostView, "Build Multiplayer UI");
        Undo.RegisterCreatedObjectUndo(r.joinView, "Build Multiplayer UI");

        so.Update();
        Set(so, "_menuView", r.menuView);
        Set(so, "_hostView", r.hostView);
        Set(so, "_joinView", r.joinView);
        Set(so, "_nameInput", r.nameInput);
        Set(so, "_createRoomButton", r.createRoomButton);
        Set(so, "_joinGameButton", r.joinGameButton);
        Set(so, "_backToMenuButton", r.backToMenuButton);
        Set(so, "_roomCodeText", r.roomCodeText);
        Set(so, "_hostStatusText", r.hostStatusText);
        Set(so, "_copyCodeButton", r.copyCodeButton);
        Set(so, "_selectedGridText", r.selectedGridText);
        Set(so, "_startGameButton", r.startGameButton);
        Set(so, "_closeRoomButton", r.closeRoomButton);
        Set(so, "_codeInput", r.codeInput);
        Set(so, "_joinConfirmButton", r.joinConfirmButton);
        Set(so, "_joinStatusText", r.joinStatusText);
        Set(so, "_joinBackButton", r.joinBackButton);

        SerializedProperty grid = so.FindProperty("_gridButtons");
        grid.arraySize = r.gridButtons.Length;
        for (int i = 0; i < r.gridButtons.Length; i++)
        {
            grid.GetArrayElementAtIndex(i).objectReferenceValue = r.gridButtons[i];
        }
        so.ApplyModifiedProperties();

        // Chỉ hiện MenuView để dễ chỉnh; bấm vào từng view trong Hierarchy và bật lên để chỉnh Host/Join
        r.hostView.SetActive(false);
        r.joinView.SetActive(false);

        EditorSceneManager.MarkSceneDirty(networkUI.gameObject.scene);
        Selection.activeGameObject = r.menuView;
        Debug.Log("Đã tạo UI Multiplayer. Bật panel Multiplayer và từng view (Menu/Host/Join) trong Hierarchy để chỉnh sửa. Nhớ Ctrl+S để lưu scene.");
    }

    private static void Set(SerializedObject so, string propertyName, Object value)
    {
        so.FindProperty(propertyName).objectReferenceValue = value;
    }
}
