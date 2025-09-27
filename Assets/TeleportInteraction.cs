using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeleportInteraction : MonoBehaviour
{
    // Cài đặt cho lời thoại trước khi tương tác
    [Tooltip("Dòng chữ hiển thị trước khi bảng câu hỏi hiện ra. Nếu để trống, bảng câu hỏi sẽ hiện ngay lập tức.")]
    public string preInteractionLine;
    public TextMeshProUGUI interactionText;

    // Cài đặt cho bảng câu hỏi tùy chỉnh
    [Tooltip("Kéo bảng UI chứa câu hỏi và lựa chọn vào đây.")]
    public GameObject questionPanel;
    [Tooltip("Kéo TextMeshProUGUI của câu hỏi vào đây.")]
    public TextMeshProUGUI questionText;
    [Tooltip("Kéo TextMeshProUGUI của lựa chọn 'Có' vào đây.")]
    public TextMeshProUGUI yesText;
    [Tooltip("Kéo TextMeshProUGUI của lựa chọn 'Không' vào đây.")]
    public TextMeshProUGUI noText;

    public string myQuestion = "Bạn có muốn dịch chuyển không?";
    public string myYesText = "Có";
    public string myNoText = "Không";

    // Cài đặt cho hiệu ứng mờ màn hình
    [Tooltip("Kéo Canvas Group của đối tượng làm mờ màn hình vào đây. Đối tượng này nên là một Panel màu đen.")]
    public CanvasGroup faderCanvasGroup;
    public float fadeSpeed = 1f;
    public float blackScreenDuration = 1f; // Thời gian màn hình tối hoàn toàn

    // Sử dụng ID để tìm điểm dịch chuyển
    public string destinationId;

    // Tham chiếu đến script nhân vật
    private Character_movement playerController;
    private Animator playerAnimator;

    private bool playerInRange = false;
    private int selectedOption = 0;
    private bool isInteracting = false;
    private bool isShowingPreText = false;
    private bool isFading = false;

    private float defaultFontSize = 36f;
    private float selectedFontSize = 48f;

    void Start()
    {
        if (questionText != null) questionText.text = myQuestion;
        if (yesText != null) yesText.text = myYesText;
        if (noText != null) noText.text = myNoText;

        HideUI();
        if (interactionText != null) interactionText.text = "";
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();

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

    void OnTriggerExit(Collider other)
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

    void Update()
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

    private IEnumerator ShowPreInteractionText()
    {
        // Chờ để tránh hiện text quá nhanh
        yield return new WaitForSeconds(0.1f);

        if (interactionText != null)
        {
            interactionText.text = preInteractionLine;
            isShowingPreText = true;
        }
        else
        {
            // Nếu không có interactionText, hiện UI ngay
            ShowUI();
        }
    }

    void ShowUI()
    {
        isInteracting = true;

        if (questionPanel != null)
        {
            questionPanel.SetActive(true);
            selectedOption = 0;
            UpdateSelectionUI();
        }

        if (playerController != null)
        {
            playerController.canMove = false;
        }
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMoving", false);
        }
    }

    void HideUI()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }
    }

    void UpdateSelectionUI()
    {
        if (yesText != null)
        {
            yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
        }
        if (noText != null)
        {
            noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
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
            if (selectedOption == 0) // Chọn Có
            {
                StartCoroutine(FadeAndTeleport());
            }
            else // Chọn Không
            {
                EndInteraction();
            }
        }
    }

    private IEnumerator FadeAndTeleport()
    {
        if (faderCanvasGroup == null)
        {
            Debug.LogError("Fader Canvas Group is not assigned. Teleporting instantly.");
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

    private void TeleportPlayer()
    {
        GameObject destinationObject = FindObjectWithId(destinationId);
        if (playerController != null && destinationObject != null)
        {
            playerController.gameObject.transform.position = destinationObject.transform.position;
        }
        else
        {
            Debug.LogError("Teleport destination with ID '" + destinationId + "' not found!");
        }
    }

    void EndInteraction()
    {
        isInteracting = false;
        isShowingPreText = false;
        HideUI();
        if (interactionText != null)
        {
            interactionText.text = "";
        }
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }

    // Phương thức để tìm đối tượng đích
    private GameObject FindObjectWithId(string id)
    {
        // Tìm tất cả các đối tượng có script TeleportDestination
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