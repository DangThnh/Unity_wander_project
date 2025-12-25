using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro; // Sử dụng TextMeshPro cho UI hiện đại

/// <summary>
/// Script quản lý toàn bộ câu đố hình trụ Hexa. 
/// Nó chịu trách nhiệm cho input, hiển thị UI và gọi lại Trigger khi hoàn thành.
/// </summary>
public class HexaPuzzleManager : MonoBehaviour
{
    // --- STATIC STATE CHO EXTERNAL SCRIPTS ---
    public static bool IsPuzzleActiveStatic { get; private set; } = false;
    public static bool IsPuzzleSolvedStatic { get; private set; } = false;

    // --- SỰ KIỆN DECOUPLING QUAN TRỌNG ---
    // Các script Interactable Object khác có thể ĐĂNG KÝ để nhận thông báo này
    public static event System.Action OnPuzzleCompleted;

    // --- HẰNG SỐ TEXT ---
    private const string DEFAULT_INSTRUCTION = "Press E to rotate, arrow keys to toggle and Enter to confirm. Press Esc to stop.";
    private const string ERROR_MESSAGE = "Error! Wrong Passcode!";
    // Đã gộp thông báo: Manager chịu trách nhiệm hiển thị thông báo cuối cùng
    private const string SUCCESS_MESSAGE = "Correct! Access Granted. Control Room Unlocked.";

    // --- CÁC BIẾN CẦN GÁN TRONG UNITY INSPECTOR ---
    [Tooltip("Danh sách 4 hình trụ. Gán chúng theo thứ tự từ TRÁI sang PHẢI.")]
    public List<RotatableCylinder> cylinders;

    [Tooltip("Trình tự biểu tượng ĐÚNG (mỗi giá trị từ 0 đến 5).")]
    public int[] correctSequence = new int[4] { 1, 3, 5, 2 };

    [Header("Visual & Exit Control")]
    [Tooltip("GameObject cha chứa TẤT CẢ các thành phần của câu đố (Camera, UI, Cylinders).")]
    public GameObject puzzleRootGameObject;

    [Header("UI Feedback")]
    [Tooltip("Component TextMeshProUGUI để hiển thị thông báo.")]
    public TextMeshProUGUI messageTextComponent;

    [Tooltip("Thời gian hiển thị thông báo lỗi/thành công (giây)")]
    public float messageDisplayTime = 3f;

    // --- TRẠNG THÁI NỘI BỘ ---
    private int currentCylinderIndex = 0;
    private bool isPuzzleActive = false;
    private bool isPuzzleSolved = false;
    private Coroutine messageCoroutine;

    // --- BIẾN CALLBACK ---
    private FusePuzzleTrigger completionTrigger;

    void Start()
    {
        // Kiểm tra cơ bản
        if (cylinders == null || cylinders.Count != 4)
        {
            Debug.LogError("HexaPuzzleManager requires exactly 4 RotatableCylinder components assigned.");
        }

        if (puzzleRootGameObject != null)
        {
            puzzleRootGameObject.SetActive(false);
        }

        isPuzzleActive = false;
        IsPuzzleActiveStatic = false;
        isPuzzleSolved = false;
        IsPuzzleSolvedStatic = false;

        // Đảm bảo tất cả hình trụ đều được deselect
        if (cylinders != null)
        {
            foreach (var cyl in cylinders)
            {
                if (cyl != null) cyl.Deselect();
            }
        }
    }

    void Update()
    {
        // XỬ LÝ ESCAPE (THOÁT KHỎI PUZZLE NẾU CHƯA GIẢI XONG)
        if (isPuzzleActive && !isPuzzleSolved && Input.GetKeyDown(KeyCode.Escape))
        {
            DeactivatePuzzle(false);
            return;
        }

        if (!isPuzzleActive || isPuzzleSolved)
        {
            return;
        }

        HandleInput();
    }

    /// <summary>
    /// Gán tham chiếu đến trigger gọi puzzle, được gọi từ FusePuzzleTrigger.cs.
    /// </summary>
    public void SetCompletionCallback(FusePuzzleTrigger trigger)
    {
        this.completionTrigger = trigger;
        Debug.Log("Completion Callback set from FusePuzzleTrigger.");
    }

    // Kích hoạt giao diện câu đố
    public void ActivatePuzzle()
    {
        if (isPuzzleSolved || isPuzzleActive) return;

        isPuzzleActive = true;
        IsPuzzleActiveStatic = true;

        if (puzzleRootGameObject != null)
        {
            puzzleRootGameObject.SetActive(true);
        }

        SetMessageText(DEFAULT_INSTRUCTION);

        currentCylinderIndex = 0;
        if (cylinders != null && cylinders.Count > 0 && cylinders[currentCylinderIndex] != null)
        {
            cylinders[currentCylinderIndex].Select();
        }

        Debug.Log("Hexa Puzzle activated. Cylinder 1 selected.");
    }

