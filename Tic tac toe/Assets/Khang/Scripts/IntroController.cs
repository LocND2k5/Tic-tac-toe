using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Khang
{
    /// <summary>
    /// Điều khiển màn hình Loading / Intro với thanh Slider tiến trình.
    /// Tải bất đồng bộ (Async) sang MainMenuScene và cập nhật thanh Slider mượt mà từ 0% đến 100%.
    /// </summary>
    public class IntroController : MonoBehaviour
    {
        [Header("Cấu hình chuyển Scene")]
        [SerializeField] private string nextSceneName = "MainMenuScene";
        [SerializeField] private float minimumLoadingTime = 2.0f; // Thời gian chạy thanh loading (giây)

        [Header("UI Loading")]
        [SerializeField] private Slider loadingSlider;              // Thanh Slider tiến trình
        [SerializeField] private TextMeshProUGUI progressText;       // Chữ hiển thị phần trăm (Tùy chọn)

        private void Start()
        {
            if (loadingSlider != null)
            {
                loadingSlider.minValue = 0f;
                loadingSlider.maxValue = 1f;
                loadingSlider.value = 0f;
            }

            StartCoroutine(LoadSceneAsyncRoutine());
        }

        private IEnumerator LoadSceneAsyncRoutine()
        {
            // Bắt đầu nạp scene nền
            AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
            operation.allowSceneActivation = false; // Tạm dừng chuyển scene cho đến khi thanh chạy đủ 100%

            float timer = 0f;

            while (timer < minimumLoadingTime)
            {
                timer += Time.deltaTime;

                // Tiến độ thực tế từ Unity (tối đa 0.9 trước khi activate) kết hợp với thời gian tối thiểu
                float timeProgress = Mathf.Clamp01(timer / minimumLoadingTime);
                float unityProgress = Mathf.Clamp01(operation.progress / 0.9f);
                float currentProgress = Mathf.Min(timeProgress, unityProgress);

                // Cập nhật Slider
                if (loadingSlider != null)
                {
                    loadingSlider.value = currentProgress;
                }

                // Cập nhật chữ phần trăm
                if (progressText != null)
                {
                    progressText.text = $"Loading... {Mathf.RoundToInt(currentProgress * 100f)}%";
                }

                yield return null;
            }

            // Đảm bảo đạt đủ 100%
            if (loadingSlider != null) loadingSlider.value = 1f;
            if (progressText != null) progressText.text = "Loading... 100%";

            yield return new WaitForSeconds(0.2f);

            // Cho phép chuyển sang MainMenuScene
            operation.allowSceneActivation = true;
        }
    }
}
