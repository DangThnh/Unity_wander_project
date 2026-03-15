using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Quản lý tương tác đơn giản theo chuỗi hội thoại và mở khóa cửa sau khi xác nhận, kèm theo âm thanh.
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

    // === Cài đặt Âm thanh ===
    [Header("Audio Settings")]
    [Tooltip("Âm thanh phát ra khi cửa được mở khóa thành công.")]
    public AudioClip unlockSound;

    [Tooltip("Âm thanh phát ra mỗi khi bấm E để chuyển dòng hội thoại (tùy chọn).")]
    public AudioClip dialogueNextSound;

    [Range(0, 1)] public float volume = 1.0f;

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
    private AudioSource audioSource;

    // === Trạng thái Nội bộ ===
    private bool isSolved = false;
    private bool playerInRange = false;
    private int dialogueIndex = 0; // 0: Ready, 1..N: Dialogue steps, N+1: Confirmation step

    void Awake()
    {
        // Khởi tạo AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // Âm thanh 3D
    }

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
            EndInteraction();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isSolved)
        {
            HandleInteraction();
        }
    }

    void HandleInteraction()
    {
        // Phát âm thanh chuyển dòng hội thoại (nếu có)
        if (dialogueIndex <= dialogueTexts.Count)
        {
            PlaySound(dialogueNextSound);
        }

        if (dialogueIndex == 0)
        {
            StartInteraction();
            return;
        }

        if (dialogueIndex < dialogueTexts.Count)
        {
            if (interactionText != null)
            {
                interactionText.text = dialogueTexts[dialogueIndex];
            }
            dialogueIndex++;
        }
        else if (dialogueIndex == dialogueTexts.Count)
        {
            if (interactionText != null)
            {
                interactionText.text = confirmationText;
            }
            dialogueIndex++;
        }
        else if (dialogueIndex == dialogueTexts.Count + 1)
        {
            UnlockDoorSequence();
        }
    }

    void StartInteraction()
    {
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        if (interactionText != null && dialogueTexts.Count > 0)
        {
            if (dialogueTexts.Count > 1)
            {
                interactionText.text = dialogueTexts[1];
                dialogueIndex = 2;
            }
            else
            {
                interactionText.text = confirmationText;
                dialogueIndex = dialogueTexts.Count + 1;
            }
        }
    }

    void UnlockDoorSequence()
    {
        if (isSolved) return;

        isSolved = true;
        dialogueIndex = dialogueTexts.Count + 2;

        // PHÁT ÂM THANH: Mở khóa thành công
        PlaySound(unlockSound);

        if (targetDoor != null)
        {
            targetDoor.UnlockDoor();
            Debug.Log("Door unlocked successfully!");
        }

        if (GameManager.instance != null && GameManager.instance.completedPuzzles != null && !GameManager.instance.completedPuzzles.Contains(puzzleId))
        {
            GameManager.instance.completedPuzzles.Add(puzzleId);
        }

        StartCoroutine(HandlePostSolveUIAndUnlock());
    }

    private System.Collections.IEnumerator HandlePostSolveUIAndUnlock()
    {
        if (playerInRange && interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = successText;
        }

        yield return new WaitForSeconds(postSolveDisplayTime);
        EndInteraction();
    }

    void EndInteraction()
    {
        dialogueIndex = 0;

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}