    /// <summary>
    /// Vô hiệu hóa câu đố, ẩn UI và trả quyền điều khiển.
    /// </summary>
    /// <param name="solved">True nếu tắt sau khi giải đố thành công.</param>
    public void DeactivatePuzzle(bool solved)
    {
        if (!isPuzzleActive && !IsPuzzleActiveStatic)
        {
            return;
        }

        isPuzzleActive = false;
        IsPuzzleActiveStatic = false;

        if (solved)
        {
            isPuzzleSolved = true;
            IsPuzzleSolvedStatic = true;
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        if (puzzleRootGameObject != null)
        {
            puzzleRootGameObject.SetActive(false);
        }

        // Bỏ chọn hình trụ đang chọn
        if (cylinders != null && currentCylinderIndex >= 0 && currentCylinderIndex < cylinders.Count && cylinders[currentCylinderIndex] != null)
        {
            cylinders[currentCylinderIndex].Deselect();
        }

        Debug.Log("Hexa Puzzle deactivated. Solved: " + isPuzzleSolved);
    }

    // Xử lý khi giải đố thành công
    private void HandleCorrectSequence()
    {
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(SolveSequenceAndDeactivate());
    }

    /// <summary>
    /// Coroutine xử lý chuỗi hành động khi giải đố thành công (Hiển thị thông báo -> Gọi Trigger -> Tắt màn hình).
    /// </summary>
    private IEnumerator SolveSequenceAndDeactivate()
    {
        // 1. Hiển thị thông báo thành công (Đã gộp)
        SetMessageText(SUCCESS_MESSAGE);

        // 2. Chờ để người chơi kịp thấy thông báo (3 giây)
        yield return new WaitForSeconds(messageDisplayTime);

        // 3. GỌI LẠI VỀ TRIGGER: Lưu trạng thái, mở cửa, và cập nhật UI tương tác ngoài thế giới
        if (completionTrigger != null)
        {
            completionTrigger.SolvePuzzle();
            Debug.Log("Callback to FusePuzzleTrigger.SolvePuzzle() completed.");
        }
        else
        {
            Debug.LogWarning("Completion Trigger is missing! Cannot open door or save puzzle state.");
        }

        // 4. Vô hiệu hóa puzzle và trả về quyền điều khiển cho người chơi
        DeactivatePuzzle(true);

        // 5. THÔNG BÁO CHO TOÀN BỘ THẾ GIỚI RẰNG CÂU ĐỐ ĐÃ HOÀN THÀNH
        // Việc này buộc các đối tượng tương tác khác (như cửa/panel mới) phải
        // chạy lại logic kiểm tra phạm vi và hiển thị text UI của chúng.
        OnPuzzleCompleted?.Invoke();
        Debug.Log("Event OnPuzzleCompleted invoked. Other InteractableObjects should now refresh their UI state.");

        messageCoroutine = null;
    }

    // Actions on incorrect puzzle attempt
    private void HandleWrongSequence()
    {
        SetMessageText(ERROR_MESSAGE);
        Debug.Log("Puzzle check failed. Wrong sequence. Reverting instruction text shortly.");

        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(RevertMessageAfterDelay());
    }

    // Thiết lập nội dung text và màu sắc
    private void SetMessageText(string message)
    {
        if (messageTextComponent != null)
        {
            messageTextComponent.text = message;

            // Đặt màu dựa trên loại thông báo
            if (message == ERROR_MESSAGE)
            {
                messageTextComponent.color = Color.red;
            }
            else if (message == SUCCESS_MESSAGE)
            {
                messageTextComponent.color = Color.green; // Màu xanh cho thành công
            }
            else
            {
                messageTextComponent.color = Color.white; // Màu mặc định cho hướng dẫn
            }
        }
        else
        {
            Debug.LogWarning($"[UI] Message Text Component is NULL. Message: {message}");
        }
    }

    // Coroutine để trả lại văn bản hướng dẫn mặc định sau thông báo lỗi
    private IEnumerator RevertMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        if (isPuzzleActive)
        {
            SetMessageText(DEFAULT_INSTRUCTION);
        }
        messageCoroutine = null;
    }

    private void ChangeCylinderSelection(int direction)
    {
        if (cylinders == null || cylinders.Count == 0) return;

        if (currentCylinderIndex >= 0 && currentCylinderIndex < cylinders.Count && cylinders[currentCylinderIndex] != null)
        {
            cylinders[currentCylinderIndex].Deselect();
        }

        currentCylinderIndex = (currentCylinderIndex + direction + cylinders.Count) % cylinders.Count;

        if (currentCylinderIndex >= 0 && currentCylinderIndex < cylinders.Count && cylinders[currentCylinderIndex] != null)
        {
            cylinders[currentCylinderIndex].Select();
        }

        Debug.Log($"Cylinder selection changed to: {currentCylinderIndex + 1}");
    }

    private void RotateSelectedCylinder()
    {
        if (cylinders == null || currentCylinderIndex < 0 || currentCylinderIndex >= cylinders.Count || cylinders[currentCylinderIndex] == null) return;

        cylinders[currentCylinderIndex].RotateClockwise();

        Debug.Log($"Cylinder {currentCylinderIndex + 1} rotated. Current Symbol Index: {cylinders[currentCylinderIndex].currentSymbolIndex}");
    }

    // Kiểm tra trình tự biểu tượng hiện tại
    private void CheckSequence()
    {
        if (isPuzzleSolved) return;
        if (cylinders == null || cylinders.Count != correctSequence.Length || cylinders.Any(c => c == null))
        {
            Debug.LogError("Cylinders setup is invalid! Cannot check sequence.");
            HandleWrongSequence();
            return;
        }

        int[] currentSequence = new int[cylinders.Count];

        // 1. Lấy chỉ số biểu tượng hiện tại từ tất cả các hình trụ
        for (int i = 0; i < cylinders.Count; i++)
        {
            currentSequence[i] = cylinders[i].currentSymbolIndex;
        }

        // 2. So sánh với trình tự đúng
        bool isCorrect = currentSequence.SequenceEqual(correctSequence);

        if (isCorrect)
        {
            HandleCorrectSequence();
        }
        else
        {
            HandleWrongSequence();
        }
    }

    private void HandleInput()
    {
        // 1. CHUYỂN ĐỔI HÌNH TRỤ
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeCylinderSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeCylinderSelection(1);
        }

        // 2. XOAY HÌNH TRỤ (Phím E)
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RotateSelectedCylinder();
        }

        // 3. KIỂM TRA KẾT QUẢ (Phím Enter)
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckSequence();
        }
    }
}