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

    [Header("Âm thanh (Kéo clip vào đây)")]
    public AudioClip openSound; // File âm thanh mở cửa
    [Range(0, 1)] public float volume = 1.0f; // Độ lớn âm thanh

    private bool playerInRange = false;
    private bool isOpened = false;
    private bool isRotating = false;
    private BoxCollider doorCollider;
    private AudioSource audioSource; // Thành phần phát âm thanh

    void Awake()
    {
        // Lấy Box Collider của cánh cửa
        doorCollider = GetComponent<BoxCollider>();

        // Thiết lập AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Tự động thêm AudioSource nếu đối tượng chưa có
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Cấu hình mặc định cho AudioSource để âm thanh nghe chân thực hơn (3D)
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // Chế độ âm thanh 3D (nghe xa gần)

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
                // Bạn có thể thêm nội dung như "Nhấn E để mở" ở đây
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