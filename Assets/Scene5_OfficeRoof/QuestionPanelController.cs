using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestionPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup panelCanvasGroup; // Gán vào Root Question Panel
    [SerializeField] private Image backgroundOverlay;     // Image màu đen (nền)
    [SerializeField] private RectTransform contentArea;    // Chứa Text và Buttons

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float maxBackgroundAlpha = 0.8f; // Độ tối tối đa (0-1)

    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Khởi tạo trạng thái ẩn
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0;
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// Kích hoạt bảng câu hỏi với hiệu ứng làm tối nền
    /// </summary>
    public void ShowQuestion()
    {
        gameObject.SetActive(true);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeSequence(true));
    }

    /// <summary>
    /// Ẩn bảng câu hỏi
    /// </summary>
    public void HideQuestion()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeSequence(false));
    }

    private IEnumerator FadeSequence(bool fadeIn)
    {
        float counter = 0;
        float startAlpha = fadeIn ? 0 : 1;
        float endAlpha = fadeIn ? 1 : 0;

        // Bật chặn tương tác ngay lập tức khi hiện
        if (fadeIn)
        {
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;
        }

        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            float progress = counter / fadeDuration;

            // 1. Làm tối nền (Background Overlay)
            // Chúng ta chỉ thay đổi alpha của Image nền, không phải của toàn bộ Group
            if (backgroundOverlay != null)
            {
                Color bgColor = backgroundOverlay.color;
                float targetBgAlpha = fadeIn ? maxBackgroundAlpha : 0;
                float currentBgAlpha = Mathf.Lerp(fadeIn ? 0 : maxBackgroundAlpha, targetBgAlpha, progress);
                backgroundOverlay.color = new Color(bgColor.r, bgColor.g, bgColor.b, currentBgAlpha);
            }

            // 2. Hiện nội dung chữ (Content Area)
            // Bạn có thể chọn hiện chữ cùng lúc hoặc hiện sau khi nền đã tối một chút
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            }

            yield return null;
        }

        if (!fadeIn)
        {
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
            gameObject.SetActive(false);
        }
    }
}