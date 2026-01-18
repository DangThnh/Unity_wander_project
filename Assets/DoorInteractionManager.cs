using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorInteractionManager : MonoBehaviour
{
    // Cài đặt chuyển phòng
    [Header("Scene Transition Settings")]
    public string destinationSceneName;

    // Điểm xuất hiện trong scene mới (Sử dụng tên chuỗi để GameManager xử lý)
    [Tooltip("Tên điểm spawn trong Scene đích (phải khớp với tên điểm trong SpawnPointManager).")]
    public string destinationSpawnPointName;

    [Header("UI & State Settings")]
    public string myQuestion = "Do you want to step through the door?";
    public string myYesText = "Yes";
    public string myNoText = "No";

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false; // Trạng thái tương tác (UI đang hiển thị)

    // Tham chiếu đến script điều khiển nhân vật
    private Character_movement playerController;
    private Animator playerAnimator;

    // Tham chiếu UI cục bộ (sẽ được lấy từ GameManager)
    private TextMeshProUGUI questionText;
    private GameObject questionPanel;

    // Thiết lập kích thước font chữ
    private float defaultFontSize = 36f;
    private float selectedFontSize = 48f;

    void Start()
    {
        // Khởi tạo các tham chiếu UI từ GameManager khi Start
        if (GameManager.instance != null)
        {
            questionPanel = GameManager.instance.questionPanel;

            // Cần tìm Question Text trong Panel (giống logic trong InteractionManager_RequiredItemTeleport)
            if (questionPanel != null)
            {
                questionText = questionPanel.GetComponentInChildren<TextMeshProUGUI>();
                questionPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("[DoorInteraction] GameManager.instance is null!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Lấy tham chiếu đến script điều khiển nhân vật
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Tắt UI nếu nhân vật rời đi
            EndInteraction();
        }
    }

    void Update()
    {
        // Bắt đầu tương tác khi nhân vật ở trong vùng và bấm E
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            ShowUI();
        }

        // Xử lý Input khi UI đang bật
        if (isInteracting)
        {
            HandleUIInput();
        }
    }

    void ShowUI()
    {
        if (questionPanel == null || GameManager.instance == null) return;

        isInteracting = true;
        questionPanel.SetActive(true);
        selectedOption = 0; // Mặc định chọn Yes

        // Thiết lập nội dung text
        if (questionText != null) questionText.text = myQuestion;
        if (GameManager.instance.yesText != null)
        {
            GameManager.instance.yesText.text = myYesText;
            GameManager.instance.yesText.gameObject.SetActive(true);
        }
        if (GameManager.instance.noText != null)
        {
            GameManager.instance.noText.text = myNoText;
            GameManager.instance.noText.gameObject.SetActive(true);
        }

        UpdateSelectionUI();

        // Khóa chuyển động của nhân vật
        if (playerController != null)
        {
            playerController.canMove = false;
        }
        if (playerAnimator != null)
        {
            // Dừng mọi animation bằng cách đặt biến trạng thái di chuyển thành false
            playerAnimator.SetBool("IsMoving", false);
        }
    }

    void HideUI()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        // Mở khóa chuyển động của nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }

    void EndInteraction()
    {
        isInteracting = false;
        HideUI();
    }

    void UpdateSelectionUI()
    {
        // Sử dụng tham chiếu từ GameManager
        if (GameManager.instance != null)
        {
            if (GameManager.instance.yesText != null)
            {
                GameManager.instance.yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
            }
            if (GameManager.instance.noText != null)
            {
                GameManager.instance.noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
            }
        }
    }

    void HandleUIInput()
    {
        // Chuyển đổi lựa chọn bằng phím mũi tên trái/phải
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedOption = (selectedOption + 1) % 2; // Đảo ngược lựa chọn
            UpdateSelectionUI();
        }

        // Xác nhận lựa chọn bằng phím Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (selectedOption == 0) // Chọn Yes: Chuyển Scene
            {
                // BẮT ĐẦU COROUTINE FADE VÀ LOAD SCENE
                StartCoroutine(FadeAndLoadScene());
            }
            else // Chọn No: Thoát tương tác
            {
                EndInteraction();
            }
        }
    }

    // Thêm Coroutine Fade và Load Scene để đồng bộ với script kia
    private IEnumerator FadeAndLoadScene()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager is null. Cannot proceed with scene load.");
            yield break;
        }

        EndInteraction(); // Ẩn UI và mở khóa di chuyển

        // KHÔNG CÓ FADER TRONG SCRIPT NÀY, NÊN TẢI NGAY LẬP TỨC
        // Nếu muốn hiệu ứng fade, bạn cần phải tìm/gọi Fader UI giống như InteractionManager_RequiredItemTeleport.

        // --- Logic Load Scene mới và Spawn Point ---

        // 1. Thiết lập Spawn Point mong muốn (SỬA LỖI Ở ĐÂY)
        // Thay thế: GameManager.instance.spawnPosition = destinationSpawnPoint.position;
        // Bằng:
        GameManager.instance.SetNextSpawnPoint(destinationSpawnPointName);

        Debug.Log($"[DoorInteraction] Đặt SpawnPointName: {destinationSpawnPointName}. Tải Scene: {destinationSceneName}");

        // 2. Tải Scene mới
        SceneManager.LoadScene(destinationSceneName);

        // Script này kết thúc sau khi Scene được tải. GameManager sẽ tiếp quản.
    }
}