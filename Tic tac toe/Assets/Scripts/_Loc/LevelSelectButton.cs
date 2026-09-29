using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelSelectButton : MonoBehaviour
{
    public int boardSize;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            MainMenuUI menuUI = FindObjectOfType<MainMenuUI>();
            if (menuUI != null)
            {
                menuUI.LoadBoardDynamic(boardSize);
            }
            else
            {
                // Fallback in case MainMenuUI is missing
                MainMenuUI.SelectedBoardSize = boardSize;
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            }
        });
    }
}
