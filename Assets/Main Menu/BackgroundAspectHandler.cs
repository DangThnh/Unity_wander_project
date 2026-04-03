using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AspectRatioFitter))]
public class FlexibleBackgroundController : MonoBehaviour
{
    private AspectRatioFitter _aspectFitter;
    private RectTransform _rectTransform;

    [Header("Cấu hình Tỷ lệ")]
    [SerializeField] private float targetAspect = 1.3333f; // Tỷ lệ gốc của ảnh (4:3)

    private bool _lastFullScreenState;

    private void Awake()
    {
        _aspectFitter = GetComponent<AspectRatioFitter>();
        _rectTransform = GetComponent<RectTransform>();

        _aspectFitter.aspectRatio = targetAspect;
        _lastFullScreenState = Screen.fullScreen;

        ApplyCorrectMode();
    }

    private void Update()
    {
        // Chỉ cập nhật khi trạng thái FullScreen thay đổi để tránh xung đột RectTransform
        if (Screen.fullScreen != _lastFullScreenState)
        {
            _lastFullScreenState = Screen.fullScreen;
            ApplyCorrectMode();
        }
    }

    public void ApplyCorrectMode()
    {
        if (Screen.fullScreen)
        {
            // CHẾ ĐỘ TOÀN MÀN HÌNH (16:9 chẳng hạn)
            // Kích hoạt lại Fitter để tạo Black Bars (Letterbox)
            _aspectFitter.enabled = true;
            _aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        }
        else
        {
            // CHẾ ĐỘ CỬA SỔ (Windowed)
            // 1. Tắt Fitter để lấy lại quyền kiểm soát RectTransform
            _aspectFitter.enabled = false;

            // 2. Ép RectTransform giãn cực đại (Full Stretch) để xóa sạch Black Bars
            _rectTransform.anchorMin = new Vector2(0, 0);
            _rectTransform.anchorMax = new Vector2(1, 1);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }

    // Gọi hàm này từ UI Settings Button sau khi thực hiện lệnh Screen.fullScreen = false
    // để đảm bảo hiệu ứng cập nhật ngay lập tức
    public void OnSettingsChanged()
    {
        Invoke(nameof(ApplyCorrectMode), 0.1f); // Delay nhẹ để Screen cập nhật xong resolution
    }
}