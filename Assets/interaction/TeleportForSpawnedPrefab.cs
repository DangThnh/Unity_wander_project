using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

// Lớp này dùng cho các Prefab được sinh ra trong Scene và kế thừa logic cơ sở
public class TeleportForSpawnedPrefab : TeleportInteractionBase
{
    private const string FADER_GROUP_NAME = "Panel"; // Hằng số tìm kiếm cho Fader

    // Trạng thái cục bộ (Dùng 'new' để giải quyết cảnh báo CS0108)
    private new bool isFading = false; // Đang trong quá trình Fade/Dịch chuyển

    // KHẮC PHỤC CS0103: Thêm biến trạng thái này, có lẽ nó bị thiếu hoặc không thể truy cập từ lớp cơ sở.
    // Đặt là 'protected' để cho phép lớp con truy cập, cần thiết cho logic WaitForGameManagerUIRegistration.
    protected bool isUIReady = false; // Cờ theo dõi việc gán các tham chiếu UI

    // --- Cần Ghi Đè Lại các hằng số (nếu lớp cha không có) ---
    private const float DEFAULT_FONT_SIZE = 24f;
    private const float SELECTED_FONT_SIZE = 36f;

    // Ghi đè phương thức Start() để sử dụng Coroutine và GameManager
    protected override void Start()
    {
        // Bắt đầu coroutine để gán các tham chiếu UI an toàn sau khi GameManager đã đăng ký
        StartCoroutine(WaitForGameManagerUIRegistration());

        // Loại bỏ logic khởi tạo liên quan đến Pre-Text
    }

