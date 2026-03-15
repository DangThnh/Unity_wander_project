using System.Collections;
using UnityEngine;
using TMPro;

public class PasscodePanelVendingMachine : MonoBehaviour
{
    public DoorPasscodeVendingMachine targetDoor;
    public string passcode = "3H4F";
    public TextMeshProUGUI displayText;
    public GameObject keypadUI;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip buttonClickClip;  // Tiếng bấm nút
    public AudioClip correctSoundClip; // Tiếng khi nhập đúng (Ting!)
    public AudioClip wrongSoundClip;   // Tiếng khi nhập sai (Bíp bíp)

    private string currentInput = "";
    private bool isSolved = false;
    private string wrongPasscodePrompt = "Out of stock!";
    private string correctPasscodePrompt = "Hidden password confirmed!";
    private string allowedLetters = "FGHJKL";

    void Awake()
    {
        // Tự động tìm AudioSource trên UI Panel
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
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
        if (targetDoor != null) targetDoor.EndInteraction();
    }

    private void HandleKeypadInput()
    {
        if (Input.anyKeyDown)
        {
            foreach (char c in Input.inputString)
            {
                char upperC = char.ToUpper(c);
                if (char.IsDigit(c) || allowedLetters.Contains(upperC.ToString()))
                {
                    if (currentInput.Length < passcode.Length)
                    {
                        currentInput += (char.IsDigit(c) ? c : upperC);
                        PlaySound(buttonClickClip); // PHÁT TIẾNG BẤM PHÍM
                        UpdateDisplay();
                    }
                    break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) CheckPasscode();
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                PlaySound(buttonClickClip);
                UpdateDisplay();
            }
        }
        else if (Input.GetKeyDown(KeyCode.E)) HideKeypad();
    }

    private void CheckPasscode()
    {
        if (currentInput == passcode)
        {
            isSolved = true;
            displayText.text = correctPasscodePrompt;
            PlaySound(correctSoundClip); // PHÁT TIẾNG ĐÚNG
            StartCoroutine(OpenDoorAfterDelay());
        }
        else
        {
            displayText.text = wrongPasscodePrompt;
            PlaySound(wrongSoundClip); // PHÁT TIẾNG SAI
            currentInput = "";
            StartCoroutine(ClearDisplayAfterDelay());
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void UpdateDisplay() { displayText.text = currentInput; }

    private IEnumerator ClearDisplayAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (!isSolved) displayText.text = "";
    }

    private IEnumerator OpenDoorAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (targetDoor != null) targetDoor.UnlockDoor();
        HideKeypad();
    }
}