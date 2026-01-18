using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class VideoIntroManager : MonoBehaviour
{
    // === Tham chiếu cần gán trong Inspector ===
    [Header("Video & Scene Setup")]
    // Kéo VideoPlayer Component vào đây
    public VideoPlayer videoPlayer;
    // Index của scene Menu (Giả định là scene index 1)
    public int nextSceneIndex = 1;

    [Header("UI Effects")]
    // Panel/Image đen phủ toàn màn hình (Dùng để Fade to Black)
    public Image fadeImage;
    // Tiêu đề game/Logo (TextMeshProUGUI hoặc Image)
    public MaskableGraphic titleGraphic;

    // === Cài đặt thời gian ===
    [Header("Timing Settings")]
    [Tooltip("Số giây trước khi video kết thúc để bắt đầu hiện tiêu đề")]
    public float titleShowTimeBeforeEnd = 5.0f;
    [Tooltip("Số giây trước khi video kết thúc để bắt đầu Fade Out màn hình")]
    public float fadeOutTimeBeforeEnd = 2.0f;
    [Tooltip("Thời gian hiệu ứng Fade Out (tối dần)")]
    public float fadeDuration = 1.5f;

    // === Trạng thái nội bộ ===
    private double videoLength;
    private bool titleShown = false;
    private bool screenFaded = false;

    // Tham chiếu đến Coroutine đang chạy để tránh chạy nhiều lần
    private IEnumerator titleCoroutine;
    private IEnumerator fadeCoroutine;


    void Start()
    {
        // 1. Kiểm tra video
        if (videoPlayer == null || videoPlayer.clip == null)
        {
            Debug.LogError("VideoPlayer hoặc VideoClip chưa được gán! Tự động chuyển đến Menu.");
            SceneManager.LoadScene(nextSceneIndex);
            return;
        }

        // 2. Thiết lập trạng thái ban đầu
        videoLength = videoPlayer.clip.length;

        // Đặt màu ban đầu của Title Graphic là trong suốt
        Color titleColor = titleGraphic.color;
        titleGraphic.color = new Color(titleColor.r, titleColor.g, titleColor.b, 0f);

        // Đặt màu ban đầu của Fade Image là trong suốt và tắt tương tác
        fadeImage.color = new Color(0, 0, 0, 0f);
        fadeImage.raycastTarget = false;

        // 3. Đăng ký sự kiện kết thúc video
        videoPlayer.loopPointReached += OnVideoEnd;

        // 4. Bắt đầu chơi video
        videoPlayer.Play();
    }

    void Update()
    {
        // Kiểm tra thời điểm để hiện tiêu đề và Fade Out
        double timeRemaining = videoLength - videoPlayer.time;

        // Bắt đầu hiện Tiêu đề (5 giây trước khi kết thúc)
        if (!titleShown && timeRemaining <= titleShowTimeBeforeEnd)
        {
            titleShown = true;
            titleCoroutine = FadeGraphic(titleGraphic, 1.0f, titleShowTimeBeforeEnd - fadeOutTimeBeforeEnd);
            StartCoroutine(titleCoroutine);
        }

        // Bắt đầu Fade Out màn hình (2 giây trước khi kết thúc)
        if (!screenFaded && timeRemaining <= fadeOutTimeBeforeEnd)
        {
            screenFaded = true;
            fadeCoroutine = FadeImage(fadeImage, 1.0f, fadeDuration);
            StartCoroutine(fadeCoroutine);
        }
    }

    // Coroutine chung để làm mờ đồ họa
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

    // Coroutine riêng cho Fade Image (đảm bảo nó là màu đen)
    IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
        // Kích hoạt tương tác để chặn input khi màn hình đang tối
        image.raycastTarget = true;

        float startAlpha = image.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            image.color = new Color(0, 0, 0, alpha); // Đảm bảo màu đen (R=0, G=0, B=0)
            yield return null;
        }

        image.color = new Color(0, 0, 0, targetAlpha);
    }

    // Xử lý khi video kết thúc
    void OnVideoEnd(VideoPlayer vp)
    {
        // Chắc chắn màn hình đã hoàn toàn tối (nếu không, đợi nó tối)
        if (fadeImage.color.a < 0.99f)
        {
            // Nếu chưa fade xong, chúng ta chuyển cảnh ngay sau 1 khung hình
            // hoặc bạn có thể gọi lại FadeCoroutine với thời gian 0.1s
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Màn hình đã tối, chuyển cảnh ngay lập tức
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
