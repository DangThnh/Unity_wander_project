using System.Collections;
using UnityEngine;
using TMPro;

public class PasscodePanel : MonoBehaviour
{
    // Cánh cửa mà script này sẽ điều khiển
    public DoorInteractionPasscode targetDoor;

    // Mật khẩu đúng
    public string passcode = "1023";

    // Tham chiếu đến màn hình hiển thị
    public TextMeshProUGUI displayText;

    // Các tham chiếu UI khác
    public GameObject keypadUI;

    private string currentInput = "";
    private bool isSolved = false;
    // ĐÃ XÓA: private bool isPlayerInRange = false; // Loại bỏ cảnh báo CS0414

    // Hướng dẫn
    // ĐÃ XÓA: private string pressEToExitPrompt = "Press E to exit"; // Loại bỏ cảnh báo CS0414
    private string wrongPasscodePrompt = "Wrong Passcode!";
    private string correctPasscodePrompt = "Correct!";

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
    }

    public void HideKeypad()
    {
        keypadUI.SetActive(false);
        if (GameManager.instance != null && GameManager.instance.interactionText != null)
        {
            GameManager.instance.interactionText.text = "";
        }
    }

    private void HandleKeypadInput()
    {
        // Xử lý nhập số từ bàn phím
        if (Input.anyKeyDown)
        {
            foreach (char c in Input.inputString)
            {
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
            if (targetDoor != null)
            {
                targetDoor.EndInteraction();
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