using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // ==========================================
    // Chế độ 1 Scene duy nhất (Dynamic Board)
    // ==========================================
    public static int SelectedBoardSize = 3; // Biến tĩnh lưu giữ cấu hình

    public void LoadBoardDynamic(int size)
    {
        SelectedBoardSize = size;
        SceneManager.LoadScene("SampleScene"); // Nạp Scene Game
    }
}
