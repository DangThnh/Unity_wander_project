using UnityEngine;
using UnityEngine.UI;

public class CinematicBarsPlayer : MonoBehaviour
{
    [Header("Cấu hình Hiệu ứng")]
    [Tooltip("Độ cao của mỗi thanh đen (pixel).")]
    public float targetBarHeight = 150f;

    [Tooltip("Tốc độ co dãn và dịch chuyển khung hình.")]
    public float transitionSpeed = 5f;

    [Tooltip("Độ lệch dọc (Offset). Số dương đẩy khung hình lên, số âm kéo xuống.")]
    public float verticalOffset = 0f;

    [Header("Cấu hình Trigger")]
    [Tooltip("Tag của vùng Trigger để kích hoạt.")]
    public string triggerTag = "TriggerCamRatio";

    // Quản lý UI nội bộ
    private GameObject canvasObj;
    private RectTransform topBar;
    private RectTransform bottomBar;
    private float currentHeight = 0f;
    private float currentOffset = 0f;
    private bool isCinematicActive = false;

    void Start()
    {
        InitializeCinematicUI();
    }

    void Update()
    {
        // Tính toán các giá trị mục tiêu dựa trên trạng thái cinematic
        float targetHeight = isCinematicActive ? targetBarHeight : 0f;
        float targetOffset = isCinematicActive ? verticalOffset : 0f;

        // Nội suy mượt mà các giá trị để tạo hiệu ứng chuyển động
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * transitionSpeed);
        currentOffset = Mathf.Lerp(currentOffset, targetOffset, Time.deltaTime * transitionSpeed);

        UpdateUIElements();
    }

    private void InitializeCinematicUI()
    {
        // Tạo Canvas mới để chứa các thanh đen
        canvasObj = new GameObject("Cinematic_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Luôn hiển thị trên cùng

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Tạo 2 thanh đen trên và dưới
        topBar = CreateBlackBar("TopBar", canvasObj.transform, true);
        bottomBar = CreateBlackBar("BottomBar", canvasObj.transform, false);
    }

    private RectTransform CreateBlackBar(string name, Transform parent, bool isTop)
    {
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(parent);

        Image img = bar.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false; // Tránh chặn các sự kiện click chuột

        RectTransform rt = bar.GetComponent<RectTransform>();

        // Thiết lập Anchor và Pivot để thanh đen co dãn từ cạnh màn hình
        if (isTop)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
        }
        else
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
        }

        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 0);

        return rt;
    }

    private void UpdateUIElements()
    {
        if (topBar != null && bottomBar != null)
        {
            // Cập nhật kích thước (chiều cao) của thanh đen
            topBar.sizeDelta = new Vector2(0, currentHeight);
            bottomBar.sizeDelta = new Vector2(0, currentHeight);

            // Áp dụng Offset để dịch chuyển vị trí các thanh đen
            // Tạo hiệu ứng "khung hình" bị đẩy lên hoặc xuống
            topBar.anchoredPosition = new Vector2(0, currentOffset);
            bottomBar.anchoredPosition = new Vector2(0, currentOffset);
        }
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

    private void OnDestroy()
    {
        // Dọn dẹp UI khi object bị xóa
        if (canvasObj != null) Destroy(canvasObj);
    }
}