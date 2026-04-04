using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public Image fadeOverlay;

    [Header("Âm thanh & Mixer")]
    public AudioMixer mainMixer;
    public string volumeParameter = "MasterVol";
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public Slider volumeSlider;

    [Header("Giao diện Settings")]
    public Toggle fullScreenToggle;

    [Header("Cấu hình Chuyển cảnh")]
    public float fadeDuration = 1.0f;
    public string mainMenuSceneName = "MainMenu"; // Tên scene Menu của bạn

    private bool isPaused = false;
    private bool isInitializing = true;
    private bool isTransitioning = false;

    // Tỉ lệ mục tiêu 4:3 đồng bộ với MainMenu
    private const int targetWidth = 860;
    private const int targetHeight = 645;

    void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            // Hiệu ứng mờ dần khi vào màn chơi
            StartCoroutine(Fade(1, 0));
        }

        // Load cài đặt từ PlayerPrefs để đồng bộ với Menu
        isInitializing = true;
        LoadSettings();
        isInitializing = false;

        // Đảm bảo thời gian chạy bình thường khi bắt đầu
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isTransitioning)
        {
            TogglePause();
        }
    }

    // --- LOGIC TẠM DỪNG ---

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void ResumeGame()
    {
        PlayClickSound();
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        // Ẩn chuột nếu game của bạn yêu cầu
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }

    void PauseGame()
    {
        pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f; // Dừng toàn bộ logic game
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- ĐIỀU HƯỚNG ---

    public void OpenSettings()
    {
        PlayClickSound();
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayClickSound();
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        PlayerPrefs.Save(); // Lưu lại thiết lập xuống ổ cứng
    }

    public void BackToMainMenu()
    {
        if (isTransitioning) return;
        PlayClickSound();
        StartCoroutine(TransitionToMenu());
    }

    private IEnumerator TransitionToMenu()
    {
        isTransitioning = true;
        Time.timeScale = 1f; // PHẢI trả lại thời gian để Coroutine chạy được
        yield return StartCoroutine(Fade(0, 1));
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // --- HỆ THỐNG SETTINGS (Copy đồng bộ từ MainMenuManager) ---

    public void SetVolume(float sliderValue)
    {
        if (mainMixer == null) return;
        // Công thức tính dB chuẩn
        float dB = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
        mainMixer.SetFloat(volumeParameter, dB);
        PlayerPrefs.SetFloat("SavedVolume", sliderValue);
    }

    public void SetFullScreen(bool isFull)
    {
        if (!isInitializing) PlayClickSound();

        if (isFull)
        {
            Resolution maxRes = Screen.currentResolution;
            Screen.SetResolution(maxRes.width, maxRes.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed);
        }

        PlayerPrefs.SetInt("IsFullScreen", isFull ? 1 : 0);
    }

    private void LoadSettings()
    {
        // Lấy giá trị đã lưu từ Main Menu
        float savedVol = PlayerPrefs.GetFloat("SavedVolume", 0.75f);
        if (volumeSlider != null) volumeSlider.value = savedVol;
        SetVolume(savedVol);

        bool isFull = PlayerPrefs.GetInt("IsFullScreen", 0) == 1;
        if (fullScreenToggle != null) fullScreenToggle.isOn = isFull;
        SetFullScreen(isFull);
    }

    // --- ÂM THANH & HIỆU ỨNG ---

    public void PlayHoverSound()
    {
        // Sử dụng PlayOneShot để âm thanh không bị cắt ngang
        if (sfxSource != null && hoverSound != null)
            sfxSource.PlayOneShot(hoverSound);
    }

    private void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeOverlay == null) yield break;
        float elapsed = 0f;
        Color color = fadeOverlay.color;

        // Sử dụng WaitForSecondsRealtime vì Time.timeScale có thể bằng 0
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeOverlay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        fadeOverlay.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}