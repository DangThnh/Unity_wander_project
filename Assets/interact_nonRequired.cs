using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class InteractionManager_NonRequiredButton : MonoBehaviour
{
    private const string FADER_GROUP_NAME = "Panel"; // Hằng số tìm kiếm cho Fader

    [Header("Teleport Settings")]
    public string destinationSceneName; // Tên Scene đích
    // Biến này được giữ lại để thiết lập trong Inspector, nhưng không còn được sử dụng
    // để gán cho GameManager nữa.
    public string destinationSpawnPointName; // Điểm xuất hiện trong Scene mới 

    [Header("Fade Settings")]
    public float fadeSpeed = 1.0f; // Tốc độ mờ màn hình (Fade In/Out) tính bằng giây
    public float blackScreenDuration = 0.5f; // Thời gian dừng màn hình đen

    [Header("UI Settings")]
    public string myQuestion = "Do you want to step across?";
    public string myYesText = "Yes";
    public string myNoText = "No";

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false; // Trạng thái tương tác

    private bool isSceneTransitionActive = false; // Trạng thái chuyển cảnh

    // Tham chiếu
    private Character_movement playerController;
    private Animator playerAnimator;
    private CanvasGroup faderCanvasGroup; // Tham chiếu đến Fader UI

    // Tham chiếu TextMeshProUGUI cục bộ 
    private TextMeshProUGUI questionText;

    // Thiết lập kích thước font chữ
    private float defaultFontSize = 36f;
    private float selectedFontSize = 48f;

    void Start()
    {
        // Bắt đầu coroutine để tìm Fader và gán text
        StartCoroutine(SetupTextAndFader());
    }

    // Coroutine riêng để thiết lập các tham chiếu UI tĩnh, chỉ chạy một lần
    private IEnumerator SetupTextAndFader()
    {
        // 1. Chờ GameManager và Question Panel
        while (GameManager.instance == null || GameManager.instance.questionPanel == null)
        {
            yield return null;
        }

        // 2. Lấy tham chiếu Fader
        if (faderCanvasGroup == null)
        {
            GameObject faderObj = GameObject.Find(FADER_GROUP_NAME);
            if (faderObj != null)
            {
                faderCanvasGroup = faderObj.GetComponent<CanvasGroup>();
            }
            else
            {
                Debug.LogWarning($"[InteractionManager] Warning: Could not find '{FADER_GROUP_NAME}'. Hiệu ứng chuyển cảnh mờ dần sẽ bị bỏ qua.");
            }
        }

        // 3. Lấy tham chiếu Question Text từ Panel
        if (GameManager.instance.questionPanel != null)
        {
            // Tìm TextMeshProUGUI đầu tiên trong Question Panel
            questionText = GameManager.instance.questionPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        // 4. Gán nội dung TEXT (chỉ làm 1 lần)
        if (questionText != null)
        {
            questionText.text = myQuestion;
        }

        if (GameManager.instance.yesText != null)
        {
            GameManager.instance.yesText.text = myYesText;
        }
        if (GameManager.instance.noText != null)
        {
            GameManager.instance.noText.text = myNoText;
        }

        // Ẩn UI khi bắt đầu
        HideUI();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Lấy tham chiếu đến script điều khiển nhân vật
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();

            // Bắt đầu coroutine để đợi UI sẵn sàng trước khi hiển thị
            StartCoroutine(WaitForUIReadyAndShow());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Chỉ kết thúc tương tác nếu không đang trong quá trình chuyển cảnh
            if (!isSceneTransitionActive)
            {
                EndInteraction();
            }
        }
    }

    void Update()
    {
        // Hiển thị UI ngay lập tức khi nhân vật ở trong vùng và không có tương tác/fade nào đang diễn ra
        if (playerInRange && !isInteracting && !isSceneTransitionActive)
        {
            ShowUI();
        }

        // Xử lý khi UI đang bật
        if (isInteracting && !isSceneTransitionActive)
        {
            HandleUIInput();
        }
    }

    // Coroutine để đợi UI được gán tham chiếu an toàn trước khi hiển thị
    private IEnumerator WaitForUIReadyAndShow()
    {
        // Chờ cho đến khi GameManager và Question Panel sẵn sàng và questionText đã được gán
        while (GameManager.instance == null || GameManager.instance.questionPanel == null || questionText == null)
        {
            yield return null;
        }

        // Hiển thị UI nếu chưa tương tác và người chơi vẫn trong vùng
        if (!isInteracting && playerInRange)
        {
            ShowUI();
        }
    }

    void ShowUI()
    {
        isInteracting = true;

        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(true);
            selectedOption = 0; // Mặc định chọn Yes
            UpdateSelectionUI();

            // Khóa chuyển động của nhân vật
            if (playerController != null)
            {
                playerController.canMove = false;
            }
            if (playerAnimator != null)
            {
                // Dừng animation di chuyển
                playerAnimator.SetBool("IsMoving", false);
            }
        }
    }

    void HideUI()
    {
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(false);
        }

        // Mở khóa chuyển động của nhân vật (chỉ khi không đang Fade)
        if (!isSceneTransitionActive && playerController != null)
        {
            playerController.canMove = true;
        }
    }

    void UpdateSelectionUI()
    {
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
            if (selectedOption == 0) // Kiểm tra nếu chọn Yes (Tải Scene)
            {
                // Bắt đầu trình tự Fade và Load Scene
                if (!isSceneTransitionActive)
                {
                    StartCoroutine(FadeAndLoadScene());
                }
            }
            else // Nếu chọn No (Thoát tương tác)
            {
                EndInteraction();
            }
        }
    }

    void EndInteraction()
    {
        isInteracting = false;

        // Tắt question panel
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(false);
        }

        // Mở khóa chuyển động nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }

    // Coroutine xử lý quá trình làm tối màn hình và tải Scene mới
    private IEnumerator FadeAndLoadScene()
    {
        if (faderCanvasGroup == null)
        {
            Debug.LogWarning("Fader Canvas Group is null. Loading scene instantly.");
            LoadNewScene();
            yield break;
        }

        isSceneTransitionActive = true;
        HideUI(); // Ẩn Question Panel và đã khóa chuyển động nhân vật trước đó

        faderCanvasGroup.blocksRaycasts = true; // Chặn tương tác

        // 1. Fade Out (Mờ dần vào đen)
        while (faderCanvasGroup.alpha < 1)
        {
            faderCanvasGroup.alpha += Time.deltaTime / fadeSpeed;
            yield return null;
        }
        faderCanvasGroup.alpha = 1; // Đảm bảo đen hoàn toàn

        // 2. Chờ màn hình tối hoàn toàn
        yield return new WaitForSeconds(blackScreenDuration);

        // 3. Tải Scene mới
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        if (GameManager.instance != null)
        {
            // Đã loại bỏ hai dòng gây lỗi:
            // GameManager.instance.desiredSpawnPointName = destinationSpawnPointName;
            // GameManager.instance.isFirstLoad = false;

            // Chỉ thực hiện tải Scene đích
            SceneManager.LoadScene(destinationSceneName);
        }
    }
}