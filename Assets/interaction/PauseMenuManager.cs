using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using System.Collections;

public class PauseMenuManager : MonoBehaviour
{
    // --- STATIC STATE ĐỂ CÁC SCRIPT KHÁC KIỂM TRA ---
    public static bool IsPausedStatic { get; private set; } = false;

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
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private bool isInitializing = true;
    private bool isTransitioning = false;

    private const int targetWidth = 860;
    private const int targetHeight = 645;
    private const float targetAspect = 4f / 3f;

    void Start()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.raycastTarget = true;
            StartCoroutine(Fade(1, 0));
        }

        isInitializing = true;
        LoadSettings();
        isInitializing = false;

        Time.timeScale = 1f;
        IsPausedStatic = false; // Reset trạng thái khi bắt đầu
    }

    void Update()
    {
        // Nếu đang giải đố, không cho phép mở Pause Menu bằng Esc
        if (HexaPuzzleManager.IsPuzzleActiveStatic) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !isTransitioning)
        {
            TogglePause();
        }
    }

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
        IsPausedStatic = false; // Cập nhật biến static
    }

    void PauseGame()
    {
        // Chặn mở Pause Menu nếu Inventory đang mở (tùy chọn, tùy logic game của bạn)
        // if (InventoryUI.IsInventoryOpenStatic) return;

        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        IsPausedStatic = true; // Cập nhật biến static
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // ... (Các hàm khác giữ nguyên như cũ: OpenSettings, CloseSettings, BackToMainMenu, v.v.)

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
        PlayerPrefs.Save();
    }

    public void BackToMainMenu()
    {
        if (isTransitioning) return;
        PlayClickSound();
        StartCoroutine(TransitionToMenu());

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

    private IEnumerator TransitionToMenu()
    {
        isTransitioning = true;
        Time.timeScale = 1f;
        if (fadeOverlay != null) fadeOverlay.raycastTarget = true;
        yield return StartCoroutine(Fade(0, 1));
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SetVolume(float sliderValue)
    {
        if (mainMixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
        mainMixer.SetFloat(volumeParameter, dB);
        PlayerPrefs.SetFloat("SavedVolume", sliderValue);
    }

    public void SetFullScreen(bool isFull)
    {
        if (!isInitializing) PlayClickSound();
        if (isFull)
        {
            int screenHeight = Screen.currentResolution.height;
            int calculatedWidth = Mathf.RoundToInt(screenHeight * targetAspect);
            Screen.SetResolution(calculatedWidth, screenHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed);
        }
        PlayerPrefs.SetInt("IsFullScreen", isFull ? 1 : 0);
    }

    private void LoadSettings()
    {
        float savedVol = PlayerPrefs.GetFloat("SavedVolume", 0.75f);
        if (volumeSlider != null) volumeSlider.value = savedVol;
        SetVolume(savedVol);
        bool isFull = PlayerPrefs.GetInt("IsFullScreen", 0) == 1;
        if (fullScreenToggle != null) fullScreenToggle.isOn = isFull;
        SetFullScreen(isFull);
    }

    public void PlayHoverSound()
    {
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
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeOverlay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        fadeOverlay.color = new Color(color.r, color.g, color.b, endAlpha);
        if (endAlpha <= 0.1f) fadeOverlay.raycastTarget = false;
    }
}