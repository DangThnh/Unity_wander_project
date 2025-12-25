using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class FadeImageManager: MonoBehaviour
{
    // Tham chiếu đến các thành phần UI
    public Image backgroundImage;
    public TextMeshProUGUI pressAnyKeyText;

    // Biến trạng thái
    private bool canProceedToNextScene = false;
    private IEnumerator blinkCoroutine; // Biến mới để lưu tham chiếu đến coroutine nhấp nháy

    void Start()
    {
       
    }

    void Update()
    {
        // Nếu các hiệu ứng đã xong và người chơi bấm phím bất kỳ
        if (Input.anyKeyDown)
        {
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
        StartCoroutine(FadeText(pressAnyKeyText, 3.0f, 0.0f));

        // Đợi 3 giây để hiệu ứng hoàn thành
        yield return new WaitForSeconds(3.0f);

        // Đợi thêm 2 giây trước khi chuyển scene
        yield return new WaitForSeconds(2.0f);

        // Chuyển scene
        SceneManager.LoadScene(2);
    }
}