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

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false;
    private bool isSceneTransitionActive = false;
    private bool hasDeclined = false; // MỚI: Trạng thái đã từ chối tương tác

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
            // Khi mới bước vào vùng, reset trạng thái từ chối
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
            // Khi bước ra khỏi vùng, reset trạng thái để lần sau bước vào lại có thể hiện UI
            hasDeclined = false;

            if (!isSceneTransitionActive)
            {
                EndInteraction();
            }
        }
    }

    void Update()
    {
        // CHỈ hiển thị UI nếu: 
        // 1. Người chơi trong vùng
        // 2. Chưa đang tương tác
        // 3. Không đang chuyển cảnh
        // 4. QUAN TRỌNG: Người chơi chưa bấm "No" (hasDeclined == false)
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

        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            // Cập nhật lại Text mỗi khi hiện (phòng trường hợp nhiều trigger dùng chung 1 panel)
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
                hasDeclined = true; // Đánh dấu là người chơi đã từ chối
                EndInteraction();
            }
        }
    }

    void EndInteraction()
    {
        isInteracting = false;
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
        if (GameManager.instance != null)
        {
            SceneManager.LoadScene(destinationSceneName);
        }
    }
}