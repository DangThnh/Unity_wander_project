using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public TextMeshProUGUI descriptionText;
    public Image[] slotImages; // Gán 10 Image của các slot vào đây
    public GameObject selectedSlotEffect; // Hiệu ứng viền sáng

    public Character_movement playerController; // Tham chiếu đến script điều khiển nhân vật
    public Sprite emptySlotSprite; // Hình ảnh mặc định cho slot trống

    private int selectedSlotIndex = 0;

    void Update()
    {
        // Bật/tắt menu
        if (Input.GetKeyDown(KeyCode.C))
        {
            bool panelState = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(panelState);

            // Khóa/mở chuyển động nhân vật
            if (playerController != null)
            {
                playerController.canMove = !panelState;
            }

            if (panelState)
            {
                UpdateUI();
            }
        }

        // Chỉ xử lý input khi menu đang bật
        if (inventoryPanel.activeSelf)
        {
            HandleSlotSelection();
        }
    }

    void HandleSlotSelection()
    {
        int previousIndex = selectedSlotIndex;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedSlotIndex--;
            // Di chuyển vòng tròn sang hàng khác nếu cần
            if (selectedSlotIndex < 0)
            {
                selectedSlotIndex = slotImages.Length - 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedSlotIndex++;
            if (selectedSlotIndex >= slotImages.Length)
            {
                selectedSlotIndex = 0;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Di chuyển lên/xuống giữa 2 hàng
            int row = selectedSlotIndex / 5;
            selectedSlotIndex = (row == 0) ? selectedSlotIndex + 5 : selectedSlotIndex - 5;
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotImages.Length - 1);
        }

        if (selectedSlotIndex != previousIndex)
        {
            UpdateSlotSelectionUI();
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            // Luôn hiển thị các slot, chỉ thay đổi sprite
            if (i < InventoryManager.instance.items.Count)
            {
                slotImages[i].sprite = InventoryManager.instance.items[i].itemIcon;
            }
            else
            {
                slotImages[i].sprite = emptySlotSprite;
            }
        }

        UpdateSlotSelectionUI();
    }

    void UpdateSlotSelectionUI()
    {
        // Luôn hiển thị hiệu ứng viền sáng trên slot được chọn
        selectedSlotEffect.SetActive(true);
        selectedSlotEffect.transform.position = slotImages[selectedSlotIndex].transform.position;

        // Cập nhật mô tả chỉ khi slot có item
        if (selectedSlotIndex < InventoryManager.instance.items.Count)
        {
            descriptionText.text = InventoryManager.instance.items[selectedSlotIndex].itemDescription;
        }
        else
        {
            descriptionText.text = "";
        }
    }
}