using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class InteractionManager_RequiredItemTeleport : MonoBehaviour
{
    private const string FADER_GROUP_NAME = "Panel";

    [Header("Teleport Settings")]
    public string destinationSceneName;
    public string destinationSpawnPointName;

    [Header("Requirement Settings")]
    public string requiredItemId; // Để trống nếu không cần item
    public string text1 = "You discover an ancient portal..."; // Luôn hiện đầu tiên
    public string text2HasItem = "The key in your pocket begins to glow."; // Hiện nếu có item
    public string requirementFailureText = "It seems you lack the necessary item to activate this.";

    [Header("Fade Settings")]
    public float fadeSpeed = 1.0f;
    public float blackScreenDuration = 0.5f;

    [Header("UI Settings")]
    public TextMeshProUGUI customQuestionText; // Option gán text thủ công
    public string myQuestion = "Do you want to step across?";
    public string myYesText = "Yes";
    public string myNoText = "No";

    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false;
    private bool isSceneTransitionActive = false;

    // Quản lý các bước tương tác
    // 0: Idle, 1: Text 1, 2: Text 2 (Has Item), 3: Question Panel, 4: Failure Message
    private int interactionStep = 0;

    private Character_movement playerController;
    private Animator playerAnimator;
    private CanvasGroup faderCanvasGroup;

    private TextMeshProUGUI questionText;
    private TextMeshProUGUI yesText;
    private TextMeshProUGUI noText;

    private float defaultFontSize = 16f;
    private float selectedFontSize = 22f;

    void Start()
    {
        StartCoroutine(SetupTextAndFader());
    }

    private IEnumerator SetupTextAndFader()
    {
        while (GameManager.instance == null || GameManager.instance.questionPanel == null)
            yield return null;

        if (faderCanvasGroup == null)
        {
            GameObject faderObj = GameObject.Find(FADER_GROUP_NAME);
            if (faderObj != null)
                faderCanvasGroup = faderObj.GetComponent<CanvasGroup>();
        }

        if (GameManager.instance.questionPanel != null)
        {
            questionText = (customQuestionText != null) ? customQuestionText : GameManager.instance.questionPanel.GetComponentInChildren<TextMeshProUGUI>();
            yesText = GameManager.instance.yesText;
            noText = GameManager.instance.noText;
        }

        HideUI();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (isInteracting && !isSceneTransitionActive) EndInteraction();
        }
    }

    void Update()
    {
        // Thêm kiểm tra GetKeyDown để tránh việc thực thi liên tục trong một frame
        if (playerInRange && !isSceneTransitionActive && Input.GetKeyDown(KeyCode.E))
        {
            HandleInteractionFlow();
        }

        if (isInteracting && interactionStep == 3 && !isSceneTransitionActive)
        {
            HandleUIInput();
        }
    }

    void HandleInteractionFlow()
    {
        if (!isInteracting)
        {
            StartInteraction();
            return;
        }

        switch (interactionStep)
        {
            case 1:
                CheckRequirementsAndProceed();
                break;

            case 2:
                ShowQuestionPanel();
                break;

            case 4:
                // Cần return ngay sau khi gọi End để tránh các logic Update phía sau can thiệp
                EndInteraction();
                break;
        }
    }

    void StartInteraction()
    {
        isInteracting = true;
        interactionStep = 1;

        if (playerController != null) playerController.canMove = false;
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isMovingForward", false);
            playerAnimator.SetBool("isMovingBackward", false);
        }

        ShowTextOnly(text1);
    }

    void CheckRequirementsAndProceed()
    {
        bool isRequired = !string.IsNullOrEmpty(requiredItemId);

        if (!isRequired)
        {
            ShowQuestionPanel();
            return;
        }

        bool hasItem = (InventoryManager.instance != null && InventoryManager.instance.HasItem(requiredItemId));

        if (hasItem)
        {
            interactionStep = 2;
            ShowTextOnly(text2HasItem);
        }
        else
        {
            interactionStep = 4;
            ShowTextOnly(requirementFailureText);
        }
    }

    void ShowTextOnly(string content)
    {
        if (GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(true);
            if (questionText != null) questionText.text = content;
            if (yesText != null) yesText.gameObject.SetActive(false);
            if (noText != null) noText.gameObject.SetActive(false);
        }
    }

    void ShowQuestionPanel()
    {
        interactionStep = 3;
        if (GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(true);
            if (questionText != null) questionText.text = myQuestion;

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

            selectedOption = 0;
            UpdateSelectionUI();
        }
    }

    void HandleUIInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            selectedOption = (selectedOption + 1) % 2;
            UpdateSelectionUI();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (selectedOption == 0) StartCoroutine(FadeAndLoadScene());
            else EndInteraction();
        }
    }

    void UpdateSelectionUI()
    {
        if (yesText != null) yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
        if (noText != null) noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
    }

    void HideUI()
    {
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            // Reset text về rỗng trước khi ẩn panel để tránh "ghost text" ở lần hiện sau
            if (questionText != null) questionText.text = "";
            GameManager.instance.questionPanel.SetActive(false);
        }
    }

    void EndInteraction()
    {
        isInteracting = false;
        interactionStep = 0;
        HideUI();
        if (playerController != null && !isSceneTransitionActive) playerController.canMove = true;
    }

    private IEnumerator FadeAndLoadScene()
    {
        if (faderCanvasGroup == null)
        {
            SceneManager.LoadScene(destinationSceneName);
            yield break;
        }

        isSceneTransitionActive = true;
        HideUI();
        faderCanvasGroup.blocksRaycasts = true;

        while (faderCanvasGroup.alpha < 1)
        {
            faderCanvasGroup.alpha += Time.deltaTime / fadeSpeed;
            yield return null;
        }

        yield return new WaitForSeconds(blackScreenDuration);
        SceneManager.LoadScene(destinationSceneName);
    }
}