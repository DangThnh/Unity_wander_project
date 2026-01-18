using UnityEngine;

// Đổi tên class thành CameraFollow để khớp với tên file.
public class CameraFollow : MonoBehaviour
{
    // Không cần biến public để kéo thả nữa, vì nó sẽ tự tìm Player
    private Transform playerTarget;

    // Tốc độ xoay của camera (để việc xoay mượt mà hơn)
    public float rotationSpeed = 5.0f;

    void Awake()
    {
        // Gắn playerTarget ngay khi Scene được tải.
        // Khắc phục lỗi CS0117 bằng cách sử dụng FindObjectOfType<T>()
        // để tìm đối tượng Player trong Scene.

        Character_movement playerComponent = FindObjectOfType<Character_movement>();

        if (playerComponent != null)
        {
            playerTarget = playerComponent.transform;
            Debug.Log("Camera đã tìm thấy và thiết lập Player Target.");
        }
        else
        {
            Debug.LogError("Lỗi: Không tìm thấy Character_movement trong Scene. Đảm bảo Player được load/sinh ra trước Camera.");
        }
    }

    void LateUpdate()
    {
        // Kiểm tra xem có mục tiêu (player) để nhìn vào không
        if (playerTarget == null)
            return;

        // Tính toán góc xoay cần thiết để nhìn thẳng vào nhân vật
        // Quaternion.LookRotation sẽ tạo ra một góc xoay từ vị trí hiện tại
        // của camera tới vị trí của nhân vật.
        Quaternion targetRotation = Quaternion.LookRotation(playerTarget.position - transform.position);

        // Áp dụng góc xoay một cách mượt mà theo thời gian
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}