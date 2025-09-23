using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    // Tham chiếu đến các thành phần UI
    public Image backgroundImage;
    public Image titleImage;
    public TextMeshProUGUI pressAnyKeyText;

    // Biến trạng thái
    private bool canProceedToNextScene = false;
    private IEnumerator blinkCoroutine; // Biến mới để lưu tham chiếu đến coroutine nhấp nháy

    void Start()
    {
        // Bắt đầu chuỗi hiệu ứng
        StartCoroutine(ShowMenuSequence());
    }

    void Update()
    {
        // Nếu các hiệu ứng đã xong và người chơi bấm phím bất kỳ
        if (canProceedToNextScene && Input.anyKeyDown)
        {
            StartCoroutine(FadeOutAndLoad());
        }
    }

    IEnumerator ShowMenuSequence()
    {
        // Giai đoạn 1: Hiệu ứng hình nền
        yield return StartCoroutine(FadeImage(backgroundImage, 2.0f, 1.0f));

        // Tạm dừng 1 giây
        yield return new WaitForSeconds(1.0f);

        // Giai đoạn 2: Hiệu ứng tiêu đề
        yield return StartCoroutine(FadeImage(titleImage, 2.0f, 1.0f));

        // Tạm dừng 2 giây
        yield return new WaitForSeconds(1.0f);

        // Giai đoạn 3: Hiệu ứng nhấp nháy
        // Lưu lại tham chiếu trước khi khởi chạy
        blinkCoroutine = BlinkText(pressAnyKeyText);
        StartCoroutine(blinkCoroutine);

        // Mở khóa chuyển cảnh
        canProceedToNextScene = true;
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

    // Coroutine để làm chữ nhấp nháy (đã bỏ tham số interval vì không cần thiết)
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
        canProceedToNextScene = false; // Ngăn người chơi bấm nút khi đang chuyển cảnh

        // Dừng chỉ coroutine nhấp nháy, không phải tất cả
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        // Làm mờ đồng thời cả 3 thành phần
        StartCoroutine(FadeImage(backgroundImage, 3.0f, 0.0f));
        StartCoroutine(FadeImage(titleImage, 3.0f, 0.0f));
        StartCoroutine(FadeText(pressAnyKeyText, 3.0f, 0.0f));

        // Đợi 3 giây để hiệu ứng hoàn thành
        yield return new WaitForSeconds(3.0f);

        // Đợi thêm 2 giây trước khi chuyển scene
        yield return new WaitForSeconds(2.0f);

        // Chuyển scene
        SceneManager.LoadScene(1);
    }
}