using UnityEngine;
using TMPro;
using System.Collections;

public class FusePuzzleTrigger : MonoBehaviour
{
    [Header("External References (Required)")]
    [Tooltip("Cần thiết để script ElectricPanelCode không bị lỗi compile.")]
    public GameObject parentInteractableObject;

    [Header("Puzzle Setup")]
    public string puzzleId = "ControlRoomFusePuzzle";
    public DoorController targetDoor;
    public HexaPuzzleManager puzzleManager;

    [Header("Audio Setup")]
    public AudioSource worldAudioSource;
    public AudioClip electricHumClip;    // Tiếng rè điện khi chưa giải
    public AudioClip startPuzzleClip;    // Tiếng cạch khi mở màn hình giải đố
    public AudioClip solvedPowerOnClip;  // Tiếng điện chạy ổn định (Loop)
    public AudioClip doorOpenClip;       // Tiếng mở cửa

    [Header("UI & Interaction")]
    public string initialText = "The high voltage fuse is in place. Start the circuit puzzle? (Press E)";
    public string puzzleSolvedText = "Circuit complete. Control Room unlocked.";

    private TextMeshProUGUI interactionText;
    private bool isSolved = false;
    private bool playerInRange = false;

    void Awake()
    {
        // GIẢI PHÁP 1: Tự động tìm kiếm thành phần nếu bị Missing khi dịch chuyển
        if (worldAudioSource == null)
            worldAudioSource = GetComponent<AudioSource>();

        // Đảm bảo các thiết lập âm thanh 3D chuẩn
        if (worldAudioSource != null)
        {
            worldAudioSource.playOnAwake = false;
            worldAudioSource.spatialBlend = 1.0f; // Chế độ 3D
        }
    }

    void Start()
    {
        InitializeInteraction();
    }

    // Hàm khởi tạo/làm mới (Gọi lại hàm này nếu bạn dịch chuyển Object từ nơi khác đến)
    public void InitializeInteraction()
    {
        if (GameManager.instance != null)
            interactionText = GameManager.instance.interactionText;

        // Kiểm tra trạng thái lưu trữ
        if (GameManager.instance != null && GameManager.instance.completedPuzzles.Contains(puzzleId))
        {
            isSolved = true;
            if (targetDoor != null) targetDoor.UnlockDoor();
            PlayPowerOnLoop();
        }
    }

    void OnEnable()
    {
        // Khi Object được bật lên ở vị trí mới, đảm bảo mọi thứ sạch sẽ
        if (interactionText != null) interactionText.gameObject.SetActive(false);
        playerInRange = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isSolved)
        {
            playerInRange = true;

            // Âm thanh rè điện cảnh báo
            if (worldAudioSource != null && electricHumClip != null)
            {
                worldAudioSource.clip = electricHumClip;
                worldAudioSource.loop = true;
                worldAudioSource.Play();
            }

            if (interactionText != null)
            {
                interactionText.text = initialText;
                interactionText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Tắt âm thanh rè nếu đi xa
            if (!isSolved && worldAudioSource != null) worldAudioSource.Stop();

            if (interactionText != null) interactionText.gameObject.SetActive(false);

            // Tắt puzzle nếu đang giải mà bỏ chạy
            if (puzzleManager != null && HexaPuzzleManager.IsPuzzleActiveStatic)
                puzzleManager.DeactivatePuzzle(false);
        }
    }

    void Update()
    {
        // Kiểm tra điều kiện tương tác
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isSolved && !HexaPuzzleManager.IsPuzzleActiveStatic)
        {
            StartPuzzleSession();
        }
    }

    private void StartPuzzleSession()
    {
        // GIẢI PHÁP 2: Tắt ngay dòng chữ UI để không bị đè lên màn hình giải đố
        if (interactionText != null) interactionText.gameObject.SetActive(false);

        // Phát âm thanh bắt đầu
        if (worldAudioSource != null && startPuzzleClip != null)
            worldAudioSource.PlayOneShot(startPuzzleClip);

        if (puzzleManager != null)
        {
            puzzleManager.SetCompletionCallback(this);
            puzzleManager.ActivatePuzzle();
        }
    }

    public void SolvePuzzle()
    {
        if (isSolved) return;
        isSolved = true;

        // 1. Xử lý Cửa
        if (targetDoor != null)
        {
            targetDoor.UnlockDoor();
            if (worldAudioSource != null && doorOpenClip != null)
                worldAudioSource.PlayOneShot(doorOpenClip);
        }

        // 2. Lưu tiến trình
        if (GameManager.instance != null && !GameManager.instance.completedPuzzles.Contains(puzzleId))
            GameManager.instance.completedPuzzles.Add(puzzleId);

        // 3. Âm thanh môi trường: Chuyển sang điện chạy ổn định
        PlayPowerOnLoop();

        // Cập nhật UI kết quả
        if (playerInRange && interactionText != null)
        {
            interactionText.text = puzzleSolvedText;
            interactionText.gameObject.SetActive(true);
        }
    }

    private void PlayPowerOnLoop()
    {
        if (worldAudioSource != null && solvedPowerOnClip != null)
        {
            worldAudioSource.Stop();
            worldAudioSource.clip = solvedPowerOnClip;
            worldAudioSource.loop = true;
            worldAudioSource.Play();
        }
    }
}