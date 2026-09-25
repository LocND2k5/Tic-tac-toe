using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // Nếu bạn làm mỗi board là 1 Scene riêng (Khuyên dùng với project hiện tại của bạn)
    public void LoadBoard3x3()
    {
        // Nhớ đổi "Scene_3x3" thành tên file Scene chính xác của bạn
        SceneManager.LoadScene("Scene_3x3");
    }

    public void LoadBoard5x5()
    {
        SceneManager.LoadScene("Scene_5x5");
    }

    public void LoadBoard7x7()
    {
        SceneManager.LoadScene("Scene_7x7");
    }

    // ==========================================
    // Nếu bạn làm 1 Scene duy nhất bằng Code động (Nâng cao)
    // ==========================================
    public static int SelectedBoardSize = 3; // Biến tĩnh lưu giữ cấu hình

    public void LoadBoardDynamic(int size)
    {
        SelectedBoardSize = size;
        SceneManager.LoadScene("GameScene"); // Nạp 1 Scene duy nhất
    }
}
