using UnityEngine;
using TMPro;
using System.Collections; // Cần thiết cho Coroutine

/// <summary>
/// Quản lý logic tương tác, xoay, và trạng thái khóa/mở khóa của cửa.
/// Cửa xoay bằng cách thay đổi localRotation của hingePoint.
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Door Configuration")]
    [Tooltip("Vật thể con là điểm xoay (HingePoint)")]
    public Transform hingePoint;
    [Tooltip("Góc cửa sẽ mở (ví dụ: 90f hoặc -90f)")]
    public float openAngle = 90f;
    [Tooltip("Tốc độ xoay của cửa")]
    public float rotationSpeed = 2.0f;

    [Header("Visuals and Status")]
    [Tooltip("Mô hình đèn báo trạng thái (Ví dụ: một quả cầu nhỏ)")]
    public GameObject statusLight;

    // BIẾN CHO TƯƠNG TÁC (Có thể chỉnh sửa trong Inspector)
    [Header("Interaction Messages")]
    [Tooltip("Nội dung text hiển thị khi cửa bị KHÓA (chỉ hiện khi nhấn E).")]
    public string lockedInteractionMessage = "Door is Locked! Find the key/fuse.";
    [Tooltip("Thời gian (giây) thông báo khóa sẽ hiển thị.")]
    public float lockedMessageDuration = 2.0f;
    [Tooltip("Nội dung text hiển thị khi cửa đã MỞ KHÓA và đóng.")]
    public string unlockedInteractionMessage = "";
    [Tooltip("Nội dung text hiển thị khi cửa đã MỞ KHÓA và đang mở.")]
    public string closeInteractionMessage = "";


    [Header("State")]
    public bool isLocked = true;
    private bool isOpened = false;
    private bool isRotating = false;
    private BoxCollider doorCollider;
    private bool playerInRange = false;

    // Coroutine để quản lý thông báo tạm thời
    private Coroutine messageCoroutine;

    // Tham chiếu đến UI Text (từ GameManager)
    private TextMeshProUGUI interactionText;

    void Awake()
    {
        doorCollider = GetComponent<BoxCollider>();
        if (doorCollider == null)
        {
            Debug.LogError("BoxCollider not found on the door controller object!");
        }
    }

    void Start()
    {
        if (GameManager.instance != null)
        {
            interactionText = GameManager.instance.interactionText;
        }
        else
        {
            Debug.LogWarning("GameManager instance not found. Interaction text will not be displayed.");
        }

        // Thiết lập trạng thái đèn ban đầu (Đỏ là khóa, Xanh là mở khóa)
        UpdateStatusLight(isLocked);
    }

    void Update()
    {
        // 1. Logic xoay cửa
        if (isRotating)
        {
            float targetAngle = isOpened ? openAngle : 0f;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            hingePoint.localRotation = Quaternion.Slerp(hingePoint.localRotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(hingePoint.localRotation, targetRotation) < 0.1f)
            {
                isRotating = false;
                hingePoint.localRotation = targetRotation;

                if (isOpened && doorCollider != null)
                {
                    doorCollider.enabled = false;
                }
                else if (!isOpened && doorCollider != null)
                {
                    doorCollider.enabled = true;
                }

                // Cập nhật text sau khi dừng xoay
                UpdateInteractionText();
            }
        }

        // 2. Logic tương tác mở/đóng cửa
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isRotating)
        {
            if (!isLocked)
            {
                // Cửa mở khóa: Mở/Đóng cửa
                ToggleDoor();
            }
            else // isLocked == true
            {
                // Cửa bị khóa: Chỉ hiển thị thông báo khóa tạm thời
                Debug.Log("Door is locked by the main circuit system.");
                DisplayLockedMessage();
                // TODO: Thêm âm thanh khóa điện tử
            }
        }
    }

    /// <summary>
    /// Hiển thị thông báo cửa bị khóa trong một khoảng thời gian ngắn.
    /// </summary>
    private void DisplayLockedMessage()
    {
        if (interactionText == null) return;

        // Dừng coroutine cũ nếu đang chạy để tránh xung đột
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        // Hiện thông báo khóa ngay lập tức
        interactionText.text = lockedInteractionMessage;
        interactionText.gameObject.SetActive(true);

        // Khởi chạy coroutine để xóa thông báo sau lockedMessageDuration
        messageCoroutine = StartCoroutine(ClearLockedMessageAfterDelay(lockedMessageDuration));
    }

    /// <summary>
    /// Coroutine để xóa thông báo bị khóa sau một khoảng thời gian.
    /// </summary>
    private IEnumerator ClearLockedMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Chỉ xóa text nếu nó VẪN là thông báo khóa.
        if (interactionText != null && interactionText.text == lockedInteractionMessage)
        {
            interactionText.text = "";
        }
        messageCoroutine = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Chỉ cập nhật prompt mở/đóng. Nếu bị khóa, text sẽ là ""
            UpdateInteractionText();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionText != null)
            {
                // Tắt text tương tác và dừng coroutine nếu đang chạy
                interactionText.text = "";
                if (messageCoroutine != null)
                {
                    StopCoroutine(messageCoroutine);
                    messageCoroutine = null;
                }
            }
        }
    }

    /// <summary>
    /// Hàm công khai được gọi từ FusePuzzleTrigger khi giải đố thành công.
    /// </summary>
    public void UnlockDoor()
    {
        if (!isLocked) return;

        isLocked = false;
        UpdateStatusLight(false); // Đèn chuyển xanh
        Debug.Log("Control Room Door is now UNLOCKED.");

        // Dọn dẹp coroutine khóa (nếu đang hiển thị)
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        // TỰ ĐỘNG MỞ CỬA NGAY LẬP TỨC (nếu nó chưa mở)
        if (!isOpened)
        {
            ToggleDoor();
        }

        // Cập nhật text tương tác (lúc này sẽ hiện prompt mở/đóng)
        UpdateInteractionText();

        // TODO: SoundManager.PlaySound("door_unlock_success");
    }

    /// <summary>
    /// Mở hoặc đóng cửa.
    /// </summary>
    private void ToggleDoor()
    {
        if (isRotating) return;

        isOpened = !isOpened;
        isRotating = true;

        if (doorCollider != null)
        {
            // Tắt collider ngay lập tức khi bắt đầu mở
            doorCollider.enabled = !isOpened;
        }

        // Text sẽ được cập nhật sau khi xoay xong trong Update()
    }

    /// <summary>
    /// Cập nhật text tương tác hiển thị cho người chơi.
    /// </summary>
    private void UpdateInteractionText()
    {
        if (interactionText == null || !playerInRange) return;

        if (isLocked)
        {
            // YÊU CẦU MỚI: Cửa khóa sẽ KHÔNG hiện text cho đến khi bấm 'E'
            interactionText.text = "";
        }
        else if (isOpened)
        {
            interactionText.text = closeInteractionMessage;
        }
        else // isOpened == false, isLocked == false
        {
            interactionText.text = unlockedInteractionMessage;
        }

        // Đảm bảo GameObject chứa text được bật lên nếu nó bị script khác tắt.
        if (interactionText.gameObject.activeSelf == false)
        {
            interactionText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái đèn báo (Đỏ/Xanh).
    /// </summary>
    private void UpdateStatusLight(bool isCurrentLocked)
    {
        if (statusLight == null) return;

        Renderer lightRenderer = statusLight.GetComponent<Renderer>();
        if (lightRenderer != null)
        {
            lightRenderer.material.color = isCurrentLocked ? Color.red : Color.green;
        }
    }
}