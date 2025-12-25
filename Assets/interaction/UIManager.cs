using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // === Thiết lập Singleton ===
    public static UIManager instance;

    // Kéo Image/Panel đen phủ toàn màn hình vào đây
    [Tooltip("Panel/Image đen phủ toàn màn hình, phải ở lớp trên cùng của Canvas.")]
    public Image fadeImage;

    // Đảm bảo Image ban đầu phải có Alpha = 0

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Giữ UIManager này tồn tại giữa các Scene (nếu cần)
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Thực hiện hiệu ứng tối dần màn hình và sau đó tải Scene mới.
    /// </summary>
    /// <param name="sceneIndex">Index của Scene cần tải.</param>
    /// <param name="duration">Thời gian (giây) để Fade Out.</param>
    public void FadeOutAndLoadScene(int sceneIndex, float duration)
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image chưa được gán trong UIManager.");
            SceneManager.LoadScene(sceneIndex); // Chuyển Scene ngay lập tức nếu thiếu Fade Image
            return;
        }

        // Bắt đầu Coroutine để làm tối màn hình và chuyển Scene
        StartCoroutine(PerformFadeOut(sceneIndex, duration));
    }

    private IEnumerator PerformFadeOut(int sceneIndex, float duration)
    {
        float startTime = Time.time;
        Color startColor = fadeImage.color;

        // Đảm bảo Image luôn có màu đen (RGB=0,0,0) và kích hoạt tương tác để chặn input
        fadeImage.color = new Color(0, 0, 0, startColor.a);
        fadeImage.raycastTarget = true;

        // Tối dần màn hình đến Alpha = 1 (hoàn toàn đen)
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(startColor.a, 1f, t));
            yield return null;
        }

        // Đảm bảo màu cuối cùng là đen tuyệt đối
        fadeImage.color = new Color(0, 0, 0, 1f);

        // Chuyển Scene
        SceneManager.LoadScene(sceneIndex);
    }
}
