using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Khang
{
    /// <summary>
    /// Điều khiển màn hình giới thiệu (Intro / Splash Screen).
    /// Tự động hiển thị Logo mờ dần vào (Fade in), giữ trong vài giây rồi mờ dần ra (Fade out)
    /// và chuyển sang MainMenuScene.
    /// Cho phép người chơi click chuột hoặc bấm phím bất kỳ để bỏ qua (Skip).
    /// </summary>
    public class IntroController : MonoBehaviour
    {
        [Header("Cấu hình chuyển Scene")]
        [SerializeField] private string nextSceneName = "MainMenuScene";
        [SerializeField] private float displayDuration = 2.0f; // Thời gian giữ logo (giây)

        [Header("Hiệu ứng mờ dần (Tùy chọn)")]
        [SerializeField] private CanvasGroup logoCanvasGroup;
        [SerializeField] private float fadeInDuration = 0.8f;
        [SerializeField] private float fadeOutDuration = 0.8f;

        [Header("Âm thanh Intro (Tùy chọn)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip introJingle;

        [Header("Tùy chọn Bỏ qua")]
        [SerializeField] private bool allowSkip = true;

        private bool hasTransitioned = false;

        private void Start()
        {
            if (audioSource != null && introJingle != null)
            {
                audioSource.PlayOneShot(introJingle);
            }

            StartCoroutine(IntroSequenceRoutine());
        }

        private void Update()
        {
            // Bấm chuột hoặc phím bất kỳ để skip qua Intro
            if (allowSkip && !hasTransitioned && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                StopAllCoroutines();
                GoToNextScene();
            }
        }

        private IEnumerator IntroSequenceRoutine()
        {
            // 1. Hiệu ứng Fade In (Mờ dần hiện lên)
            if (logoCanvasGroup != null)
            {
                logoCanvasGroup.alpha = 0f;
                float elapsed = 0f;
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    logoCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                    yield return null;
                }
                logoCanvasGroup.alpha = 1f;
            }

            // 2. Giữ màn hình trong một khoảng thời gian
            yield return new WaitForSeconds(displayDuration);

            // 3. Hiệu ứng Fade Out (Mờ dần biến mất)
            if (logoCanvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    logoCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeOutDuration));
                    yield return null;
                }
                logoCanvasGroup.alpha = 0f;
            }

            // 4. Chuyển sang Main Menu
            GoToNextScene();
        }

        private void GoToNextScene()
        {
            if (hasTransitioned) return;
            hasTransitioned = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