    private IEnumerator WaitForGameManagerUIRegistration()
    {
        // Bước 1: Chờ cho đến khi GameManager tồn tại và CÁC tham chiếu UI quan trọng được gán.
        while (GameManager.instance == null || GameManager.instance.questionPanel == null || GameManager.instance.interactionText == null)
        {
            yield return null; // Đợi khung hình tiếp theo
        }

        // Bước 2: Gán các biến nội bộ (kế thừa) bằng các tham chiếu từ GameManager
        if (interactionText == null) interactionText = GameManager.instance.interactionText;
        if (questionPanel == null)
        {
            questionPanel = GameManager.instance.questionPanel;
            if (questionPanel != null) questionText = questionPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (yesText == null) yesText = GameManager.instance.yesText;
        if (noText == null) noText = GameManager.instance.noText;

        // Tìm Fader Canvas Group (dùng GameObject.Find vì nó không được GameManager quản lý)
        if (faderCanvasGroup == null)
        {
            GameObject faderObj = GameObject.Find(FADER_GROUP_NAME);
            if (faderObj != null)
            {
                faderCanvasGroup = faderObj.GetComponent<CanvasGroup>();
            }
            else
            {
                Debug.LogWarning($"[TeleportPrefab] Warning: Could not find '{FADER_GROUP_NAME}'. Hiệu ứng làm tối màn hình sẽ bị bỏ qua.");
            }
        }

        if (interactionText == null)
        {
            Debug.LogError("[TeleportPrefab] FATAL ERROR: interactionText is NULL. Kiểm tra SceneUIRegistrar.");
        }
        else
        {
            Debug.Log("[TeleportPrefab] Các tham chiếu UI đã được truy xuất và gán thành công.");
        }

        // Bước 3: Đánh dấu UI đã sẵn sàng
        isUIReady = true;

        // Bước 4: Thực hiện logic khởi tạo (thiết lập text và ẩn UI)
        if (questionText != null) questionText.text = myQuestion;
        if (yesText != null) yesText.text = myYesText;
        if (noText != null) noText.text = myNoText;

        base.HideUI(); // Ẩn Question Panel
    }

    // Ghi đè OnTriggerEnter để xử lý va chạm và HIỂN THỊ QUESTION PANEL NGAY LẬP TỨC
    private new void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Bắt đầu Coroutine để xử lý va chạm an toàn (chờ UI sẵn sàng)
            StartCoroutine(HandleSafeInteraction(other));
        }
    }

    private IEnumerator HandleSafeInteraction(Collider other)
    {
        // CHỜ UI SẴN SÀNG TRƯỚC KHI TIẾN HÀNH TƯƠNG TÁC
        while (!isUIReady)
        {
            yield return null;
        }

        // Gán tham chiếu Player
        playerInRange = true;
        playerController = other.GetComponent<Character_movement>();
        playerAnimator = other.GetComponent<Animator>();

        if (playerController == null)
        {
            Debug.LogError("[TeleportPrefab] Lỗi: Không tìm thấy Character_movement trên Player.");
            yield break;
        }

        // Ngăn tương tác lặp lại nếu đang trong quá trình chuyển cảnh
        if (isFading) yield break;

        // Bắt đầu luồng tương tác: Hiển thị Question Panel ngay lập tức
        ShowQuestionUI();
    }

    // Ghi đè OnTriggerExit để ẩn Panel
    private new void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Chỉ kết thúc tương tác nếu không đang chuyển cảnh
            if (!isFading)
            {
                EndInteraction();
            }
        }
    }

    // Ghi đè Update để chỉ xử lý Input Panel
    private new void Update()
    {
        // Chỉ xử lý Input ở trạng thái Question Panel
        if (isInteracting)
        {
            HandleQuestionInput();
        }
    }

    // Ghi đè ShowUI của lớp cha (Chỉ hiển thị Panel, KHÔNG KHÓA CHUYỂN ĐỘNG)
    private new void ShowUI()
    {
        ShowQuestionUI();
    }

    private void ShowQuestionUI()
    {
        isInteracting = true;

        if (questionPanel != null)
        {
            questionPanel.SetActive(true);
            selectedOption = 0;
            UpdateSelectionUI();
        }
        // Đã loại bỏ lệnh khóa chuyển động
    }

    // Sử dụng logic cập nhật UI của lớp cơ sở
    private new void UpdateSelectionUI()
    {
        if (yesText != null)
        {
            yesText.fontSize = (selectedOption == 0) ? SELECTED_FONT_SIZE : DEFAULT_FONT_SIZE;
        }
        if (noText != null)
        {
            noText.fontSize = (selectedOption == 1) ? SELECTED_FONT_SIZE : DEFAULT_FONT_SIZE;
        }
    }

    // Ghi đè HandleUIInput
    private void HandleQuestionInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedOption = (selectedOption + 1) % 2;
            UpdateSelectionUI();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (selectedOption == 0) // Chọn Có
            {
                // Bắt đầu trình tự Fade và Teleport (Intra-Scene)
                StartCoroutine(FadeAndTeleport());
            }
            else // Chọn Không
            {
                EndInteraction();
            }
        }
    }

    // Ghi đè EndInteraction
    private new void EndInteraction()
    {
        isInteracting = false;
        base.HideUI(); // Ẩn Question Panel (gọi của lớp cha)
    }

    // --- LOGIC FADE VÀ DỊCH CHUYỂN (INTRA-SCENE) ---
    // Thêm 'new' để ẩn phương thức của lớp cơ sở
    private new IEnumerator FadeAndTeleport()
    {
        // Kiểm tra an toàn trước khi chạy
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager không tồn tại. Không thể dịch chuyển.");
            yield break;
        }

        if (faderCanvasGroup == null)
        {
            Debug.LogError("Fader Canvas Group is not assigned. Teleporting instantly.");
            // 2. Dịch chuyển nhân vật qua GameManager
            GameManager.instance.TeleportPlayerInScene(destinationId);
            EndInteraction();
            yield break;
        }

        isFading = true;
        base.HideUI(); // Ẩn Question Panel

        // 1. Mờ dần vào (Fade Out - chuyển sang màu đen)
        faderCanvasGroup.blocksRaycasts = true; // Chặn tương tác
        // Đã loại bỏ biến 'timer' không sử dụng (Khắc phục CS0219)
        while (faderCanvasGroup.alpha < 1)
        {
            // Tối ưu hóa: sử dụng logic đơn giản hóa cho Fade (Time.deltaTime / fadeSpeed)
            faderCanvasGroup.alpha += Time.deltaTime / fadeSpeed;
            yield return null;
        }
        faderCanvasGroup.alpha = 1; // Đảm bảo đen hoàn toàn

        // 2. Dịch chuyển nhân vật (SỬ DỤNG PHƯƠNG THỨC AN TOÀN CỦA GAMEMANAGER)
        GameManager.instance.TeleportPlayerInScene(destinationId);

        // 3. Chờ thời gian màn hình tối hoàn toàn
        yield return new WaitForSeconds(blackScreenDuration);

        // 4. Mờ dần ra (Fade In - chuyển từ màu đen về trong suốt)
        while (faderCanvasGroup.alpha > 0)
        {
            faderCanvasGroup.alpha -= Time.deltaTime / fadeSpeed;
            yield return null;
        }
        faderCanvasGroup.alpha = 0; // Đảm bảo trong suốt hoàn toàn
        faderCanvasGroup.blocksRaycasts = false; // Cho phép tương tác lại

        isFading = false;

        // 5. Kết thúc tương tác
        EndInteraction();
    }

    // --- LOGIC CŨ ĐÃ BỊ LOẠI BỎ (Thay thế bằng GameManager.TeleportPlayerInScene) ---
    // Phương thức này đã bị loại bỏ vì giờ đây ta dùng GameManager.TeleportPlayerInScene
    // private new void TeleportPlayer() { ... } 
    // private new GameObject FindObjectWithId(string id) { ... }
}