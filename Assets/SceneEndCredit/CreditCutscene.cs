using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class CreditCutscene : MonoBehaviour
{
    // === Tham chiếu cần gán trong Inspector ===
    [Header("Video & Scene Setup")]
    public VideoPlayer videoPlayer;
    public int nextSceneIndex = 1;

    [Header("UI Effects")]
    public Image fadeImage;
    public MaskableGraphic titleGraphic;

    // === Cài đặt thời gian ===
    [Header("Timing Settings")]
    public float titleShowTimeBeforeEnd = 5.0f;
    public float fadeOutTimeBeforeEnd = 2.0f;
    public float fadeDuration = 1.5f;

    // === Trạng thái nội bộ ===
    private double videoLength;
    private bool titleShown = false;
    private bool screenFaded = false;
    private bool isCleaningUp = false; // Tránh gọi dọn dẹp nhiều lần

    private IEnumerator titleCoroutine;
    private IEnumerator fadeCoroutine;

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer chưa được gán! Tự động chuyển đến Menu.");
            PerformFinalTransition();
            return;
        }

        // Thiết lập ban đầu
        videoLength = videoPlayer.clip != null ? videoPlayer.clip.length : 0;

        Color titleColor = titleGraphic.color;
        titleGraphic.color = new Color(titleColor.r, titleColor.g, titleColor.b, 0f);

        fadeImage.color = new Color(0, 0, 0, 0f);
        fadeImage.raycastTarget = false;

        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }

    void Update()
    {
        if (videoPlayer.clip == null) return;

        double timeRemaining = videoLength - videoPlayer.time;

        // Hiện tiêu đề
        if (!titleShown && timeRemaining <= titleShowTimeBeforeEnd)
        {
            titleShown = true;
            titleCoroutine = FadeGraphic(titleGraphic, 1.0f, titleShowTimeBeforeEnd - fadeOutTimeBeforeEnd);
            StartCoroutine(titleCoroutine);
        }

        // Fade màn hình
        if (!screenFaded && timeRemaining <= fadeOutTimeBeforeEnd)
        {
            screenFaded = true;
            fadeCoroutine = FadeImage(fadeImage, 1.0f, fadeDuration);
            StartCoroutine(fadeCoroutine);
        }
    }

    IEnumerator FadeGraphic(MaskableGraphic graphic, float targetAlpha, float duration)
    {
        float startAlpha = graphic.color.a;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            yield return null;
        }
        graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, targetAlpha);
    }

    IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
        image.raycastTarget = true;
        float startAlpha = image.color.a;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        image.color = new Color(0, 0, 0, targetAlpha);
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        PerformFinalTransition();
    }

    // Hàm tổng hợp để dọn dẹp và chuyển cảnh
    private void PerformFinalTransition()
    {
        if (isCleaningUp) return;
        isCleaningUp = true;

        Debug.Log("Video kết thúc. Bắt đầu dọn dẹp dữ liệu và chuyển cảnh...");

        // 1. Thực hiện dọn dẹp dữ liệu game
        CleanUpGameState();

        // 2. Chuyển sang Menu chính
        SceneManager.LoadScene(nextSceneIndex);
    }

    private void CleanUpGameState()
    {
        // A. Reset Inventory (Gọi hàm Reset của InventoryManager)
        // Lưu ý: Chúng ta giả định InventoryManager.Instance tồn tại hoặc tìm qua Find
        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null)
        {
            inv.ResetInventory();
            Debug.Log("Inventory đã được reset.");
        }

        // B. Hủy các Managers cứng đầu (DontDestroyOnLoad)
        string[] managersToDestroy = { "GameManager", "InventoryManager", "GlobalAudio", "PlayerStatus", "QuestManager" };
        foreach (string managerName in managersToDestroy)
        {
            GameObject obj = GameObject.Find(managerName);
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        // C. Reset các thông số hệ thống
        Time.timeScale = 1f;

        // D. Cấu hình lại Cursor cho Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // E. Giải phóng bộ nhớ
        System.GC.Collect();
    }
}