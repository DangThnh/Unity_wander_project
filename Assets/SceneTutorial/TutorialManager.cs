using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public Sprite tutorialImage;      // Hình ảnh hướng dẫn
        [TextArea(3, 5)]
        public string tutorialText;       // Nội dung văn bản hướng dẫn
    }

    [Header("UI References")]
    public Image displayImage;            // Image component để hiển thị hình hướng dẫn
    public TextMeshProUGUI displayText;    // TextMeshPro để hiển thị nội dung hướng dẫn
    public TextMeshProUGUI continueText;   // Chữ "Nhấn E để tiếp tục"
    public Image fadeOverlay;             // Tấm nền đen để làm hiệu ứng chuyển cảnh

    [Header("Tutorial Content")]
    public List<TutorialStep> tutorialSteps; // Danh sách các bước hướng dẫn (tùy chỉnh số lượng trong Inspector)

    [Header("Settings")]
    public float blinkSpeed = 0.5f;       // Tốc độ nhấp nháy của chữ Continue
    public float fadeDuration = 0.5f;     // Thời gian mờ dần khi chuyển bước
    public int mainMenuSceneIndex = 1;    // Index của Scene Menu chính

    private int currentStepIndex = 0;
    private bool isTransitioning = false;
    private bool isBlinking = true;

    void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 0;
            fadeOverlay.color = c;
        }

        // Bắt đầu nhấp nháy chữ hướng dẫn
        StartCoroutine(BlinkTextCoroutine());

        // Hiển thị bước đầu tiên
        UpdateTutorialUI();
    }

    void Update()
    {
        // Kiểm tra phím E để chuyển bước
        if (Input.GetKeyDown(KeyCode.E) && !isTransitioning)
        {
            if (currentStepIndex < tutorialSteps.Count - 1)
            {
                StartCoroutine(NextStep());
            }
            else
            {
                // Nếu đã là bước cuối cùng, chuyển về Menu
                StartCoroutine(FinishTutorial());
            }
        }
    }

    private void UpdateTutorialUI()
    {
        if (tutorialSteps.Count > 0 && currentStepIndex < tutorialSteps.Count)
        {
            if (displayImage != null) displayImage.sprite = tutorialSteps[currentStepIndex].tutorialImage;
            if (displayText != null) displayText.text = tutorialSteps[currentStepIndex].tutorialText;
        }
    }

    IEnumerator NextStep()
    {
        isTransitioning = true;

        // 1. Fade out màn hình một chút hoặc làm mờ ảnh cũ
        yield return StartCoroutine(Fade(1f));

        // 2. Thay đổi nội dung
        currentStepIndex++;
        UpdateTutorialUI();

        // 3. Fade in trở lại
        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    IEnumerator FinishTutorial()
    {
        isTransitioning = true;
        isBlinking = false;
        if (continueText != null) continueText.enabled = true;

        // Fade sang đen hoàn toàn trước khi thoát
        yield return StartCoroutine(Fade(1f));

        // Dọn dẹp và Load Menu (Tương tự code Credit của bạn)
        CleanUpBeforeExit();

        Debug.Log("Hoàn thành hướng dẫn, đang quay lại Menu...");
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    IEnumerator Fade(float targetAlpha)
    {
        if (fadeOverlay == null) yield break;

        float startAlpha = fadeOverlay.color.a;
        float timer = 0;
        Color tempColor = fadeOverlay.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            tempColor.a = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeOverlay.color = tempColor;
            yield return null;
        }
    }

    IEnumerator BlinkTextCoroutine()
    {
        while (isBlinking)
        {
            if (continueText != null) continueText.enabled = !continueText.enabled;
            yield return new WaitForSeconds(blinkSpeed);
        }
        if (continueText != null) continueText.enabled = true;
    }

    private void CleanUpBeforeExit()
    {
        // Đảm bảo chuột hiển thị để dùng Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        // Bạn có thể thêm các hàm hủy Manager ở đây nếu cần như code cũ
    }

    private void OnDisable()
    {
        isBlinking = false;
        StopAllCoroutines();
    }
}