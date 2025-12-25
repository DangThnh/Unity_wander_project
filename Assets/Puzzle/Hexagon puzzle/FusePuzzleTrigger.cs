using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Cần thiết cho GameManager.instance.completedPuzzles

/// <summary>
/// Quản lý khu vực trigger để kích hoạt câu đố FUSE HEXA, lưu trạng thái hoàn thành và gọi mở cửa.
/// Đã khôi phục lại tham chiếu 'parentInteractableObject' để fix lỗi compile do script ngoài (ElectricPanelCode.cs) đang sử dụng.
/// </summary>
public class FusePuzzleTrigger : MonoBehaviour
{
    // === Cài đặt Puzzle và Door ===
    [Header("Puzzle Setup")]
    [Tooltip("ID duy nhất để lưu trạng thái hoàn thành trong GameManager.")]
    public string puzzleId = "ControlRoomFusePuzzle";

    [Tooltip("Tham chiếu đến script DoorController của cánh cửa cần mở/mở khóa.")]
    public DoorController targetDoor;

    // KHÔI PHỤC TRƯỜNG NÀY: Được yêu cầu bởi script ElectricPanelCode bên ngoài để tham chiếu đến đối tượng cũ.
    // Logic sử dụng trường này bên trong FusePuzzleTrigger.cs đã được loại bỏ.
    [Tooltip("Tham chiếu đến Game Object chứa InteractableObject cũ (Bảng điện hỏng).")]
    public GameObject parentInteractableObject;

    [Header("Mini-Game Manager")]
    [Tooltip("Kéo thả HexaPuzzleManager ở đây.")]
    public HexaPuzzleManager puzzleManager;

    // === Cài đặt UI và Text ===
    private TextMeshProUGUI interactionText;
    public string initialText = "The high voltage fuse is in place. Start the circuit puzzle? (Press E)";
    public string puzzleSolvedText = "Circuit complete. Control Room unlocked.";

    // === Trạng thái nội bộ ===
    private bool isSolved = false;
    private bool playerInRange = false;

    void Start()
    {
        // Lấy tham chiếu InteractionText từ GameManager
        if (GameManager.instance != null)
        {
            interactionText = GameManager.instance.interactionText;
        }
        else
        {
            Debug.LogError("GameManager.instance not found! Interaction UI will not work.");
        }

        // 1. KIỂM TRA TRẠNG THÁI GIẢI ĐỐ (đã giải chưa)
        if (GameManager.instance != null && GameManager.instance.completedPuzzles != null && GameManager.instance.completedPuzzles.Contains(puzzleId))
        {
            isSolved = true;
            if (targetDoor != null)
            {
                // Mở cửa ngay khi game load nếu puzzle đã được giải
                targetDoor.UnlockDoor();
                Debug.Log($"Puzzle {puzzleId} đã giải trước đó. Cửa đã mở khóa.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Chỉ hiển thị text của trigger này nếu puzzle chưa được giải
            if (interactionText != null)
            {
                interactionText.text = isSolved ? puzzleSolvedText : initialText;
                interactionText.gameObject.SetActive(true);
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
                // Luôn tắt text khi player rời khỏi trigger
                interactionText.gameObject.SetActive(false);
            }

            // Đảm bảo mini-game bị đóng nếu player rời trigger
            if (puzzleManager != null && HexaPuzzleManager.IsPuzzleActiveStatic)
            {
                // Tắt puzzle nhưng KHÔNG đánh dấu là solved (vì thoát giữa chừng)
                puzzleManager.DeactivatePuzzle(false);
            }
        }
    }

    void Update()
    {
        // Chỉ cho phép kích hoạt nếu playerInRange, bấm E, chưa giải và puzzle chưa active.
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isSolved && !HexaPuzzleManager.IsPuzzleActiveStatic)
        {
            // BẮT ĐẦU MINI-GAME GIẢI ĐỐ

            // Tắt text tương tác ngay lập tức
            if (interactionText != null) interactionText.gameObject.SetActive(false);

            // Kích hoạt Puzzle Manager
            if (puzzleManager != null)
            {
                Debug.Log("Activating Hexa Circuit Puzzle.");
                puzzleManager.SetCompletionCallback(this);
                puzzleManager.ActivatePuzzle();

            }
            else
            {
                Debug.LogError("HexaPuzzleManager is not assigned to the Trigger! Simulating completion.");
                // Nếu không có manager, giả lập hoàn thành để không bị kẹt
                StartCoroutine(SimulatePuzzleCompletion(3.0f));
            }
        }
    }

    private IEnumerator SimulatePuzzleCompletion(float delay)
    {
        yield return new WaitForSeconds(delay);
        SolvePuzzle();
    }

    /// <summary>
    /// Hàm được gọi từ HexaPuzzleManager khi người chơi hoàn thành Mini-Game.
    /// Trigger sẽ mở cửa và lưu trạng thái.
    /// </summary>
    public void SolvePuzzle()
    {
        if (isSolved) return;

        isSolved = true;

        // 1. Mở khóa cửa
        if (targetDoor != null)
        {
            targetDoor.UnlockDoor();
            Debug.Log("Cửa Control Room đã được mở khóa!");
        }
        else
        {
            Debug.LogError("Cửa mục tiêu (targetDoor) chưa được gán!");
        }

        // 2. Lưu trạng thái vào GameManager
        if (GameManager.instance != null && GameManager.instance.completedPuzzles != null && !GameManager.instance.completedPuzzles.Contains(puzzleId))
        {
            GameManager.instance.completedPuzzles.Add(puzzleId);
            Debug.Log($"Puzzle ID: {puzzleId} saved to GameManager.");
        }

        // 3. Cập nhật UI (nếu player vẫn trong phạm vi)
        if (playerInRange && interactionText != null)
        {
            // Đảm bảo text hiện ra với thông báo đã giải đố
            interactionText.gameObject.SetActive(true);
            interactionText.text = puzzleSolvedText;
        }

        // PuzzleManager đã tự DeactivatePuzzle(true) sau khi gọi hàm này.
    }
}