using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class InteractionManager_NonRequiredButton : MonoBehaviour
{
    private const string FADER_GROUP_NAME = "Panel";

    [Header("Teleport Settings")]
    public string destinationSceneName;
    public string destinationSpawnPointName;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.0f;
    public float blackScreenDuration = 0.5f;

    [Header("UI Settings")]
    public string myQuestion = "Bạn có muốn bước qua không?";
    public string myYesText = "Có";
    public string myNoText = "Không";

    // Biến static để các script khác (Inventory) kiểm tra trạng thái
    public static bool IsInteractingWithUI = false;

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false;
    private bool isSceneTransitionActive = false;
    private bool hasDeclined = false;

    // Tham chiếu
    private Character_movement playerController;
    private Animator playerAnimator;
    private CanvasGroup faderCanvasGroup;
    private TextMeshProUGUI questionText;

    private float defaultFontSize = 24f;
    private float selectedFontSize = 32f;

    void Start()
    {
        StartCoroutine(SetupTextAndFader());
    }

    private IEnumerator SetupTextAndFader()
    {
        while (GameManager.instance == null || GameManager.instance.questionPanel == null)
        {
            yield return null;
        }

        if (faderCanvasGroup == null)
        {
            GameObject faderObj = GameObject.Find(FADER_GROUP_NAME);
            if (faderObj != null)
            {
                faderCanvasGroup = faderObj.GetComponent<CanvasGroup>();
            }
        }

        if (GameManager.instance.questionPanel != null)
        {
            questionText = GameManager.instance.questionPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (questionText != null) questionText.text = myQuestion;
        if (GameManager.instance.yesText != null) GameManager.instance.yesText.text = myYesText;
        if (GameManager.instance.noText != null) GameManager.instance.noText.text = myNoText;

        HideUI();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            hasDeclined = false;

            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();

            StartCoroutine(WaitForUIReadyAndShow());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            hasDeclined = false;

            if (!isSceneTransitionActive)
            {
                EndInteraction();
            }
        }
    }

    void Update()
    {
        if (playerInRange && !isInteracting && !isSceneTransitionActive && !hasDeclined)
        {
            ShowUI();
        }

        if (isInteracting && !isSceneTransitionActive)
        {
            HandleUIInput();
        }
    }

    private IEnumerator WaitForUIReadyAndShow()
    {
        while (GameManager.instance == null || GameManager.instance.questionPanel == null || questionText == null)
        {
            yield return null;
        }

        if (!isInteracting && playerInRange && !hasDeclined)
        {
            ShowUI();
        }
    }

    void ShowUI()
    {
        isInteracting = true;
        IsInteractingWithUI = true; // Kích hoạt trạng thái khóa các phím chức năng (C, M...)

        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            if (questionText != null) questionText.text = myQuestion;
            if (GameManager.instance.yesText != null) GameManager.instance.yesText.text = myYesText;
            if (GameManager.instance.noText != null) GameManager.instance.noText.text = myNoText;

            GameManager.instance.questionPanel.SetActive(true);
            selectedOption = 0;
            UpdateSelectionUI();

            if (playerController != null) playerController.canMove = false;
            if (playerAnimator != null) playerAnimator.SetBool("IsMoving", false);
        }
    }

    void HideUI()
    {
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(false);
        }

        // Chỉ mở khóa di chuyển nếu không đang trong quá trình chuyển cảnh
        if (!isSceneTransitionActive && playerController != null)
        {
            playerController.canMove = true;
        }
    }

    void UpdateSelectionUI()
    {
        if (GameManager.instance != null)
        {
            if (GameManager.instance.yesText != null)
                GameManager.instance.yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
            if (GameManager.instance.noText != null)
                GameManager.instance.noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
        }
    }

    void HandleUIInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedOption = (selectedOption + 1) % 2;
            UpdateSelectionUI();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (selectedOption == 0) // Chọn YES
            {
                if (!isSceneTransitionActive)
                {
                    StartCoroutine(FadeAndLoadScene());
                }
            }
            else // Chọn NO
            {
                hasDeclined = true;
                EndInteraction();
            }
        }
    }

    void EndInteraction()
    {
        isInteracting = false;
        IsInteractingWithUI = false; // Giải phóng trạng thái để có thể bấm C, M...
        HideUI();
    }

    private IEnumerator FadeAndLoadScene()
    {
        if (faderCanvasGroup == null)
        {
            LoadNewScene();
            yield break;
        }

        isSceneTransitionActive = true;
        // Giữ IsInteractingWithUI = true để không bấm được gì khi đang mờ dần màn hình
        HideUI();

        faderCanvasGroup.blocksRaycasts = true;

        while (faderCanvasGroup.alpha < 1)
        {
            faderCanvasGroup.alpha += Time.deltaTime / fadeSpeed;
            yield return null;
        }
        faderCanvasGroup.alpha = 1;

        yield return new WaitForSeconds(blackScreenDuration);
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        // Trước khi đổi scene hoàn toàn, nên reset biến static
        IsInteractingWithUI = false;

        if (GameManager.instance != null)
        {
            SceneManager.LoadScene(destinationSceneName);
        }
    }
}