using System.Collections;
using UnityEngine;
using TMPro;

public class PasscodePanel : MonoBehaviour
{
    public DoorInteractionPasscode targetDoor;
    public string passcode = "1023";
    public TextMeshProUGUI displayText;
    public GameObject keypadUI;

    private string currentInput = "";
    private bool isSolved = false;

    private string wrongPasscodePrompt = "Wrong Passcode!";
    private string correctPasscodePrompt = "Correct!";

    void Start()
    {
        keypadUI.SetActive(false);
    }

    void Update()
    {
        // Thêm kiểm tra: Chỉ cho phép nhập nếu UI đang hiện và cửa chưa được giải xong
        if (keypadUI.activeSelf && !isSolved)
        {
            HandleKeypadInput();
        }
    }

    public void ShowKeypad()
    {
        keypadUI.SetActive(true);
        isSolved = false; // Reset trạng thái khi mở lại
        currentInput = "";
        UpdateDisplay();

        // Vô hiệu hóa di chuyển của Player nếu cần (tùy chọn)
        // Cursor.lockState = CursorLockMode.None;
        // Cursor.visible = true;
    }

    public void HideKeypad()
    {
        keypadUI.SetActive(false);

        // Thay vì gọi GameManager (có thể gây lỗi nếu chưa set up), 
        // hãy gọi trực tiếp thông qua targetDoor để dọn Text UI
        if (targetDoor != null)
        {
            targetDoor.EndInteraction();
        }
    }

    private void HandleKeypadInput()
    {
        if (Input.anyKeyDown)
        {
            foreach (char c in Input.inputString)
            {
                if (char.IsDigit(c))
                {
                    if (currentInput.Length < passcode.Length)
                    {
                        currentInput += c;
                        UpdateDisplay();
                    }
                    break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckPasscode();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                UpdateDisplay();
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            HideKeypad();
        }
    }

    private void CheckPasscode()
    {
        if (currentInput == passcode)
        {
            isSolved = true;
            displayText.text = correctPasscodePrompt;

            // QUAN TRỌNG: Gọi UnlockDoor từ targetDoor để kích hoạt âm thanh "Thành công"
            if (targetDoor != null)
            {
                targetDoor.UnlockDoor();
            }

            StartCoroutine(ClosePanelAfterDelay());
        }
        else
        {
            displayText.text = wrongPasscodePrompt;

            // QUAN TRỌNG: Gọi DenyAccess để phát âm thanh "Sai mã"
            if (targetDoor != null)
            {
                targetDoor.DenyAccess();
            }

            currentInput = "";
            StartCoroutine(ClearDisplayAfterDelay());
        }
    }

    private void UpdateDisplay()
    {
        displayText.text = currentInput;
    }

    private IEnumerator ClearDisplayAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);
        if (!isSolved) UpdateDisplay(); // Trả lại hiển thị rỗng
    }

    private IEnumerator ClosePanelAfterDelay()
    {
        // Chờ một chút để người chơi thấy chữ "Correct!"
        yield return new WaitForSeconds(1.0f);
        HideKeypad();
    }
}