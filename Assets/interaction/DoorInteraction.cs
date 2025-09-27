using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoorInteraction : MonoBehaviour
{
    // Kéo và thả TextMeshProUGUI từ Inspector vào đây
    public TextMeshProUGUI interactionText;

    // Cánh cửa sẽ xoay quanh điểm này
    public Transform hingePoint;

    // Góc cửa sẽ mở
    public float openAngle = 90f;

    // Tốc độ xoay của cửa
    public float rotationSpeed = 2.0f;

    private bool playerInRange = false;
    private bool isOpened = false;
    private bool isRotating = false; // Biến mới để kiểm soát trạng thái xoay
    private BoxCollider doorCollider;

    void Awake()
    {
        // Lấy Box Collider của cánh cửa
        doorCollider = GetComponent<BoxCollider>();
        if (doorCollider == null)
        {
            Debug.LogError("BoxCollider not found on the door object!");
        }

        if (interactionText != null)
        {
            interactionText.text = ""; // Ẩn văn bản khi bắt đầu
        }
    }

    void Update()
    {
        // Chỉ xử lý khi người chơi trong phạm vi và bấm phím 'E'
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isOpened)
        {
            OpenDoor();
        }

        // Xoay cửa nếu nó đã được mở
        if (isRotating)
        {
            // Lệnh đã được thay đổi để xoay hingePoint
            Quaternion targetRotation = Quaternion.Euler(0, openAngle, 0);
            hingePoint.localRotation = Quaternion.Slerp(hingePoint.localRotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Kiểm tra nếu đã gần đạt góc xoay mục tiêu
            if (Quaternion.Angle(hingePoint.localRotation, targetRotation) < 0.1f)
            {
                // Dừng quá trình xoay để tiết kiệm tài nguyên
                isRotating = false;
                hingePoint.localRotation = targetRotation; // Đặt chính xác góc xoay cuối cùng
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionText != null && !isOpened)
            {
                interactionText.text = "Press 'E' to open";
            }
        }
    }
   

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionText != null)
            {
                interactionText.text = "";
            }
        }
    }

    private void OpenDoor()
    {
        isOpened = true;
        isRotating = true; // Kích hoạt biến xoay

        // Vô hiệu hóa collider để người chơi có thể đi qua
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }
}