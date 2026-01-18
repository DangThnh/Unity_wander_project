using System.Collections;
using UnityEngine;
using TMPro;

public class PasscodePanelVendingMachine : MonoBehaviour
{
    // Đã sửa tham chiếu tới tên class tương ứng: DoorInteractionPasscode
    // Kéo và thả script DoorInteractionPasscode của cánh cửa vào đây
    public DoorPasscodeVendingMachine targetDoor;

    // Mật khẩu đúng (Có thể chứa cả số và chữ F, G, H, J, K, L)
    public string passcode = "3H4F";

    // Tham chiếu đến màn hình hiển thị
    public TextMeshProUGUI displayText;

    // Các tham chiếu UI khác
    public GameObject keypadUI;

    private string currentInput = "";
    private bool isSolved = false;
    // Đã loại bỏ biến isPlayerInRange vì nó không cần thiết trong Panel

    // Hướng dẫn
    // ĐÃ XÓA: private string pressEToExitPrompt = "Press E to exit"; // Loại bỏ cảnh báo CS0414
    private string wrongPasscodePrompt = "Out of stock!";
    private string correctPasscodePrompt = "Hidden password confirmed!";

    // Danh sách các ký tự chữ cái được phép nhập
    private string allowedLetters = "FGHJKL";

    void Start()
    {
        // Ẩn UI khi bắt đầu
        keypadUI.SetActive(false);
    }

    void Update()
    {
        if (keypadUI.activeSelf && !isSolved)
        {
            HandleKeypadInput();
        }
    }

    public void ShowKeypad()
    {
        keypadUI.SetActive(true);
        currentInput = "";
        UpdateDisplay();

        // Cập nhật gợi ý: Hiển thị hướng dẫn thoát trên màn hình game (nếu có hệ thống thông báo chung)
        // Lưu ý: Nếu có text tương tác riêng, nó nên được cập nhật bởi script gọi (DoorInteractionPasscode)
    }

    public void HideKeypad()
    {
        keypadUI.SetActive(false);
        // Khi ẩn bàn phím, thông báo tương tác sẽ được xử lý bởi targetDoor
        if (targetDoor != null)
        {
            targetDoor.EndInteraction();
        }
    }

    private void HandleKeypadInput()
    {
        // Xử lý nhập số và các chữ cái được phép
        if (Input.anyKeyDown)
        {
            foreach (char c in Input.inputString)
            {
                // 1. Kiểm tra chữ số
                if (char.IsDigit(c))
                {
                    // Thêm giới hạn để ngăn nhập liệu dài hơn mật khẩu
                    if (currentInput.Length < passcode.Length)
                    {
                        currentInput += c;
                        UpdateDisplay();
                    }
                    break;
                }

                // 2. Kiểm tra chữ cái được phép (chuyển sang chữ hoa để kiểm tra)
                char upperC = char.ToUpper(c);
                if (allowedLetters.Contains(upperC.ToString()))
                {
                    // Thêm giới hạn để ngăn nhập liệu dài hơn mật khẩu
                    if (currentInput.Length < passcode.Length)
                    {
                        currentInput += upperC; // Lưu dưới dạng chữ hoa
                        UpdateDisplay();
                    }
                    break;
                }
            }
        }

        // Xử lý các phím chức năng
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
            // EndInteraction đã được gọi trong HideKeypad()
        }
    }

    public void OnNumberClick(string number)
    {
        // Hàm này có thể được gọi từ các button UI
        if (!isSolved && currentInput.Length < passcode.Length)
        {
            // Kiểm tra xem ký tự được click có hợp lệ không (chữ số hoặc chữ cái cho phép)
            if (char.IsDigit(number[0]) || allowedLetters.Contains(char.ToUpper(number[0]).ToString()))
            {
                currentInput += char.ToUpper(number[0]); // Đảm bảo lưu chữ hoa
                UpdateDisplay();
            }
        }
    }

    private void CheckPasscode()
    {
        if (currentInput == passcode)
        {
            isSolved = true;
            displayText.text = correctPasscodePrompt;
            StartCoroutine(OpenDoorAfterDelay());
        }
        else
        {
            displayText.text = wrongPasscodePrompt;
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
        yield return new WaitForSeconds(1.5f);
        displayText.text = "";
    }

    private IEnumerator OpenDoorAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (targetDoor != null)
        {
            targetDoor.UnlockDoor();
        }
        HideKeypad();
    }
}