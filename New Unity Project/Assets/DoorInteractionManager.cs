using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DoorInteractionManager : MonoBehaviour
{
    // Cài đặt chuyển phòng
    public string destinationSceneName;

    // Cài đặt UI
    public GameObject questionPanel;
    public TextMeshProUGUI yesText;
    public TextMeshProUGUI noText;
    // Điểm xuất hiện trong scene mới
    public Transform destinationSpawnPoint;

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false; // Trạng thái tương tác

    // Tham chiếu đến script điều khiển nhân vật
    private Character_movement playerController;

    private Animator playerAnimator;

    // Thiết lập kích thước font chữ
    private float defaultFontSize = 36f;
    private float selectedFontSize = 48f;

    void Start()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
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
            // Tắt UI nếu nhân vật rời đi và không tương tác
            if (!isInteracting)
            {
                HideUI();
            }
        }
    }

    void Update()
    {
        // Bắt đầu tương tác khi nhân vật ở trong vùng và bấm E
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            ShowUI();
        }
        // Kết thúc tương tác khi UI đang bật và người chơi bấm E
        else if (isInteracting && Input.GetKeyDown(KeyCode.E))
        {
            // Thoát khỏi tương tác mà không cần chuyển scene
            if (selectedOption == 1) // Kiểm tra nếu chọn No
            {
                EndInteraction();
            }
        }

        // Xử lý khi UI đang bật
        if (isInteracting)
        {
            HandleUIInput();
        }
    }

    void ShowUI()
    {
        isInteracting = true;
        questionPanel.SetActive(true);
        selectedOption = 0; // Mặc định chọn Yes
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
        isInteracting = false;
        questionPanel.SetActive(false);

        // Mở khóa chuyển động của nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }

    void EndInteraction()
    {
        // Tắt UI và tiếp tục chơi
        HideUI();
    }

    void UpdateSelectionUI()
    {
        if (yesText != null)
        {
            yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
        }
        if (noText != null)
        {
            noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
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
            // Trong hàm HandleUIInput() của DoorInteractionManager
            if (selectedOption == 0)
            {
                if (GameManager.instance != null)
                {
                    // Lưu vị trí và hướng của cửa vào GameManager
                    GameManager.instance.spawnPosition = destinationSpawnPoint.position;
                    GameManager.instance.spawnRotation = destinationSpawnPoint.rotation;

                    // Đảm bảo biến isFirstLoad đã được đặt thành false
                    GameManager.instance.isFirstLoad = false;
                }
                SceneManager.LoadScene(destinationSceneName);
                HideUI();
            }
        }
    }
}