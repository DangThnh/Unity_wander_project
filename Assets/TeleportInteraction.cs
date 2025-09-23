using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeleportInteraction : MonoBehaviour
{
    public string myQuestion = "Do you want to teleport?";
    public string myYesText = "Yes";
    public string myNoText = "No";

    // Sử dụng ID để tìm điểm dịch chuyển
    public string destinationId;

    // Tham chiếu đến script nhân vật
    private Character_movement playerController;
    private Animator playerAnimator;

    private bool playerInRange = false;
    private int selectedOption = 0;
    private bool isInteracting = false;

    private float defaultFontSize = 36f;
    private float selectedFontSize = 48f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
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
            if (!isInteracting)
            {
                HideUI();
            }
        }
    }

    void Update()
    {
        if (isInteracting)
        {
            HandleUIInput();
        }
    }

    private IEnumerator WaitForUIReadyAndShow()
    {
        while (GameManager.instance == null || GameManager.instance.questionPanel == null)
        {
            yield return null;
        }

        if (!isInteracting)
        {
            ShowUI();
        }
    }

    void ShowUI()
    {
        isInteracting = true;

        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(true);
            selectedOption = 0;
            UpdateSelectionUI();

            if (GameManager.instance.yesText != null)
            {
                GameManager.instance.yesText.text = myYesText;
            }
            if (GameManager.instance.noText != null)
            {
                GameManager.instance.noText.text = myNoText;
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
    }

    void HideUI()
    {
        isInteracting = false;
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(false);
        }

        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }

    void UpdateSelectionUI()
    {
        if (GameManager.instance != null)
        {
            if (GameManager.instance.yesText != null)
            {
                GameManager.instance.yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
            }
            if (GameManager.instance.noText != null)
            {
                GameManager.instance.noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
            }
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
            if (selectedOption == 0) // Chọn Yes
            {
                // Tìm đối tượng đích dựa trên ID
                GameObject destinationObject = FindObjectWithId(destinationId);

                if (playerController != null && destinationObject != null)
                {
                    playerController.gameObject.transform.position = destinationObject.transform.position;
                    HideUI();
                    // Loại bỏ dòng lệnh làm đối tượng biến mất
                }
                else
                {
                    Debug.LogError("Teleport destination with ID '" + destinationId + "' not found!");
                    EndInteraction();
                }
            }
            else // Chọn No
            {
                EndInteraction();
            }
        }
    }

    void EndInteraction()
    {
        isInteracting = false;
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(false);
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