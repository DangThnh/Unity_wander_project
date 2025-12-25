using UnityEngine;

// Script này phải được gắn vào Camera chính
public class RetroRender : MonoBehaviour
{
    // Cài đặt độ phân giải render cố định (ví dụ: 320x240, 480x270)
    [Header("Retro Resolution")]
    [Tooltip("Chiều rộng render cố định (ví dụ: 320)")]
    public int renderWidth = 320;
    [Tooltip("Chiều cao render cố định (ví dụ: 240)")]
    public int renderHeight = 240;

    // Tham chiếu đến Render Texture thấp
    private RenderTexture lowResRT;

    void OnEnable()
    {
        // Khởi tạo Render Texture với độ phân giải thấp và độ sâu 24 bit
        lowResRT = new RenderTexture(renderWidth, renderHeight, 24)
        {
            // Rất quan trọng: Đặt Filter Mode là Point để khi texture được phóng to
            // các pixel sẽ hiển thị sắc nét dưới dạng khối vuông, không bị làm mờ.
            filterMode = FilterMode.Point
        };
    }

    void OnDisable()
    {
        // Giải phóng Render Texture khi script bị vô hiệu hóa hoặc bị hủy
        if (lowResRT != null)
        {
            lowResRT.Release();
            lowResRT = null;
        }
    }

    // Phương thức Post-processing, được gọi sau khi camera đã render xong
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (lowResRT == null)
        {
            // Tránh lỗi nếu texture chưa được khởi tạo đúng cách
            OnEnable();
            if (lowResRT == null)
            {
                Graphics.Blit(source, destination); // Dùng render bình thường nếu thất bại
                return;
            }
        }

        // 1. Render cảnh vào Render Texture có độ phân giải thấp
        // Chuyển từ nguồn (source - độ phân giải màn hình) sang lowResRT.
        // Dùng Graphics.Blit với FilterMode mặc định để đảm bảo mọi thứ được vẽ.
        Graphics.Blit(source, lowResRT);

        // 2. Phóng to Render Texture thấp lên màn hình (destination)
        // Khi chuyển từ lowResRT (thấp) sang destination (cao), FilterMode.Point 
        // của lowResRT sẽ đảm bảo quá trình phóng to tạo ra các pixel khối sắc nét.
        Graphics.Blit(lowResRT, destination);
    }
}
