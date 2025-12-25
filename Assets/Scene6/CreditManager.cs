using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Đảm bảo sử dụng thư viện TextMeshPro
using System.Collections;

public class CreditSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo TextMeshProUGUI của dòng chữ 'Press E to continue' vào đây.")]
    public TextMeshProUGUI continueText;

    [Header("Settings")]
    [Tooltip("Scene Index của Main Menu (Scene 1).")]
    public int mainMenuSceneIndex = 1;

    [Tooltip("Tốc độ nhấp nháy (giá trị càng thấp, nhấp nháy càng nhanh). Ví dụ: 0.5f")]
    public float blinkSpeed = 0.5f;

    private bool isBlinking = false;

    void Start()
    {
        // Khởi động hiệu ứng nhấp nháy ngay khi Scene được tải
        if (continueText != null)
        {
            StartBlinking();
        }
        else
        {
            Debug.LogError("Continue Text (TextMeshProUGUI) chưa được gán trong CreditSceneManager.");
        }
    }

    void Update()
    {
        // Kiểm tra input của người chơi
        if (Input.GetKeyDown(KeyCode.E))
        {
            LoadMainMenu();
        }
    }

    /// <summary>
    /// Bắt đầu Coroutine để làm chữ nhấp nháy.
    /// </summary>
    private void StartBlinking()
    {
        if (!isBlinking)
        {
            isBlinking = true;
            StartCoroutine(BlinkTextCoroutine());
        }
    }

    /// <summary>
    /// Coroutine để làm chữ nhấp nháy giữa hiển thị và ẩn.
    /// </summary>
    IEnumerator BlinkTextCoroutine()
    {
        while (isBlinking)
        {
            // Tắt Text
            continueText.enabled = false;
            yield return new WaitForSeconds(blinkSpeed);

            // Bật Text
            continueText.enabled = true;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    /// <summary>
    /// Tải lại Scene Main Menu.
    /// </summary>
    private void LoadMainMenu()
    {
        Debug.Log("Loading Main Menu (Scene " + mainMenuSceneIndex + ")...");
        // Dừng hiệu ứng nhấp nháy trước khi chuyển Scene
        StopAllCoroutines();
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    // Đảm bảo dừng nhấp nháy nếu script bị hủy
    private void OnDisable()
    {
        isBlinking = false;
    }
}
