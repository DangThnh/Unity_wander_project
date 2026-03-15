using UnityEngine;
using TMPro;
using System.Collections;

public class ChairRestSystem : MonoBehaviour
{
    private const string FADER_GROUP_NAME = "Panel";

    [Header("UI Assignments")]
    public GameObject myQuestionPanel;
    public TextMeshProUGUI myQuestionText;
    public TextMeshProUGUI myYesText;
    public TextMeshProUGUI myNoText;
    public CanvasGroup faderCanvasGroup;

    [Header("Cấu hình Nội dung")]
    public string myQuestion = "Bạn có muốn ngồi nghỉ một lát không?";
    public string yesButtonLabel = "Yes";
    public string noButtonLabel = "No";

    [Header("Font Settings")]
    public float defaultFontSize = 16f;
    public float selectedFontSize = 22f;

    [Header("Cấu hình Camera & Model")]
    public Camera mainCamera;
    public Camera sitCamera;
    public GameObject sittingModelB;
    public Transform sittingPoint;
    public Transform hiddenPoint;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.0f;
    public float blackScreenDuration = 0.5f;

    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false;
    private bool isResting = false;
    private bool isFading = false;
    private int interactionStep = 0;

    private Vector3 originalPlayerPos;
    private Character_movement moveScript;
    private Animator playerAnimator;

    void Start()
    {
        if (faderCanvasGroup == null)
        {
            GameObject faderObj = GameObject.Find(FADER_GROUP_NAME);
            if (faderObj != null) faderCanvasGroup = faderObj.GetComponent<CanvasGroup>();
        }

        HideUI();
        if (sitCamera != null) sitCamera.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isResting)
        {
            playerInRange = true;
            moveScript = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (isInteracting && !isFading) EndInteraction();
        }
    }

    void Update()
    {
        // Nhấn E để bắt đầu tương tác
        if (playerInRange && !isResting && !isFading && Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteracting) StartInteraction();
        }

        // Điều khiển UI khi đang ở bảng hỏi (Step 1)
        if (isInteracting && interactionStep == 1 && !isFading)
        {
            HandleUIInput();
        }

        // Thoát trạng thái nghỉ
        if (isResting && !isFading && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ExitRestMode());
        }
    }

    void StartInteraction()
    {
        isInteracting = true;

        // Khóa di chuyển
        if (moveScript != null) moveScript.canMove = false;
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isMovingForward", false);
            playerAnimator.SetBool("isMovingBackward", false);
        }

        // Nhảy thẳng vào hiện bảng hỏi (Step 1)
        ShowQuestionPanel();
    }

    void ShowQuestionPanel()
    {
        interactionStep = 1; // Đánh dấu đang ở bước chọn
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
        // Phím mũi tên/AD để chọn
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            selectedOption = (selectedOption + 1) % 2;
            UpdateSelectionUI();
        }

        // Enter/Space để xác nhận
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedOption == 0) StartCoroutine(EnterRestMode());
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
        if (moveScript != null && !isResting) moveScript.canMove = true;
    }

    IEnumerator EnterRestMode()
    {
        isFading = true;
        HideUI();

        yield return StartCoroutine(FadeEffect(1f));

        isResting = true;
        isInteracting = false;
        interactionStep = 0;
        originalPlayerPos = moveScript.transform.position;

        // Giấu player gốc, hiện model ngồi
        moveScript.transform.position = hiddenPoint.position;
        if (sittingModelB != null)
        {
            sittingModelB.transform.position = sittingPoint.position;
            sittingModelB.transform.rotation = sittingPoint.rotation;
        }

        // Đổi Camera
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (sitCamera != null) sitCamera.gameObject.SetActive(true);

        yield return new WaitForSeconds(blackScreenDuration);
        yield return StartCoroutine(FadeEffect(0f));
        isFading = false;
    }

    IEnumerator ExitRestMode()
    {
        isFading = true;
        yield return StartCoroutine(FadeEffect(1f));

        isResting = false;
        if (sittingModelB != null) sittingModelB.transform.position = hiddenPoint.position;
        if (sitCamera != null) sitCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        moveScript.transform.position = originalPlayerPos;
        moveScript.canMove = true;

        yield return new WaitForSeconds(blackScreenDuration);
        yield return StartCoroutine(FadeEffect(0f));
        isFading = false;
    }

    IEnumerator FadeEffect(float targetAlpha)
    {
        if (faderCanvasGroup == null) yield break;
        float timer = 0;
        float startAlpha = faderCanvasGroup.alpha;
        while (timer < fadeSpeed)
        {
            timer += Time.deltaTime;
            faderCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeSpeed);
            yield return null;
        }
        faderCanvasGroup.alpha = targetAlpha;
    }
}