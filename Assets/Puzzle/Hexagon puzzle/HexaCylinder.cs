using UnityEngine;
using System.Collections;

// Script này quản lý việc xoay và trạng thái của một hình trụ Hexa đơn lẻ
public class RotatableCylinder : MonoBehaviour
{
    // Góc xoay cho mỗi lần bấm 'E' (360 độ / 6 mặt = 60 độ)
    private const float ROTATION_ANGLE = 60.0f;
    // Tốc độ xoay để chuyển động mượt mà
    private const float ROTATION_SPEED = 300.0f;

    [Tooltip("Chỉ số biểu tượng hiện tại (0 đến 5)")]
    public int currentSymbolIndex = 0; // Trạng thái ban đầu

    // Hiệu ứng phóng to khi hình trụ được chọn
    private Vector3 originalScale;
    private const float SELECTED_SCALE_FACTOR = 1.1f; // Phóng to 10%

    // Coroutine đang chạy cho việc xoay
    private Coroutine rotateCoroutine;

    // Gán biến này cho transform của hình trụ (ví dụ: mô hình 3D bên trong)
    [Tooltip("Transform cần xoay (thường là child của GameObject này)")]
    public Transform rotatingVisual;

    void Awake()
    {
        // Ghi lại kích thước ban đầu
        originalScale = transform.localScale;
        if (rotatingVisual == null)
        {
            Debug.LogError("Rotating Visual Transform chưa được gán trong RotatableCylinder!");
        }
    }

    // Xoay hình trụ 60 độ theo chiều kim đồng hồ (trục Y)
    public void RotateClockwise()
    {
        // Tránh xoay nếu đang trong quá trình xoay
        if (rotateCoroutine != null)
        {
            return;
        }

        // Cập nhật chỉ số biểu tượng (xoay thuận chiều kim đồng hồ)
        currentSymbolIndex = (currentSymbolIndex + 1) % 6;

        // Tính toán góc mục tiêu (Quay 60 độ quanh trục Y)
        Quaternion targetRotation = rotatingVisual.localRotation * Quaternion.Euler(0, ROTATION_ANGLE, 0);

        // Bắt đầu coroutine để xoay mượt mà
        rotateCoroutine = StartCoroutine(SmoothRotate(targetRotation));
    }

    // Coroutine để xoay mượt mà
    private IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        while (Quaternion.Angle(rotatingVisual.localRotation, targetRotation) > 0.01f)
        {
            rotatingVisual.localRotation = Quaternion.RotateTowards(
                rotatingVisual.localRotation,
                targetRotation,
                ROTATION_SPEED * Time.deltaTime
            );
            yield return null;
        }
        // Đảm bảo xoay chính xác đến góc mục tiêu
        rotatingVisual.localRotation = targetRotation;
        rotateCoroutine = null;
    }

    // Hiển thị trạng thái được chọn (phóng to)
    public void Select()
    {
        // Sử dụng một animation/tweening để phóng to mượt mà hơn trong thực tế
        transform.localScale = originalScale * SELECTED_SCALE_FACTOR;
        // Thêm hiệu ứng ánh sáng/màu sắc tại đây
    }

    // Bỏ trạng thái được chọn (trở về kích thước ban đầu)
    public void Deselect()
    {
        // Sử dụng một animation/tweening để thu nhỏ mượt mà hơn trong thực tế
        transform.localScale = originalScale;
        // Xóa hiệu ứng ánh sáng/màu sắc tại đây
    }
}