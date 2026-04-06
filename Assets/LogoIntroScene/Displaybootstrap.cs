using UnityEngine;

/// <summary>
/// Script này đảm bảo game luôn khởi động ở chế độ Fullscreen 
/// và đúng tỉ lệ màn hình mong muốn, bất kể cài đặt cũ.
/// </summary>
public class DisplayBootstrap : MonoBehaviour
{
    [Header("Cấu hình hiển thị")]
    [SerializeField] private bool forceFullScreenOnStart = true;
    [SerializeField] private float targetAspectRatio = 1.3333f; // 4:3 = 1.3333

    // Awake chạy trước cả Start, giúp ép màn hình nhanh nhất có thể
    void Awake()
    {
        if (forceFullScreenOnStart)
        {
            ApplyFullScreenSettings();
        }
    }

    private void ApplyFullScreenSettings()
    {
        // 1. Lấy độ phân giải vật lý của màn hình hiện tại
        int screenHeight = Screen.currentResolution.height;

        // 2. Tính toán chiều rộng dựa trên tỉ lệ 4:3
        int calculatedWidth = Mathf.RoundToInt(screenHeight * targetAspectRatio);

        // 3. Ép Unity đặt lại độ phân giải và chế độ hiển thị
        // FullScreenMode.FullScreenWindow là chế độ toàn màn hình tối ưu nhất hiện nay
        Screen.SetResolution(calculatedWidth, screenHeight, FullScreenMode.FullScreenWindow);

        // 4. Reset lại PlayerPrefs của Unity (Nơi Unity tự lưu cấu hình màn hình)
        // Điều này đảm bảo các lần khởi động sau không bị "nhớ" chế độ Windowed
        PlayerPrefs.SetInt("IsFullScreen", 1);

        Debug.Log($"[DisplayBootstrap] Đã ép độ phân giải: {calculatedWidth}x{screenHeight} (Full Screen)");
    }
}