using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoorInteraction : MonoBehaviour
{
    [Header("UI & Text")]
    public TextMeshProUGUI interactionText;

    [Header("Cấu hình xoay")]
    public Transform hingePoint;
    public float openAngle = 90f;
    public float rotationSpeed = 2.0f;

    [Header("Âm thanh & Thành phần phát")]
    public AudioSource audioSource; // Bây giờ có thể kéo thả tùy ý từ Inspector
    public AudioClip openSound;
    [Range(0, 1)] public float volume = 1.0f;

    private bool playerInRange = false;
    private bool isOpened = false;
    private bool isRotating = false;
    private BoxCollider doorCollider;

    void Awake()
    {
        // Lấy Box Collider của cánh cửa
        doorCollider = GetComponent<BoxCollider>();

        // Kiểm tra nếu chưa kéo thả AudioSource vào Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            // Nếu vẫn không tìm thấy component nào trên object, lúc này mới tạo mới
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Đảm bảo cấu hình âm thanh được tối ưu (giữ nguyên logic cũ)
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
        }

        if (doorCollider == null)
        {
            Debug.LogError("Không tìm thấy BoxCollider trên đối tượng cửa!");
        }

        if (interactionText != null)
        {
            interactionText.text = "";
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
            if (interactionText != null)
            {
                interactionText.text = "";
            }
        }
    }

    private void OpenDoor()
    {
        isOpened = true;
        isRotating = true;

        // Xử lý phát âm thanh
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound, volume);
        }

        // Ẩn text khi đã mở
        if (interactionText != null)
        {
            interactionText.text = "";
        }

        // Vô hiệu hóa collider để người chơi có thể đi qua
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }
}