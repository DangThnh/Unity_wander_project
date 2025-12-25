using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoorPasscodeVendingMachine : MonoBehaviour
{
    // Kéo và thả TextMeshProUGUI từ Inspector vào đây
    public TextMeshProUGUI interactionText;

    // Cánh cửa sẽ xoay quanh điểm này
    public Transform hingePoint;

    // Góc cửa sẽ mở
    public float openAngle = 90f;

    // Tốc độ xoay của cửa
    public float rotationSpeed = 2.0f;

    // Tham chiếu đến bảng nhập passcode
    public PasscodePanelVendingMachine passcodePanel;

    private bool playerInRange = false;
    private bool isOpened = false;
    private bool isRotating = false;
    private BoxCollider doorCollider;

    void Awake()
    {
        doorCollider = GetComponent<BoxCollider>();
        if (doorCollider == null)
        {
            Debug.LogError("BoxCollider not found on the door object!");
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isOpened)
        {
            if (passcodePanel != null)
            {
                passcodePanel.ShowKeypad();
                if (interactionText != null)
                {
                    // Tắt text khi mở bàn phím
                    interactionText.text = "Press Enter to confirm order\nPress E to close";
                }
            }
        }

        if (isRotating)
        {
            Quaternion targetRotation = Quaternion.Euler(0, openAngle, 0);
            hingePoint.localRotation = Quaternion.Slerp(hingePoint.localRotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(hingePoint.localRotation, targetRotation) < 0.1f)
            {
                isRotating = false;
                hingePoint.localRotation = targetRotation;
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
                interactionText.text = "";
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            EndInteraction();
        }
    }

    public void UnlockDoor()
    {
        isOpened = true;
        isRotating = true;

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }

    public void EndInteraction()
    {
        if (interactionText != null)
        {
            interactionText.text = "";
        }
    }
}