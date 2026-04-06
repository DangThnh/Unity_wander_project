using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Quản lý logic câu đố Hexa. 
/// Đã được cập nhật để chặn kích hoạt khi Inventory đang mở.
/// </summary>
public class HexaPuzzleManager : MonoBehaviour
{
    // --- STATIC STATE ---
    public static bool IsPuzzleActiveStatic { get; private set; } = false;
    public static bool IsPuzzleSolvedStatic { get; private set; } = false;

    public static event System.Action OnPuzzleCompleted;

    // --- HẰNG SỐ TEXT ---
    private const string DEFAULT_INSTRUCTION = "Press E to rotate, arrow keys to toggle and Enter to confirm. Press Space Bar to stop.";
    private const string ERROR_MESSAGE = "Error! Wrong Passcode!";
    private const string SUCCESS_MESSAGE = "Correct! Access Granted. Control Room Unlocked.";

    [Tooltip("Danh sách 4 hình trụ.")]
    public List<RotatableCylinder> cylinders;

    [Tooltip("Trình tự biểu tượng đúng.")]
    public int[] correctSequence = new int[4] { 1, 3, 5, 2 };

    [Header("Visual & Exit Control")]
    public GameObject puzzleRootGameObject;

    [Header("UI Feedback")]
    public TextMeshProUGUI messageTextComponent;
    public float messageDisplayTime = 3f;

    private int currentCylinderIndex = 0;
    private bool isPuzzleActive = false;
    private bool isPuzzleSolved = false;
    private Coroutine messageCoroutine;
    private FusePuzzleTrigger completionTrigger;

    void Start()
    {
        if (cylinders == null || cylinders.Count != 4)
        {
            Debug.LogError("HexaPuzzleManager requires exactly 4 cylinders.");
        }

        if (puzzleRootGameObject != null)
        {
            puzzleRootGameObject.SetActive(false);
        }

        isPuzzleActive = false;
        IsPuzzleActiveStatic = false;
    }

    void Update()
    {
        // 1. Nếu Inventory đang mở, chúng ta KHÔNG xử lý bất cứ input nào của Puzzle
        if (InventoryUI.IsInventoryOpenStatic)
        {
            return;
        }

        // 2. Thoát Puzzle bằng phím Space
        if (isPuzzleActive && !isPuzzleSolved && Input.GetKeyDown(KeyCode.Space))
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

    public void SetCompletionCallback(FusePuzzleTrigger trigger)
    {
        this.completionTrigger = trigger;
    }

    // Kích hoạt giao diện câu đố
    public void ActivatePuzzle()
    {
        // --- KHẮC PHỤC LỖI TRÙNG PHÍM E ---
        // Nếu Inventory đang mở (bao gồm cả màn hình thông báo kết quả craft), 
        // tuyệt đối không cho phép kích hoạt Puzzle.
        if (InventoryUI.IsInventoryOpenStatic)
        {
            Debug.Log("Cannot activate puzzle: Inventory is currently open.");
            return;
        }

        if (PauseMenuManager.IsPausedStatic)
        {
            Debug.Log("Cannot activate puzzle: Game is currently paused.");
            return;
        }

        if (isPuzzleSolved || isPuzzleActive) return;

        isPuzzleActive = true;
        IsPuzzleActiveStatic = true;

        if (puzzleRootGameObject != null)
        {
            puzzleRootGameObject.SetActive(true);
        }

        SetMessageText(DEFAULT_INSTRUCTION);

        currentCylinderIndex = 0;
        if (cylinders != null && cylinders.Count > 0)
        {
            cylinders[currentCylinderIndex].Select();
        }
    }

    public void DeactivatePuzzle(bool solved)
    {
        isPuzzleActive = false;
        IsPuzzleActiveStatic = false;

        if (solved)
        {
            isPuzzleSolved = true;
            IsPuzzleSolvedStatic = true;
        }

        if (messageCoroutine != null) StopCoroutine(messageCoroutine);

        if (puzzleRootGameObject != null)
        {
            puzzleRootGameObject.SetActive(false);
        }

        if (cylinders != null && currentCylinderIndex >= 0 && currentCylinderIndex < cylinders.Count)
        {
            cylinders[currentCylinderIndex].Deselect();
        }
    }

    private void HandleCorrectSequence()
    {
        GetComponent<HexaPuzzleAudioManager>()?.PlayCorrect();
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(SolveSequenceAndDeactivate());
    }

    private IEnumerator SolveSequenceAndDeactivate()
    {
        SetMessageText(SUCCESS_MESSAGE);
        yield return new WaitForSeconds(messageDisplayTime);

        if (completionTrigger != null)
        {
            completionTrigger.SolvePuzzle();
        }

        DeactivatePuzzle(true);
        OnPuzzleCompleted?.Invoke();
        messageCoroutine = null;
    }

    private void HandleWrongSequence()
    {
        GetComponent<HexaPuzzleAudioManager>()?.PlayError();
        SetMessageText(ERROR_MESSAGE);
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(RevertMessageAfterDelay());
    }

    private void SetMessageText(string message)
    {
        if (messageTextComponent != null)
        {
            messageTextComponent.text = message;
            if (message == ERROR_MESSAGE) messageTextComponent.color = Color.red;
            else if (message == SUCCESS_MESSAGE) messageTextComponent.color = Color.green;
            else messageTextComponent.color = Color.white;
        }
    }

    private IEnumerator RevertMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        if (isPuzzleActive) SetMessageText(DEFAULT_INSTRUCTION);
        messageCoroutine = null;
    }

    private void ChangeCylinderSelection(int direction)
    {
        GetComponent<HexaPuzzleAudioManager>()?.PlaySwitch();
        cylinders[currentCylinderIndex].Deselect();
        currentCylinderIndex = (currentCylinderIndex + direction + cylinders.Count) % cylinders.Count;
        cylinders[currentCylinderIndex].Select();
    }

    private void RotateSelectedCylinder()
    {
        GetComponent<HexaPuzzleAudioManager>()?.PlayRotate();
        cylinders[currentCylinderIndex].RotateClockwise();
    }

    private void CheckSequence()
    {
        if (isPuzzleSolved) return;
        int[] currentSequence = cylinders.Select(c => c.currentSymbolIndex).ToArray();

        if (currentSequence.SequenceEqual(correctSequence)) HandleCorrectSequence();
        else HandleWrongSequence();
    }

    private void HandleInput()
    {
        // Chặn phím C và Esc khi đang trong Puzzle (để tránh mở chồng menu)
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeCylinderSelection(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeCylinderSelection(1);
        else if (Input.GetKeyDown(KeyCode.E)) RotateSelectedCylinder();
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) CheckSequence();
    }
}