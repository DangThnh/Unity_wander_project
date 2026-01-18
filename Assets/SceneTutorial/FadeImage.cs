using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class FadeImageManager : MonoBehaviour
{
    // Tham chiếu đến các thành phần UI
    public Image backgroundImage;
    public TextMeshProUGUI pressAnyKeyText;

    // Biến trạng thái
    // Đã thay đổi giá trị khởi tạo thành 'true' vì ngay khi scene bắt đầu
    // người chơi có thể bấm phím.
    private bool canProceedToNextScene = true;
    private IEnumerator blinkCoroutine; // Biến mới để lưu tham chiếu đến coroutine nhấp nháy

    void Start()
    {
        // Khởi tạo hiệu ứng nhấp nháy ngay khi bắt đầu scene
        if (pressAnyKeyText != null)
        {
            // Đảm bảo chữ hiển thị đầy đủ (alpha = 1.0f) trước khi bắt đầu nhấp nháy
            pressAnyKeyText.color = new Color(pressAnyKeyText.color.r, pressAnyKeyText.color.g, pressAnyKeyText.color.b, 1.0f);

            blinkCoroutine = BlinkText(pressAnyKeyText);
            StartCoroutine(blinkCoroutine);
        }
    }

    void Update()
    {
        // CHỈ cho phép gọi FadeOutAndLoad nếu canProceedToNextScene là true.
        // Điều này khắc phục cảnh báo CS0414 và ngăn ngừa việc gọi coroutine nhiều lần.
        if (canProceedToNextScene && Input.anyKeyDown)
        {
            // Khi người chơi bấm phím, ngay lập tức tắt cờ này
            canProceedToNextScene = false;
            StartCoroutine(FadeOutAndLoad());
        }
    }


    // Coroutine để làm mờ hình ảnh
    IEnumerator FadeImage(Image image, float duration, float targetAlpha)
    {
        float startAlpha = image.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            yield return null;
        }

        image.color = new Color(image.color.r, image.color.g, image.color.b, targetAlpha);
    }

    // Coroutine để làm chữ nhấp nháy
    IEnumerator BlinkText(TextMeshProUGUI text)
    {
        while (true)
        {
            yield return StartCoroutine(FadeText(text, 2.5f, 1.0f));
            yield return StartCoroutine(FadeText(text, 2.5f, 0.0f));
        }
    }

    // Coroutine để làm mờ chữ
    IEnumerator FadeText(TextMeshProUGUI text, float duration, float targetAlpha)
    {
        float startAlpha = text.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, targetAlpha);
    }

    // Coroutine mới để làm mờ tất cả các UI và tải scene
    IEnumerator FadeOutAndLoad()
    {
        // canProceedToNextScene đã được set = false ở Update() để ngăn re-entry

        // Dừng chỉ coroutine nhấp nháy, không phải tất cả
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        // Làm mờ đồng thời cả 3 thành phần
        StartCoroutine(FadeImage(backgroundImage, 3.0f, 0.0f));
        StartCoroutine(FadeText(pressAnyKeyText, 3.0f, 0.0f));

        // Đợi 3 giây để hiệu ứng hoàn thành
        yield return new WaitForSeconds(3.0f);

        // Đợi thêm 2 giây trước khi chuyển scene
        yield return new WaitForSeconds(2.0f);

        // Chuyển scene
        SceneManager.LoadScene(2);
    }
}