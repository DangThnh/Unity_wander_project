using UnityEngine;

public class CameraHeightFollow : MonoBehaviour
{
    [Header("Target Settings")]
    private Transform playerTarget;

    [Header("Height Settings")]
    [Tooltip("Khoảng cách chiều cao chênh lệch mặc định giữa Camera và Player")]
    public float heightOffset = 10f;

    [Tooltip("Tốc độ mượt mà khi thay đổi độ cao")]
    public float smoothSpeed = 10.0f;

    [Header("Position Constraints")]
    private float fixedX;
    private float fixedZ;

    void Awake()
    {
        // Tự động tìm nhân vật dựa trên script di chuyển (giống script cũ của bạn)
        Character_movement playerComponent = FindObjectOfType<Character_movement>();

        if (playerComponent != null)
        {
            playerTarget = playerComponent.transform;

            // Lưu lại vị trí X và Z hiện tại của Camera để giữ chúng cố định
            fixedX = transform.position.x;
            fixedZ = transform.position.z;

            // Nếu bạn muốn camera tự tính toán offset dựa trên vị trí hiện tại trong Scene:
            // heightOffset = transform.position.y - playerTarget.position.y;

            Debug.Log("Camera Height Follow: Đã tìm thấy Player.");
        }
        else
        {
            Debug.LogError("Camera Height Follow: Không tìm thấy Character_movement trong Scene.");
        }
    }

    void LateUpdate()
    {
        if (playerTarget == null) return;

        // 1. Xác định độ cao mục tiêu (Độ cao nhân vật + khoảng cách bù)
        float targetY = playerTarget.position.y + heightOffset;

        // 2. Nội suy mượt mà từ độ cao hiện tại đến độ cao mục tiêu
        float smoothedY = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);

        // 3. Cập nhật vị trí mới (X và Z giữ nguyên, chỉ thay đổi Y)
        transform.position = new Vector3(fixedX, smoothedY, fixedZ);

        // Lưu ý: Góc xoay (Rotation) sẽ không bị thay đổi vì chúng ta không code phần đó.
        // Bạn có thể tự tay chỉnh góc xoay của Camera trong Inspector để có góc nhìn ưng ý.
    }
}