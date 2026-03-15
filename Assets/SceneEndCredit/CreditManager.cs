using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CreditSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI continueText;
    public Image fadeOverlayImage;

    [Header("Settings")]
    public int mainMenuSceneIndex = 1;
    public float blinkSpeed = 0.5f;
    public float fadeDuration = 1.5f; // Tăng một chút cho mượt
    public float delayBeforeLoad = 0.5f;

    private bool isBlinking = false;
    private bool isTransitioning = false;

    void Start()
    {
        // Khởi tạo trạng thái Fade (Bắt đầu từ đen hoặc trong suốt tùy bạn)
        if (fadeOverlayImage != null)
        {
            Color c = fadeOverlayImage.color;
            c.a = 0;
            fadeOverlayImage.color = c;
            fadeOverlayImage.gameObject.SetActive(true);
            fadeOverlayImage.raycastTarget = false;
        }

        if (continueText != null)
        {
            StartBlinking();
        }
    }

    void Update()
    {
        // Kiểm tra phím E để quay lại Menu
        if (Input.GetKeyDown(KeyCode.E) && !isTransitioning)
        {
            StartCoroutine(TransitionToMainMenu());
        }
    }

    private void StartBlinking()
    {
        if (!isBlinking)
        {
            isBlinking = true;
            StartCoroutine(BlinkTextCoroutine());
        }
    }

    IEnumerator BlinkTextCoroutine()
    {
        while (isBlinking)
        {
            if (continueText != null) continueText.enabled = !continueText.enabled;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    IEnumerator TransitionToMainMenu()
    {
        isTransitioning = true;
        isBlinking = false;

        if (continueText != null) continueText.enabled = true;

        // 1. Hiệu ứng Fade Out sang Đen
        if (fadeOverlayImage != null)
        {
            fadeOverlayImage.raycastTarget = true;
            float timer = 0;
            Color tempColor = fadeOverlayImage.color;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                tempColor.a = Mathf.Clamp01(timer / fadeDuration);
                fadeOverlayImage.color = tempColor;
                yield return null;
            }
        }

        // 2. THỰC HIỆN DỌN DẸP TRIỆT ĐỂ
        CleanUpGameState();

        yield return new WaitForSeconds(delayBeforeLoad);

        // 3. LOAD SCENE
        Debug.Log("Resetting and Loading Menu Scene...");
        SceneManager.LoadScene(mainMenuSceneIndex, LoadSceneMode.Single);
    }

    private void CleanUpGameState()
    {
        // A. Hủy các Singleton/Managers cứng đầu
        // Thay vì tìm theo tên (dễ sai), ta tìm các Object được đánh dấu DontDestroyOnLoad
        // Cách tốt nhất là gọi một hàm Reset chuyên biệt từ GameManager của bạn nếu có

        string[] managersToDestroy = { "GameManager", "InventoryManager", "GlobalAudio", "PlayerStatus" };
        foreach (string managerName in managersToDestroy)
        {
            GameObject obj = GameObject.Find(managerName);
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        // B. QUAN TRỌNG: Reset các biến Static (Nếu bạn có)
        // Ví dụ: Inventory.Instance.Clear(); hoặc ScoreManager.CurrentScore = 0;
        // Bạn phải gọi thủ công ở đây nếu không chúng sẽ tồn tại mãi mãi.

        // C. Reset hệ thống vật lý và thời gian
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // D. Giải phóng bộ nhớ không còn sử dụng
        System.GC.Collect();

        // E. Cấu hình lại Cursor cho Main Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        isBlinking = false;
        StopAllCoroutines();
    }
}