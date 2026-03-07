using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Cần thiết để sử dụng component Image
using TMPro;
using System.Collections;

public class CreditSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Dòng chữ 'Press E to continue'.")]
    public TextMeshProUGUI continueText;

    [Tooltip("Kéo Image màu đen che toàn màn hình vào đây (Fade Overlay).")]
    public Image fadeOverlayImage;

    [Header("Settings")]
    [Tooltip("Scene Index của Main Menu.")]
    public int mainMenuSceneIndex = 1;

    [Tooltip("Tốc độ nhấp nháy chữ (giây).")]
    public float blinkSpeed = 0.5f;

    [Tooltip("Thời gian để màn hình tối hoàn toàn (giây).")]
    public float fadeDuration = 1.0f;

    [Tooltip("Khoảng dừng sau khi màn hình đã tối đen trước khi chuyển Scene.")]
    public float delayBeforeLoad = 0.5f;

    private bool isBlinking = false;
    private bool isTransitioning = false;

    void Start()
    {
        // Khởi tạo Image ở trạng thái trong suốt hoàn toàn
        if (fadeOverlayImage != null)
        {
            Color c = fadeOverlayImage.color;
            c.a = 0;
            fadeOverlayImage.color = c;
            fadeOverlayImage.gameObject.SetActive(true); // Đảm bảo object đang bật
            fadeOverlayImage.raycastTarget = false; // Không chặn tương tác lúc đầu
        }

        if (continueText != null)
        {
            StartBlinking();
        }
        else
        {
            Debug.LogError("Chưa gán Continue Text trong Inspector!");
        }
    }

    void Update()
    {
        // Kiểm tra phím E và đảm bảo không đang thực hiện chuyển cảnh
        if (Input.GetKeyDown(KeyCode.E) && !isTransitioning)
        {
            StartCoroutine(TransitionToMainMenu());
        }
    }

    private void StartBlinking()
    {
        if (!isBlinking)
        {
            isBlinking = true;
            StartCoroutine(BlinkTextCoroutine());
        }
    }

    IEnumerator BlinkTextCoroutine()
    {
        while (isBlinking)
        {
            continueText.enabled = !continueText.enabled;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    /// <summary>
    /// Thực hiện quá trình tối dần màn hình và chuyển Scene.
    /// </summary>
    IEnumerator TransitionToMainMenu()
    {
        isTransitioning = true;
        isBlinking = false;

        // Đảm bảo chữ hiện lên cố định trước khi mờ đi
        if (continueText != null) continueText.enabled = true;

        if (fadeOverlayImage != null)
        {
            fadeOverlayImage.raycastTarget = true; // Chặn bấm phím/chuột trong khi đang load

            float timer = 0;
            Color tempColor = fadeOverlayImage.color;

            // Vòng lặp làm tăng Alpha của Image từ 0 lên 1
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                tempColor.a = Mathf.Clamp01(timer / fadeDuration);
                fadeOverlayImage.color = tempColor;
                yield return null;
            }
        }

        // Nhịp dừng ngắn để tạo cảm giác mượt mà (giống Main Menu logic)
        yield return new WaitForSeconds(delayBeforeLoad);

        // Chuyển Scene
        Debug.Log("Đang chuyển sang Scene: " + mainMenuSceneIndex);
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void OnDisable()
    {
        isBlinking = false;
        StopAllCoroutines();
    }
}