using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Cấu hình Chuyển cảnh")]
    public string firstLevelName = "Level1";
    public float initialDelay = 1.0f;      // Thời gian chờ trước khi bắt đầu fade
    public float fadeDuration = 1.5f;     // Thời gian làm tối màn hình
    public Image fadeOverlay;             // Kéo một Image màu đen che đầy màn hình vào đây

    [Header("Âm thanh")]
    public AudioSource musicSource;       // Nguồn phát nhạc nền
    public AudioSource sfxSource;         // Nguồn phát tiếng động (SFX)
    public AudioClip hoverSound;          // Tiếng khi di chuột qua nút
    public AudioClip clickSound;          // Tiếng khi bấm nút

    private bool isStarting = false;

    void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            // Bắt đầu bằng việc làm sáng màn hình (Fade In)
            StartCoroutine(Fade(1, 0));
        }

        // Đảm bảo nhạc nền được phát
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- LOGIC CÁC NÚT BẤM ---

    public void OnStartButtonClick()
    {
        if (isStarting) return;
        isStarting = true;

        PlayClickSound();
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        // 1. Chờ khoảng 1s theo yêu cầu
        yield return new WaitForSeconds(initialDelay);

        // 2. Làm tối dần màn hình (Fade Out)
        yield return StartCoroutine(Fade(0, 1));

        // 3. Chuyển Scene
        SceneManager.LoadScene(firstLevelName);
    }

    public void OnQuitButtonClick()
    {
        PlayClickSound();
        Debug.Log("Exiting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnLoadButtonClick()
    {
        PlayClickSound();
        Debug.Log("Load logic will be implemented here later.");
    }

    // --- HỆ THỐNG ÂM THANH ---

    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    private void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    // --- HIỆU ỨNG FADE ---

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = fadeOverlay.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeOverlay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeOverlay.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}