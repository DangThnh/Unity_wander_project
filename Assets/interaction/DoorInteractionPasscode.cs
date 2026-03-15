using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoorInteractionPasscode : MonoBehaviour
{
    [Header("UI & Interaction")]
    public TextMeshProUGUI interactionText;
    public PasscodePanel passcodePanel;

    [Header("Door Movement")]
    public Transform hingePoint;
    public float openAngle = 90f;
    public float rotationSpeed = 2.0f;

    [Header("Sound Effects")]
    public AudioSource audioSource; // Thành phần phát âm thanh
    public AudioClip openDoorSound;   // Tiếng kẽo kẹt mở cửa
    public AudioClip accessGrantedSound; // Tiếng "Ting" thành công
    public AudioClip accessDeniedSound;  // Tiếng "Bíp" sai mã
    public AudioClip interactionSound;   // Tiếng bấm nút nhẹ khi mở UI

    private bool playerInRange = false;
    private bool isOpened = false;
    private bool isRotating = false;
    private BoxCollider doorCollider;

    void Awake()
    {
        doorCollider = GetComponent<BoxCollider>();
        // Tự động lấy AudioSource nếu bạn quên gán trong Inspector
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Khi người chơi nhấn E để mở bàn phím
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isOpened)
        {
            if (passcodePanel != null)
            {
                passcodePanel.ShowKeypad();
                PlaySound(interactionSound); // Âm thanh tương tác ban đầu

                if (interactionText != null)
                {
                    interactionText.text = "Press Enter to confirm passcode\nPress E to close";
                }
            }
        }

        // Xử lý xoay cửa mượt mà
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

    // Hàm này sẽ được gọi từ PasscodePanel khi nhập ĐÚNG
    public void UnlockDoor()
    {
        if (isOpened) return;

        isOpened = true;
        isRotating = true;

        PlaySound(accessGrantedSound); // Âm thanh báo thành công

        // Phát tiếng mở cửa sau một khoảng trễ nhỏ hoặc phát cùng lúc
        Invoke("PlayOpenDoorSound", 0.5f);

        if (doorCollider != null) doorCollider.enabled = false;
    }

    // Hàm này sẽ được gọi từ PasscodePanel khi nhập SAI
    public void DenyAccess()
    {
        PlaySound(accessDeniedSound); // Âm thanh báo lỗi
    }

    private void PlayOpenDoorSound()
    {
        PlaySound(openDoorSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionText != null && !isOpened)
            {
                interactionText.text = "Press E to interact";
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

    public void EndInteraction()
    {
        if (interactionText != null) interactionText.text = "";
    }
}