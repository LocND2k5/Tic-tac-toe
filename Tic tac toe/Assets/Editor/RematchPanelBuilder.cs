using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tạo sẵn panel "đối thủ muốn đấu lại" (ACCEPT / DECLINE) trong scene để tự chỉnh trong Canvas
public static class RematchPanelBuilder
{
    [MenuItem("TicTacToe/Build Rematch Panel")]
    public static void Build()
    {
        RematchUI[] found = Object.FindObjectsByType<RematchUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (found.Length == 0)
        {
            EditorUtility.DisplayDialog("Build Rematch Panel",
                "Không tìm thấy RematchUI trong scene đang mở.\nHãy mở SampleScene rồi chạy lại.", "OK");
            return;
        }

        RematchUI rematchUI = found[0];
        Canvas canvas = rematchUI.GetComponentInParent<Canvas>();
        if (canvas == null) canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Build Rematch Panel", "Không tìm thấy Canvas trong scene.", "OK");
            return;
        }

        SerializedObject so = new SerializedObject(rematchUI);
        GameObject existing = so.FindProperty("requestPanel").objectReferenceValue as GameObject;
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Build Rematch Panel",
                    "Panel đã tồn tại. Dựng lại sẽ XÓA panel hiện tại (kể cả phần bạn đã chỉnh). Tiếp tục?",
                    "Dựng lại", "Hủy"))
            {
                return;
            }
            Undo.DestroyObjectImmediate(existing);
        }

        GameObject panel = RematchUI.CreateRequestPanel(canvas.rootCanvas.transform,
            out TextMeshProUGUI messageText, out Button accept, out Button decline);
        Undo.RegisterCreatedObjectUndo(panel, "Build Rematch Panel");

        so.Update();
        so.FindProperty("requestPanel").objectReferenceValue = panel;
        so.FindProperty("requestPanelText").objectReferenceValue = messageText;
        so.FindProperty("acceptButton").objectReferenceValue = accept;
        so.FindProperty("declineButton").objectReferenceValue = decline;
        so.ApplyModifiedProperties();

        // Bật lên để dễ chỉnh trong Scene view; lúc chạy game code sẽ tự ẩn đi
        panel.SetActive(true);

        EditorSceneManager.MarkSceneDirty(rematchUI.gameObject.scene);
        Selection.activeGameObject = panel;
        Debug.Log("Đã tạo RematchRequestPanel. Chỉnh sửa tự do rồi Ctrl+S để lưu scene. Panel sẽ tự ẩn khi chạy game.");
    }
}
