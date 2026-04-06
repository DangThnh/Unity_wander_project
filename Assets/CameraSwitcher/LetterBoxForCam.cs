using UnityEngine;
using UnityEngine.UI;

public class CinematicBarsPlayer : MonoBehaviour
{
    [Header("UI References (Kéo thả từ Hierarchy)")]
    [Tooltip("Thanh đen phía trên (RectTransform).")]
    public RectTransform topBar;

    [Tooltip("Thanh đen phía dưới (RectTransform).")]
    public RectTransform bottomBar;

    [Header("Cấu hình Hiệu ứng")]
    [Tooltip("Độ cao mục tiêu của mỗi thanh đen (pixel).")]
    public float targetBarHeight = 150f;

    [Tooltip("Tốc độ co dãn.")]
    public float transitionSpeed = 5f;

    [Tooltip("Độ lệch dọc (Offset).")]
    public float verticalOffset = 0f;

    [Header("Cấu hình Trigger")]
    [Tooltip("Tag của vùng Trigger để kích hoạt.")]
    public string triggerTag = "TriggerCamRatio";

    // Quản lý trạng thái nội bộ
    private float currentHeight = 0f;
    private float currentOffset = 0f;
    private bool isCinematicActive = false;

    void Start()
    {
        // Đảm bảo ban đầu các thanh đen có chiều cao bằng 0 nếu đã được gán
        if (topBar != null && bottomBar != null)
        {
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, 0f);
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, 0f);
        }
    }

    void Update()
    {
        if (topBar == null || bottomBar == null) return;

        // Tính toán các giá trị mục tiêu
        float targetHeight = isCinematicActive ? targetBarHeight : 0f;
        float targetOffset = isCinematicActive ? verticalOffset : 0f;

        // Nội suy mượt mà
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * transitionSpeed);
        currentOffset = Mathf.Lerp(currentOffset, targetOffset, Time.deltaTime * transitionSpeed);

        UpdateUIElements();
    }

    private void UpdateUIElements()
    {
        // Cập nhật kích thước
        topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, currentHeight);
        bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, currentHeight);

        // Áp dụng vị trí dựa trên Offset
        // Lưu ý: topBar thường có pivot (0.5, 1) và bottomBar là (0.5, 0)
        topBar.anchoredPosition = new Vector2(0, currentOffset);
        bottomBar.anchoredPosition = new Vector2(0, currentOffset);
    }

    // --- Xử lý Va chạm 3D ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag)) isCinematicActive = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag)) isCinematicActive = false;
    }

    // --- Xử lý Va chạm 2D ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(triggerTag)) isCinematicActive = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(triggerTag)) isCinematicActive = false;
    }
}