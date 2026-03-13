using UnityEngine;

public class FixedPathCamera : MonoBehaviour
{
    [Header("Target Tracking")]
    public Transform player;            // Nhân vật cần theo dõi
    public Transform startPoint;        // Điểm bắt đầu đoạn đường
    public Transform endPoint;          // Điểm kết thúc đoạn đường

    [Header("Camera Offset")]
    public Vector3 positionOffset;      // Khoảng cách từ Camera đến Player (Ví dụ: 0, 5, -10)

    [Header("Rotation Settings")]
    public Vector3 climaxRotation;      // Góc xoay khi ở EndPoint
    public float transitionSpeed = 5f;  // Tốc độ mượt mà cho cả Vị trí và Xoay

    private Quaternion defaultRotation;
    private Quaternion targetClimaxRotation;
    private bool isPlayerInside = false;

    void Awake()
    {
        // Lưu lại góc xoay mặc định khi bắt đầu
        defaultRotation = transform.rotation;
        targetClimaxRotation = Quaternion.Euler(climaxRotation);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInside = false;
    }

    void LateUpdate() // Dùng LateUpdate để tránh giật hình khi nhân vật di chuyển
    {
        if (player == null) return;

        // 1. LOGIC VỊ TRÍ: Camera đi theo nhân vật nhưng giữ nguyên Offset
        // Việc cộng positionOffset trực tiếp này sẽ phớt lờ hoàn toàn việc Player đang quay hướng nào
        Vector3 desiredPosition = player.position + positionOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * transitionSpeed);

        // 2. LOGIC GÓC XOAY (FIXED ROTATION)
        Quaternion targetRot = defaultRotation;

        if (isPlayerInside && startPoint && endPoint)
        {
            // Tính toán Progress dựa trên hình chiếu lên đường thẳng (giống đoạn code trước)
            Vector3 line = endPoint.position - startPoint.position;
            float lineLength = line.magnitude;
            if (lineLength > 0.1f)
            {
                Vector3 lineDir = line.normalized;
                Vector3 playerOffset = player.position - startPoint.position;
                float dot = Vector3.Dot(playerOffset, lineDir);
                float progress = Mathf.Clamp01(dot / lineLength);

                // Tính toán góc xoay mục tiêu dựa trên tiến độ
                targetRot = Quaternion.Lerp(defaultRotation, targetClimaxRotation, progress);
            }
        }

        // Áp dụng góc xoay mượt mà
        // transform.rotation ở đây chỉ dựa vào logic Progress, không bị ảnh hưởng bởi đầu vào người chơi
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * transitionSpeed);
    }

    // Vẽ công cụ hỗ trợ trực quan trong Editor
    void OnDrawGizmos()
    {
        if (startPoint && endPoint)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawWireSphere(startPoint.position, 0.3f);
            Gizmos.DrawWireSphere(endPoint.position, 0.3f);
        }
    }
}