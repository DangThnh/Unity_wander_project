using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeManager : MonoBehaviour
{
    [Header("Cấu hình Hiệu ứng")]
    public Image fadeImage;          // Kéo tấm ảnh đen vào đây
    public float fadeSpeed = 0.8f;   // Tốc độ sáng dần (càng cao càng nhanh)
    public float startDelay = 0.2f;  // Đợi một chút rồi mới bắt đầu sáng dần

    private void Awake()
    {
        // Đảm bảo ngay khi Scene khởi chạy, màn hình PHẢI đen kịt
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // Bắt đầu quá trình làm sáng màn hình
        StartCoroutine(DoFadeIn());
    }

    private IEnumerator DoFadeIn()
    {
        // Đợi một chút để tránh hiện tượng "giật" hình khi vừa load scene
        yield return new WaitForSeconds(startDelay);

        float alpha = 1f;

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;

            // Cập nhật màu sắc cho Image
            Color c = fadeImage.color;
            c.a = Mathf.Clamp01(alpha);
            fadeImage.color = c;

            yield return null;
        }

        // Sau khi xong thì tắt Image đi để không cản trở việc click chuột vào game
        fadeImage.gameObject.SetActive(false);
    }
}