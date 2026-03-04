using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class InteractionManager_RequiredItemTeleport : MonoBehaviour
{
    private const string FADER_GROUP_NAME = "Panel";

    [Header("UI Assignments (Kéo thả vào đây)")]
    public GameObject myQuestionPanel;    // Panel chứa câu hỏi
    public TextMeshProUGUI myQuestionText; // Text nội dung câu hỏi/thông báo
    public TextMeshProUGUI myYesText;      // Text cho lựa chọn Yes
    public TextMeshProUGUI myNoText;       // Text cho lựa chọn No
    public CanvasGroup faderCanvasGroup;   // Màn hình đen để fade

    [Header("Teleport Settings")]
    public string destinationSceneName;
    public string destinationSpawnPointName;

    [Header("Requirement Settings")]
    public string requiredItemId;
    public string text1 = "You discover an ancient portal...";
    public string text2HasItem = "The key in your pocket begins to glow.";
    public string requirementFailureText = "It seems you lack the necessary item to activate this.";

    [Header("Fade Settings")]
    public float fadeSpeed = 1.0f;
    public float blackScreenDuration = 0.5f;

    [Header("UI Text Content")]
    public string myQuestion = "Do you want to step across?";
    public string yesButtonLabel = "Yes";
    public string noButtonLabel = "No";

    [Header("Font Settings")]
    public float defaultFontSize = 16f;
    public float selectedFontSize = 22f;

    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false;
    private bool isSceneTransitionActive = false;
    private int interactionStep = 0;

    private Character_movement playerController;
    private Animator playerAnimator;

    void Start()
    {
        // Nếu bạn quên gán fader, script sẽ cố gắng tìm theo tên một lần cuối
        if (faderCanvasGroup == null)
        {
            GameObject faderObj = GameObject.Find(FADER_GROUP_NAME);
            if (faderObj != null) faderCanvasGroup = faderObj.GetComponent<CanvasGroup>();
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

        // Vẫn gọi InventoryManager.instance vì đây thường là Singleton quản lý dữ liệu toàn cục
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
        if (myQuestionPanel != null)
        {
            myQuestionPanel.SetActive(true);
            if (myQuestionText != null) myQuestionText.text = content;
            if (myYesText != null) myYesText.gameObject.SetActive(false);
            if (myNoText != null) myNoText.gameObject.SetActive(false);
        }
    }

    void ShowQuestionPanel()
    {
        interactionStep = 3;
        if (myQuestionPanel != null)
        {
            myQuestionPanel.SetActive(true);
            if (myQuestionText != null) myQuestionText.text = myQuestion;

            if (myYesText != null)
            {
                myYesText.gameObject.SetActive(true);
                myYesText.text = yesButtonLabel;
            }
            if (myNoText != null)
            {
                myNoText.gameObject.SetActive(true);
                myNoText.text = noButtonLabel;
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

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedOption == 0) StartCoroutine(FadeAndLoadScene());
            else EndInteraction();
        }
    }

    void UpdateSelectionUI()
    {
        if (myYesText != null) myYesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
        if (myNoText != null) myNoText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
    }

    void HideUI()
    {
        if (myQuestionPanel != null)
        {
            if (myQuestionText != null) myQuestionText.text = "";
            myQuestionPanel.SetActive(false);
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

        float timer = 0;
        while (timer < fadeSpeed)
        {
            timer += Time.deltaTime;
            faderCanvasGroup.alpha = timer / fadeSpeed;
            yield return null;
        }
        faderCanvasGroup.alpha = 1;

        yield return new WaitForSeconds(blackScreenDuration);
        SceneManager.LoadScene(destinationSceneName);
    }
}