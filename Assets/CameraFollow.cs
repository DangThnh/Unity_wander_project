using UnityEngine;

public class FixedCameraLookAtPlayer : MonoBehaviour
{
    // Kéo và thả đối tượng nhân vật vào đây trong Inspector
    public Transform playerTarget;

    // Tốc độ xoay của camera (để việc xoay mượt mà hơn)
    public float rotationSpeed = 5.0f;

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