using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    // Cấu hình Inventory UI
    public GameObject inventoryPanel;
    public TextMeshProUGUI descriptionText;
    public Image[] slotImages;
    [Tooltip("Kéo thả thành phần Image của hiệu ứng viền sáng (slot effect)")]
    public Image selectedSlotEffect;
    public Color defaultEffectColor = Color.yellow;
    public Color craftingEffectColor = Color.green;

    // Cấu hình Crafting Result UI
    [Header("Crafting Result UI")]
    public GameObject resultPanel;
    public Image resultItemImage;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI continueText;

    [Header("Audio Settings")]
    [Tooltip("Gán AudioSource dùng để phát âm thanh UI vào đây")]
    public AudioSource uiAudioSource;
    [Tooltip("Kéo file âm thanh chúc mừng/thành công vào đây")]
    public AudioClip craftSuccessSound;

    public Character_movement playerController;
    public Sprite emptySlotSprite;

    // Biến trạng thái
    private int selectedSlotIndex = 0;
    private bool isCraftingMode = false;
    private int firstCraftingIndex = -1;
    private bool isShowingResult = false;

    void Start()
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (selectedSlotEffect != null)
        {
            selectedSlotEffect.color = defaultEffectColor;
            selectedSlotEffect.gameObject.SetActive(false);
        }

        // Tự động tìm AudioSource nếu chưa gán
        if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (GameState.isInputLocked) return;

        if (isShowingResult)
        {
            if (Input.GetKeyDown(KeyCode.E)) HideCraftingResult();
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            bool panelState = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(panelState);

            if (playerController != null) playerController.canMove = !panelState;

            if (panelState) UpdateUI();
            else ResetCraftingMode();
        }

        if (inventoryPanel.activeSelf)
        {
            HandleSlotSelection();
            HandleCraftingInput();
        }
    }

    void HandleSlotSelection()
    {
        int previousIndex = selectedSlotIndex;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedSlotIndex--;
            if (selectedSlotIndex < 0) selectedSlotIndex = slotImages.Length - 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedSlotIndex++;
            if (selectedSlotIndex >= slotImages.Length) selectedSlotIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            int row = selectedSlotIndex / 5;
            selectedSlotIndex = (row == 0) ? selectedSlotIndex + 5 : selectedSlotIndex - 5;
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotImages.Length - 1);
        }

        if (selectedSlotIndex != previousIndex) UpdateSlotSelectionUI();
    }

    void HandleCraftingInput()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            bool isSlotOccupied = selectedSlotIndex < InventoryManager.instance.items.Count;

            if (!isCraftingMode && isSlotOccupied)
            {
                isCraftingMode = true;
                firstCraftingIndex = selectedSlotIndex;
            }
            else if (isCraftingMode && isSlotOccupied)
            {
                if (selectedSlotIndex != firstCraftingIndex)
                {
                    Item item1 = InventoryManager.instance.items[firstCraftingIndex];
                    Item item2 = InventoryManager.instance.items[selectedSlotIndex];

                    Item resultItem = InventoryManager.instance.TryCraft(item1, item2);

                    if (resultItem != null)
                    {
                        int indexToRemove1 = firstCraftingIndex;
                        int indexToRemove2 = selectedSlotIndex;

                        InventoryManager.instance.items.RemoveAt(Mathf.Max(indexToRemove1, indexToRemove2));
                        InventoryManager.instance.items.RemoveAt(Mathf.Min(indexToRemove1, indexToRemove2));
                        InventoryManager.instance.AddItem(resultItem);

                        ShowCraftingResult(resultItem);
                    }
                    else
                    {
                        descriptionText.text = "Failed. These ones can't combine";
                        StartCoroutine(DelayResetCraftingMode(2f));
                    }

                    if (resultItem == null) ResetCraftingMode();
                }
            }
            else if (isCraftingMode && !isSlotOccupied)
            {
                ResetCraftingMode();
            }

            UpdateUI();
        }
    }

    void ResetCraftingMode()
    {
        isCraftingMode = false;
        firstCraftingIndex = -1;
        selectedSlotEffect.color = defaultEffectColor;
        UpdateSlotSelectionUI();
    }

    IEnumerator DelayResetCraftingMode(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isShowingResult)
        {
            ResetCraftingMode();
            UpdateUI();
        }
    }

    void ShowCraftingResult(Item result)
    {
        isShowingResult = true;
        inventoryPanel.SetActive(false);

        // PHÁT ÂM THANH KHI THÀNH CÔNG
        if (uiAudioSource != null && craftSuccessSound != null)
        {
            uiAudioSource.PlayOneShot(craftSuccessSound);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (playerController != null) playerController.canMove = false;
        }

        if (resultItemImage != null) resultItemImage.sprite = result.itemIcon;
        if (resultText != null) resultText.text = "Successfully crafted: " + result.itemName.ToUpper();
        if (continueText != null) continueText.text = "Press 'E' to continue.";

        ResetCraftingMode();
    }

    void HideCraftingResult()
    {
        isShowingResult = false;
        if (resultPanel != null) resultPanel.SetActive(false);
        inventoryPanel.SetActive(true);

        if (playerController != null) playerController.canMove = false;

        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i < InventoryManager.instance.items.Count)
            {
                slotImages[i].sprite = InventoryManager.instance.items[i].itemIcon;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = emptySlotSprite;
                slotImages[i].color = new Color(1, 1, 1, 0.5f);
            }

            if (isCraftingMode && i == firstCraftingIndex)
            {
                slotImages[i].color = new Color(0.5f, 0.5f, 1f);
            }
        }
        UpdateSlotSelectionUI();
    }

    void UpdateSlotSelectionUI()
    {
        if (inventoryPanel.activeSelf && !isShowingResult)
        {
            selectedSlotEffect.gameObject.SetActive(true);
            selectedSlotEffect.transform.position = slotImages[selectedSlotIndex].transform.position;
        }
        else
        {
            selectedSlotEffect.gameObject.SetActive(false);
        }

        if (isCraftingMode && selectedSlotEffect != null)
            selectedSlotEffect.color = craftingEffectColor;
        else if (selectedSlotEffect != null)
            selectedSlotEffect.color = defaultEffectColor;

        string statusMessage = "";
        if (isCraftingMode)
        {
            if (selectedSlotIndex == firstCraftingIndex) statusMessage = "Item selected. Choose another item and press 'K' to combine.";
            else if (selectedSlotIndex < InventoryManager.instance.items.Count) statusMessage = "Press 'K' to combine with the selected item.";
            else statusMessage = "Empty slot. Press 'K' here to cancel crafting";
        }
        else
        {
            if (selectedSlotIndex < InventoryManager.instance.items.Count)
            {
                Item selectedItem = InventoryManager.instance.items[selectedSlotIndex];
                statusMessage = selectedItem.itemDescription + "\n\n(Press 'K' to start combining)";
            }
            else statusMessage = "Empty slot. Press 'C' to close";
        }
        descriptionText.text = statusMessage;
    }
}