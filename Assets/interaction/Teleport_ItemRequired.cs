using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// Script xử lý tương tác dịch chuyển yêu cầu vật phẩm.
public class InteractionManager_RequiredItemTeleport : MonoBehaviour
{
    // Cần có các tham chiếu tĩnh cho GameManager và InventoryManager để script này hoạt động.
    // Giả định:
    // 1. GameManager.instance tồn tại và có questionPanel, yesText, noText.
    // 2. InventoryManager.instance tồn tại và có phương thức HasItem(string id).

    private const string FADER_GROUP_NAME = "Panel"; // Hằng số tìm kiếm cho Fader

    [Header("Teleport Settings")]
    public string destinationSceneName; // Tên Scene đích
    // Biến này được giữ lại vì nó là tham số thiết lập trong Inspector, 
    // nhưng việc sử dụng nó đã được loại bỏ để fix lỗi biên dịch.
    public string destinationSpawnPointName; // Điểm xuất hiện trong Scene mới 

    [Header("Requirement Settings")]
    // ID của item mà người chơi cần có trong kho đồ để có thể dịch chuyển
    public string requiredItemId;
    // Thông báo hiển thị nếu thiếu item yêu cầu
    public string requirementFailureText = "You need a specific key or item to open this portal.";

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
    private bool isInteracting = false; // Trạng thái tương tác (UI đang bật và chờ input)

    // SỬ DỤNG TRẠNG THÁI TƯƠNG TÁC (Giúp quản lý luồng E-key)
    // 0: Idle (Bình thường), 1: Question Panel (Thành công - Chọn Yes/No), 
    // 5: Failure Message (Thất bại - Hiển thị thông báo, chờ E để kết thúc)
    private int interactionState = 0;

    private bool isSceneTransitionActive = false; // Trạng thái chuyển cảnh

    // Tham chiếu
    private Character_movement playerController;
    private Animator playerAnimator;
    private CanvasGroup faderCanvasGroup; // Tham chiếu đến Fader UI

    // Tham chiếu TextMeshProUGUI cục bộ 
    private TextMeshProUGUI questionText;
    private TextMeshProUGUI yesText;
    private TextMeshProUGUI noText;

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

        // 3. Lấy tham chiếu Question, Yes, No Text từ Panel
        if (GameManager.instance.questionPanel != null)
        {
            // Giả sử questionText là TMP_Text đầu tiên, và yesText/noText đã được gán sẵn trong GameManager (theo script gốc)
            questionText = GameManager.instance.questionPanel.GetComponentInChildren<TextMeshProUGUI>();
            yesText = GameManager.instance.yesText;
            noText = GameManager.instance.noText;
        }

        // 4. Gán nội dung TEXT mặc định (chỉ làm 1 lần)
        if (questionText != null)
        {
            // Để trống, sẽ được gán trong ShowQuestionPanel/ShowFailure
        }

        if (yesText != null)
        {
            yesText.text = myYesText;
        }
        if (noText != null)
        {
            noText.text = myNoText;
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

            // GỢI Ý TƯƠNG TÁC (Optional: có thể thêm code hiển thị lời nhắc "Press E" ở đây)
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
        // YÊU CẦU MỚI: Chỉ tương tác khi nhân vật ở trong vùng và bấm E
        if (playerInRange && !isSceneTransitionActive && Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteracting)
            {
                StartInteraction();
            }
            else if (interactionState == 5)
            {
                // Nếu đang hiển thị thông báo lỗi, nhấn E lần nữa để kết thúc
                EndInteraction();
            }
        }

        // Xử lý Input khi UI đang bật và là Question Panel (interactionState == 1)
        if (isInteracting && interactionState == 1 && !isSceneTransitionActive)
        {
            HandleUIInput();
        }
    }

    void StartInteraction()
    {
        // Kiểm tra xem đã có tham chiếu UI chưa
        if (questionText == null)
        {
            Debug.LogError("UI Text components are not set up yet. Interaction aborted.");
            return;
        }

        // 1. Kiểm tra yêu cầu vật phẩm
        bool hasRequiredItem = true;
        bool isRequired = !string.IsNullOrEmpty(requiredItemId);

        if (isRequired)
        {
            // Giả sử InventoryManager.instance và HasItem() tồn tại
            if (InventoryManager.instance == null)
            {
                Debug.LogError("InventoryManager.instance is null! Cannot check item requirement.");
                hasRequiredItem = false; // Giả định thất bại nếu không tìm thấy Inventory Manager
            }
            else
            {
                // Sử dụng 'instance' thay vì 'Instance' để phù hợp với quy ước hiện tại của GameManager/InventoryManager
                // Giả định InventoryManager cũng sử dụng 'instance' viết thường.
                hasRequiredItem = InventoryManager.instance.HasItem(requiredItemId);
            }
        }

        // 2. Xử lý kết quả kiểm tra
        if (hasRequiredItem)
        {
            ShowQuestionPanel();
        }
        else
        {
            ShowFailureMessage();
        }

        // Khóa chuyển động nhân vật chung
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


    void ShowQuestionPanel()
    {
        isInteracting = true;
        interactionState = 1; // Trạng thái Question Panel

        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(true);

            // Hiển thị câu hỏi
            if (questionText != null) questionText.text = myQuestion;

            // Đảm bảo các nút Yes/No được hiển thị và có text
            if (yesText != null)
            {
                yesText.gameObject.SetActive(true);
                yesText.text = myYesText;
            }
            if (noText != null)
            {
                noText.gameObject.SetActive(true);
                noText.text = myNoText;
            }

            selectedOption = 0; // Mặc định chọn Yes
            UpdateSelectionUI();
        }
    }

    void ShowFailureMessage()
    {
        isInteracting = true;
        interactionState = 5; // Trạng thái Thất bại

        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(true);

            // Hiển thị thông báo thất bại bằng questionText
            if (questionText != null) questionText.text = requirementFailureText + " (Press E to close)";

            // Ẩn các tùy chọn Yes/No (Giả sử có thể tắt GameObject chứa text)
            // Nếu không thể truy cập GameObject, chỉ cần làm mờ hoặc xóa text
            if (yesText != null) yesText.gameObject.SetActive(false);
            if (noText != null) noText.gameObject.SetActive(false);
        }
    }

    void HideUI()
    {
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(false);
        }

        // Đảm bảo Yes/No được reset trạng thái Active nếu chúng bị ẩn trong ShowFailureMessage
        if (yesText != null) yesText.gameObject.SetActive(true);
        if (noText != null) noText.gameObject.SetActive(true);
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

        // Xác nhận lựa chọn bằng phím Enter (Chỉ hoạt động ở trạng thái Question Panel)
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
        interactionState = 0; // Đặt lại trạng thái

        // Tắt question panel và reset Yes/No text active
        HideUI();

        // Mở khóa chuyển động nhân vật
        if (playerController != null && !isSceneTransitionActive)
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