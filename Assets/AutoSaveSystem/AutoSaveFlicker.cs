using UnityEngine;
using TMPro; // Yêu cầu cài đặt TextMeshPro
using UnityEngine.UI;

public class AutoSaveFlicker : MonoBehaviour
{
    [Header("Cấu hình Hiệu ứng")]
    public float flickerSpeed = 5f; // Tốc độ nhấp nháy

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Sử dụng CanvasGroup để làm mờ cả Icon và Text cùng lúc
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        // Sử dụng hàm Sin để tạo giá trị từ 0 đến 1 nhịp nhàng
        if (canvasGroup != null)
        {
            canvasGroup.alpha = (Mathf.Sin(Time.time * flickerSpeed) + 1f) / 2f;
        }
    }
}