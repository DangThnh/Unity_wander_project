using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;
using System.IO; // Thêm thư viện này để đọc file

public class MainMenuManager : MonoBehaviour
{
    [Header("Cấu hình Chuyển cảnh")]
    public string tutorialLevelName = "TutorialScene";
    public float initialDelay = 0.5f;
    public float fadeDuration = 1.0f;
    public Image fadeOverlay;

    [Header("Logic Load Game")]
    [Tooltip("Nút Load Game trong Menu (Sẽ tự ẩn nếu không có file save)")]
    public Button loadGameButton;

    [Header("Âm thanh & Mixer")]
    public AudioMixer mainMixer;
    public string volumeParameter = "MasterVol";
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public Slider volumeSlider;

    [Header("Giao diện Settings")]
    public GameObject settingsPanel;
    public Toggle fullScreenToggle;

    private bool isTransitioning = false;
    private bool isInitializing = true;
    private string saveFilePath;

    // Tỉ lệ mục tiêu 4:3
    private const int targetWidth = 860;
    private const int targetHeight = 645;

    void Start()
    {
        // Khởi tạo đường dẫn file save (phải trùng với SimpleSaveSystem)
        saveFilePath = Path.Combine(Application.persistentDataPath, "autosave_data.txt");

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            StartCoroutine(Fade(1, 0));
        }

        if (settingsPanel != null) settingsPanel.SetActive(false);

        isInitializing = true;
        LoadSettings();
        CheckSaveFile(); // Kiểm tra xem có file save để hiện nút Load không
        isInitializing = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- LOGIC LOAD GAME ---

    private void CheckSaveFile()
    {
        if (loadGameButton != null)
        {
            // Chỉ hiện nút Load nếu file save tồn tại
            loadGameButton.interactable = File.Exists(saveFilePath);
        }
    }

    public void OnLoadButtonClick()
    {
        if (isTransitioning || !File.Exists(saveFilePath)) return;

        isTransitioning = true;
        PlayClickSound();

        try
        {
            string content = File.ReadAllText(saveFilePath);
            if (int.TryParse(content, out int savedSceneIndex))
            {
                StartCoroutine(TransitionToScene(savedSceneIndex));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi đọc file save tại Menu: " + e.Message);
            isTransitioning = false;
        }
    }

    private IEnumerator TransitionToScene(int sceneIndex)
    {
        yield return new WaitForSeconds(initialDelay);
        yield return StartCoroutine(Fade(0, 1));
        SceneManager.LoadScene(sceneIndex);
    }

    // --- LOGIC CHUYỂN CẢNH (NEW GAME) ---

    public void OnPlayButtonClick()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        PlayClickSound();
        StartCoroutine(SwitchToTutorial());
    }

    private IEnumerator SwitchToTutorial()
    {
        yield return new WaitForSeconds(initialDelay);
        yield return StartCoroutine(Fade(0, 1));
        SceneManager.LoadScene(tutorialLevelName);
    }

    // --- HỆ THỐNG SETTINGS ---

    public void OpenSettings()
    {
        PlayClickSound();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayClickSound();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        PlayerPrefs.Save();
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
        float savedVol = PlayerPrefs.GetFloat("SavedVolume", 0.75f);
        if (volumeSlider != null) volumeSlider.value = savedVol;
        SetVolume(savedVol);

        bool isFull = PlayerPrefs.GetInt("IsFullScreen", 0) == 1;
        if (fullScreenToggle != null) fullScreenToggle.isOn = isFull;
        SetFullScreen(isFull);
    }

    public void OnQuitButtonClick()
    {
        PlayClickSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- AM THANH & HIỆU ỨNG ---

    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null) sfxSource.PlayOneShot(hoverSound);
    }

    private void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeOverlay == null) yield break;
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