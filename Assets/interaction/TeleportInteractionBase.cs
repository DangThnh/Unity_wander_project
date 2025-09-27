using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Lớp cơ sở chứa tất cả logic vận hành, được kế thừa bởi lớp Prefab spawn
public abstract class TeleportInteractionBase : MonoBehaviour
{
    // Cài đặt cho lời thoại trước khi tương tác
    [Tooltip("Dòng chữ hiển thị trước khi bảng câu hỏi hiện ra.")]
    public string preInteractionLine;

    [Header("UI Global References")]
    [Tooltip("Dòng chữ hiển thị trên HUD (sẽ được tìm kiếm tự động nếu không gán).")]
    public TextMeshProUGUI interactionText;

    [Tooltip("Bảng UI chứa câu hỏi (sẽ được tìm kiếm tự động nếu không gán).")]
    public GameObject questionPanel;

    [Tooltip("Canvas Group của đối tượng làm mờ màn hình (sẽ được tìm kiếm tự động nếu không gán).")]
    public CanvasGroup faderCanvasGroup;

    [Header("Question Panel Components")]
    // Các trường này có thể được tìm kiếm bên trong questionPanel
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI yesText;
    public TextMeshProUGUI noText;

    // Các biến giá trị (giữ nguyên để dễ dàng chỉnh sửa Prefab)
    public string myQuestion = "Bạn có muốn dịch chuyển không?";
    public string myYesText = "Có";
    public string myNoText = "Không";

    // Cài đặt hiệu ứng
    public float fadeSpeed = 1f;
    public float blackScreenDuration = 1f;
    public string destinationId;

    // Trạng thái và điều khiển
    protected Character_movement playerController;
    protected Animator playerAnimator;
    protected bool playerInRange = false;
    protected int selectedOption = 0;
    protected bool isInteracting = false;
    protected bool isShowingPreText = false;
    protected bool isFading = false;
    protected float defaultFontSize = 36f;
    protected float selectedFontSize = 48f;

    // Phương thức Start() này sẽ được gọi BỞI lớp con (Derived Class)
    protected virtual void Start()
    {
        // Khởi tạo các giá trị text
        if (questionText != null) questionText.text = myQuestion;
        if (yesText != null) yesText.text = myYesText;
        if (noText != null) noText.text = myNoText;

        HideUI();
        if (interactionText != null) interactionText.text = "";
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();

            if (interactionText == null)
            {
                Debug.LogError("InteractionText is missing. Cannot proceed with interaction.");
                return;
            }

            if (string.IsNullOrEmpty(preInteractionLine))
            {
                ShowUI();
            }
            else
            {
                StartCoroutine(ShowPreInteractionText());
            }
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (!isInteracting && !isFading)
            {
                EndInteraction();
            }
        }
    }

    protected void Update()
    {
        if (isInteracting)
        {
            HandleUIInput();
        }
        else if (playerInRange && isShowingPreText && Input.GetKeyDown(KeyCode.E))
        {
            isShowingPreText = false;
            if (interactionText != null) interactionText.text = "";
            ShowUI();
        }
    }

    protected IEnumerator ShowPreInteractionText()
    {
        yield return new WaitForSeconds(0.1f);
        if (interactionText != null)
        {
            interactionText.text = preInteractionLine;
            isShowingPreText = true;
        }
        else
        {
            ShowUI();
        }
    }

    protected void ShowUI()
    {
        isInteracting = true;

        if (questionPanel != null)
        {
            questionPanel.SetActive(true);
            selectedOption = 0;
            UpdateSelectionUI();
        }
        else
        {
            Debug.LogError("Question Panel is missing! Cannot show interaction UI.");
            EndInteraction();
            return;
        }

        if (playerController != null) playerController.canMove = false;
        if (playerAnimator != null) playerAnimator.SetBool("IsMoving", false);
    }

    protected void HideUI()
    {
        if (questionPanel != null) questionPanel.SetActive(false);
    }

    protected void UpdateSelectionUI()
    {
        if (yesText != null) yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
        if (noText != null) noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
    }

    protected void HandleUIInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedOption = (selectedOption + 1) % 2;
            UpdateSelectionUI();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (selectedOption == 0)
            {
                StartCoroutine(FadeAndTeleport());
            }
            else
            {
                EndInteraction();
            }
        }
    }

    protected IEnumerator FadeAndTeleport()
    {
        if (faderCanvasGroup == null)
        {
            Debug.LogWarning("Fader Canvas Group is missing. Teleporting instantly.");
            TeleportPlayer();
            EndInteraction();
            yield break;
        }

        isFading = true;
        HideUI();

        // Mờ dần vào
        while (faderCanvasGroup.alpha < 1)
        {
            faderCanvasGroup.alpha += Time.deltaTime / fadeSpeed;
            yield return null;
        }
        faderCanvasGroup.alpha = 1;

        // Dịch chuyển
        TeleportPlayer();

        // Chờ thời gian màn hình tối hoàn toàn
        yield return new WaitForSeconds(blackScreenDuration);

        // Mờ dần ra
        while (faderCanvasGroup.alpha > 0)
        {
            faderCanvasGroup.alpha -= Time.deltaTime / fadeSpeed;
            yield return null;
        }
        faderCanvasGroup.alpha = 0;
        isFading = false;

        EndInteraction();
    }

    protected void TeleportPlayer()
    {
        GameObject destinationObject = FindObjectWithId(destinationId);
        if (playerController != null && destinationObject != null)
        {
            playerController.gameObject.transform.position = destinationObject.transform.position;
        }
        else
        {
            Debug.LogError("Teleport destination with ID '" + destinationId + "' not found or Player is null!");
        }
    }

    protected void EndInteraction()
    {
        isInteracting = false;
        isShowingPreText = false;
        HideUI();
        if (interactionText != null) interactionText.text = "";
        if (playerController != null) playerController.canMove = true;
    }

    protected GameObject FindObjectWithId(string id)
    {
        TeleportDestination[] destinations = FindObjectsOfType<TeleportDestination>();
        foreach (TeleportDestination dest in destinations)
        {
            if (dest.uniqueId == id)
            {
                return dest.gameObject;
            }
        }
        return null;
    }
}
