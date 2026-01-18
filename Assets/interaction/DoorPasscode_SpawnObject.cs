using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Đổi tên class để phù hợp hơn với chức năng tương tác
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

    // === Cài đặt Spawn Object (Lấy cảm hứng từ InteractableObject) ===
    [Header("Spawn Object Settings")]
    [Tooltip("Đánh dấu nếu cánh cửa này sẽ spawn một vật phẩm sau khi mở.")]
    public bool isSpawnInteraction = false;
    [Tooltip("ID duy nhất cho hành động spawn này. Cần thiết để ngăn việc spawn lại sau khi lưu/tải game.")]
    public string spawnActionId;
    public GameObject objectToSpawnPrefab;
    public Transform spawnPoint;
    // ===============================================================

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

    // Thêm hàm Start để kiểm tra cảnh báo và trạng thái
    void Start()
    {
        // Kiểm tra cảnh báo nếu thiếu các tham chiếu quan trọng cho spawn
        if (isSpawnInteraction)
        {
            if (string.IsNullOrEmpty(spawnActionId))
            {
                Debug.LogWarning($"Door {gameObject.name}: isSpawnInteraction is true but spawnActionId is missing. State will not be saved.");
            }
            if (objectToSpawnPrefab == null || spawnPoint == null)
            {
                Debug.LogWarning($"Door {gameObject.name}: Missing objectToSpawnPrefab or spawnPoint for spawn interaction.");
            }
        }

        // Cần phải đảm bảo GameManager.instance tồn tại để thực hiện spawn/lưu trạng thái
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
                // Hiển thị gợi ý tương tác
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

    public void UnlockDoor()
    {
        isOpened = true;
        isRotating = true;

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        // === LOGIC SPAWN OBJECT MỚI ===
        if (isSpawnInteraction && objectToSpawnPrefab != null && spawnPoint != null && GameManager.instance != null)
        {
            // Chỉ spawn nếu hành động chưa được hoàn thành và ID đã được cung cấp
            if (!string.IsNullOrEmpty(spawnActionId) && !GameManager.instance.completedSpawnActions.Contains(spawnActionId))
            {
                // Tạo vật thể tại vị trí spawnPoint
                GameObject spawnedObject = Instantiate(objectToSpawnPrefab, spawnPoint.position, spawnPoint.rotation);

                // Giữ lại vật thể đã được sinh ra (sử dụng DontDestroyOnLoad theo mẫu tham chiếu)
                DontDestroyOnLoad(spawnedObject);

                // Đánh dấu hành động spawn đã hoàn thành
                GameManager.instance.completedSpawnActions.Add(spawnActionId);

                Debug.Log($"Door '{gameObject.name}' unlocked. Successfully spawned object '{objectToSpawnPrefab.name}' for action ID '{spawnActionId}'.");
            }
            else if (!string.IsNullOrEmpty(spawnActionId))
            {
                // Thông báo nếu hành động đã được hoàn thành trước đó
                Debug.Log($"Door {gameObject.name}: Object already spawned for action ID '{spawnActionId}'. Skipping spawn.");
            }
            else
            {
                // Nếu không có ID, ta vẫn spawn nhưng không lưu trạng thái (Chỉ dành cho debug/testing)
                Instantiate(objectToSpawnPrefab, spawnPoint.position, spawnPoint.rotation);
                Debug.LogWarning($"Door {gameObject.name}: SpawnActionId is missing, spawning without saving state.");
            }
        }
        // ===================================
    }

    public void EndInteraction()
    {
        if (interactionText != null)
        {
            interactionText.text = "";
        }
    }
}