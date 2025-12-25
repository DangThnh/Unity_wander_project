using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Quản lý tương tác đơn giản theo chuỗi hội thoại và mở khóa cửa sau khi xác nhận.
/// </summary>
public class SimpleDoorUnlockTrigger : MonoBehaviour
{
    // === Cài đặt Bắt buộc ===
    [Header("Door & State Settings")]
    [Tooltip("ID duy nhất để lưu trạng thái hoàn thành trong GameManager.")]
    public string puzzleId = "SimpleUnlockDoor";

    [Tooltip("Tham chiếu đến script DoorController của cánh cửa cần mở/mở khóa.")]
    public DoorController targetDoor;

    [Tooltip("Thời gian hiển thị thông báo cuối cùng (giây) trước khi tắt UI.")]
    public float postSolveDisplayTime = 3f;

    // === Cài đặt Dialogue ===
    [Header("Dialogue Sequence")]
    [Tooltip("Danh sách các dòng text hiển thị theo thứ tự (mỗi lần bấm E là một dòng).")]
    public List<string> dialogueTexts = new List<string>
    {
        "This panel seems to control the locking mechanism.",
        "The light is green, meaning manual override is possible."
    };

    [Tooltip("Text xác nhận cuối cùng trước khi mở cửa.")]
    public string confirmationText = "Do you want to manually unlock the door? (Press E to Confirm)";

    [Tooltip("Text hiển thị sau khi mở khóa thành công.")]
    public string successText = "Access granted. The door is now unlocked.";

    // === Tham chiếu Nội bộ ===
    private TextMeshProUGUI interactionText;
    private Character_movement playerController;

    // === Trạng thái Nội bộ ===
    private bool isSolved = false;
    private bool playerInRange = false;
    private int dialogueIndex = 0; // 0: Ready, 1..N: Dialogue steps, N+1: Confirmation step

    void Start()
    {
        // 1. Lấy tham chiếu InteractionText từ GameManager
        if (GameManager.instance != null)
        {
            interactionText = GameManager.instance.interactionText;
        }
        else
        {
            Debug.LogError("GameManager.instance not found! Interaction UI will not work.");
        }

        // 2. KIỂM TRA TRẠNG THÁI GIẢI ĐỐ (đã giải chưa)
        if (GameManager.instance != null && GameManager.instance.completedPuzzles != null && GameManager.instance.completedPuzzles.Contains(puzzleId))
        {
            isSolved = true;
            if (targetDoor != null)
            {
                // Mở cửa ngay khi game load nếu puzzle đã được giải
                targetDoor.UnlockDoor();
                Debug.Log($"Door {puzzleId} đã mở khóa trước đó.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<Character_movement>();

            // Chỉ hiển thị text đầu tiên/đã solved nếu chưa ở giữa tương tác
            if (interactionText != null)
            {
                interactionText.text = isSolved ? successText : dialogueTexts[0] + " (Press E)";
                interactionText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Đặt lại trạng thái khi người chơi rời đi
            EndInteraction();
        }
    }

    void Update()
    {
        // Chỉ tương tác nếu playerInRange và bấm E
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isSolved)
        {
            HandleInteraction();
        }
    }

    /// <summary>
    /// Xử lý logic chuyển đổi giữa các bước thoại và bước xác nhận.
    /// </summary>
    void HandleInteraction()
    {
        // BƯỚC 0: Khởi đầu (lần nhấn E đầu tiên)
        if (dialogueIndex == 0)
        {
            StartInteraction();
            return;
        }

        // BƯỚC 1: Hiển thị Dialogue
        if (dialogueIndex < dialogueTexts.Count)
        {
            // Tăng index để hiển thị dòng tiếp theo
            if (interactionText != null)
            {
                interactionText.text = dialogueTexts[dialogueIndex];
            }
            dialogueIndex++;
        }
        // BƯỚC 2: Chuyển sang Xác nhận (Sau khi hết dialogue)
        else if (dialogueIndex == dialogueTexts.Count)
        {
            if (interactionText != null)
            {
                interactionText.text = confirmationText;
            }
            // Tăng index lên trạng thái xác nhận
            dialogueIndex++;
        }
        // BƯỚC 3: Mở khóa Cửa (Sau khi xác nhận)
        else if (dialogueIndex == dialogueTexts.Count + 1)
        {
            UnlockDoorSequence();
        }
    }

    /// <summary>
    /// Bắt đầu tương tác: Khóa người chơi và hiển thị dòng text đầu tiên.
    /// </summary>
    void StartInteraction()
    {
        // 1. Khóa chuyển động người chơi
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        // 2. Hiển thị dòng text đầu tiên
        if (interactionText != null && dialogueTexts.Count > 0)
        {
            // Text đầu tiên đã được hiển thị trong OnTriggerEnter, đây là lần nhấn E thứ nhất
            // Chúng ta chỉ cần chuyển sang index 1 (dòng text thứ 2)
            if (dialogueTexts.Count > 1)
            {
                interactionText.text = dialogueTexts[1];
                dialogueIndex = 2; // Bắt đầu từ dòng thứ 2
            }
            else
            {
                // Nếu chỉ có 1 dòng text, chuyển thẳng sang xác nhận
                interactionText.text = confirmationText;
                dialogueIndex = dialogueTexts.Count + 1;
            }
        }
    }

    /// <summary>
    /// Mở khóa cửa, lưu trạng thái, và xử lý UI cuối cùng.
    /// </summary>
    void UnlockDoorSequence()
    {
        if (isSolved) return; // Tránh gọi lại

        isSolved = true;
        dialogueIndex = dialogueTexts.Count + 2; // Chuyển sang trạng thái thành công/kết thúc

        // 1. Mở khóa cửa
        if (targetDoor != null)
        {
            targetDoor.UnlockDoor();
            Debug.Log("Door unlocked successfully!");
        }
        else
        {
            Debug.LogError("Target Door is not assigned!");
        }

        // 2. Lưu trạng thái vào GameManager
        if (GameManager.instance != null && GameManager.instance.completedPuzzles != null && !GameManager.instance.completedPuzzles.Contains(puzzleId))
        {
            GameManager.instance.completedPuzzles.Add(puzzleId);
        }

        // 3. Xử lý UI cuối cùng và mở khóa người chơi thông qua Coroutine
        StartCoroutine(HandlePostSolveUIAndUnlock());
    }

    /// <summary>
    /// Coroutine xử lý UI và mở khóa nhân vật sau khi mở cửa thành công.
    /// </summary>
    private System.Collections.IEnumerator HandlePostSolveUIAndUnlock()
    {
        // 1. Hiển thị thông báo thành công
        if (playerInRange && interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = successText;
        }

        // 2. Chờ để người chơi thấy thông báo cuối cùng
        yield return new WaitForSeconds(postSolveDisplayTime);

        // 3. Kết thúc tương tác
        EndInteraction();
    }

    /// <summary>
    /// Kết thúc tương tác, mở khóa người chơi và tắt UI Text.
    /// </summary>
    void EndInteraction()
    {
        dialogueIndex = 0;

        // Tắt text UI
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        // Mở khóa chuyển động nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }
}