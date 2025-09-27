using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Cần thiết cho Coroutine

public class InventoryUI : MonoBehaviour
{
    // Cấu hình Inventory UI
    public GameObject inventoryPanel;
    public TextMeshProUGUI descriptionText;
    public Image[] slotImages; // Gán 10 Image của các slot vào đây
    [Tooltip("Kéo thả thành phần Image của hiệu ứng viền sáng (slot effect)")]
    public Image selectedSlotEffect; // PHẢI LÀ IMAGE để có thể thay đổi màu
    public Color defaultEffectColor = Color.yellow; // Màu mặc định (Vàng)
    public Color craftingEffectColor = Color.green; // Màu khi ở chế độ kết hợp (Xanh lá)

    // Cấu hình Crafting Result UI
    [Header("Crafting Result UI")]
    [Tooltip("Panel phải là một GameObject ở giữa màn hình")]
    public GameObject resultPanel;
    public Image resultItemImage;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI continueText; // "Press 'E' to continue"

    public Character_movement playerController; // Tham chiếu đến script điều khiển nhân vật
    public Sprite emptySlotSprite; // Hình ảnh mặc định cho slot trống

    // Biến trạng thái
    private int selectedSlotIndex = 0;
    private bool isCraftingMode = false;
    private int firstCraftingIndex = -1; // -1: Chưa chọn, >=0: Đã chọn slot đầu tiên
    private bool isShowingResult = false; // Trạng thái đang hiển thị kết quả

    void Start()
    {
        // Ẩn panel kết quả ngay từ đầu
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // Thiết lập màu mặc định cho hiệu ứng slot
        if (selectedSlotEffect != null)
        {
            selectedSlotEffect.color = defaultEffectColor;
            selectedSlotEffect.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Xử lý khi đang hiển thị kết quả chế tạo (ƯU TIÊN HÀNG ĐẦU)
        if (isShowingResult)
        {
            // Xử lý khi đang xem kết quả (Nhấn E)
            if (Input.GetKeyDown(KeyCode.E))
            {
                HideCraftingResult();
            }
            // Không xử lý bất kỳ input nào khác khi đang xem kết quả
            return;
        }

        // 2. Xử lý Bật/Tắt Inventory (Phím C)
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Vì isShowingResult đã được xử lý ở trên, chúng ta có thể bỏ qua check ở đây
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
            else
            {
                // Nếu đóng inventory, hủy chế độ kết hợp
                ResetCraftingMode();
            }
        }

        // 3. Xử lý di chuyển và kết hợp (Chỉ khi Inventory đang bật)
        if (inventoryPanel.activeSelf)
        {
            HandleSlotSelection();
            HandleCraftingInput();
        }
    }

    void HandleSlotSelection()
    {
        int previousIndex = selectedSlotIndex;

        // Xử lý di chuyển bằng phím mũi tên
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
            // Giả định 2 hàng 5 cột
            int row = selectedSlotIndex / 5;
            selectedSlotIndex = (row == 0) ? selectedSlotIndex + 5 : selectedSlotIndex - 5;
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotImages.Length - 1);
        }

        if (selectedSlotIndex != previousIndex)
        {
            UpdateSlotSelectionUI();
        }
    }

    void HandleCraftingInput()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            // Kiểm tra slot hiện tại có item không
            bool isSlotOccupied = selectedSlotIndex < InventoryManager.instance.items.Count;

            if (!isCraftingMode && isSlotOccupied)
            {
                // 1. Bắt đầu chế độ kết hợp: Chọn item thứ nhất
                isCraftingMode = true;
                firstCraftingIndex = selectedSlotIndex;
            }
            else if (isCraftingMode && isSlotOccupied)
            {
                // 2. Thực hiện kết hợp: Chọn item thứ hai
                if (selectedSlotIndex != firstCraftingIndex)
                {
                    // Lấy 2 item để kết hợp
                    Item item1 = InventoryManager.instance.items[firstCraftingIndex];
                    Item item2 = InventoryManager.instance.items[selectedSlotIndex];

                    Item resultItem = InventoryManager.instance.TryCraft(item1, item2);

                    if (resultItem != null)
                    {
                        // Thành công: Xóa item cũ và thêm item mới
                        int indexToRemove1 = firstCraftingIndex;
                        int indexToRemove2 = selectedSlotIndex;

                        // Xóa item tại index lớn hơn trước để tránh sai lệch index khi RemoveAt
                        // Chúng ta sẽ dùng RemoveAt để xóa theo vị trí slot đã chọn
                        InventoryManager.instance.items.RemoveAt(Mathf.Max(indexToRemove1, indexToRemove2));
                        InventoryManager.instance.items.RemoveAt(Mathf.Min(indexToRemove1, indexToRemove2));

                        // Thêm item mới
                        InventoryManager.instance.AddItem(resultItem);

                        // Hiển thị kết quả
                        ShowCraftingResult(resultItem);
                    }
                    else
                    {
                        // Thất bại: Thông báo và hủy chế độ sau 2 giây
                        descriptionText.text = "Failed. These ones can't combine";
                        StartCoroutine(DelayResetCraftingMode(2f));
                    }

                    // ResetCraftingMode() sẽ được gọi trong UpdateUI sau khi xử lý. 
                    // Hoặc chúng ta có thể gọi nó ở đây để đảm bảo trạng thái reset ngay lập tức, 
                    // nhưng nên để nó ở cuối quá trình xử lý hoặc trong DelayResetCraftingMode
                    if (resultItem == null)
                    {
                        ResetCraftingMode();
                    }
                }
            }
            else if (isCraftingMode && !isSlotOccupied)
            {
                // 3. Hủy chế độ kết hợp: Nhấn K vào slot trống
                ResetCraftingMode();
            }

            // Cập nhật lại UI sau khi xử lý K
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
        // Kiểm tra nếu không có result panel đang hiển thị thì mới reset.
        if (!isShowingResult)
        {
            ResetCraftingMode();
            UpdateUI();
        }
    }

    void ShowCraftingResult(Item result)
    {
        isShowingResult = true;
        inventoryPanel.SetActive(false); // Ẩn inventory chính

        // Đặt panel kết quả lên trên cùng
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            // Đảm bảo panel kết quả luôn khóa nhân vật
            if (playerController != null)
            {
                playerController.canMove = false;
            }
        }

        if (resultItemImage != null)
        {
            resultItemImage.sprite = result.itemIcon;
        }
        if (resultText != null)
        {
            // Dòng chữ đã được dịch sang tiếng Anh trong bản cập nhật trước, giữ nguyên
            resultText.text = "Succesfully crafted: " + result.itemName.ToUpper();
        }
        if (continueText != null)
        {
            continueText.text = "Press 'E' to continue.";
        }

        // Cần reset chế độ crafting ngay lập tức để khi thoát result screen, 
        // inventory trở về trạng thái bình thường.
        ResetCraftingMode();
    }

    void HideCraftingResult()
    {
        isShowingResult = false;

        if (resultPanel != null)
        {
            resultPanel.SetActive(false); // Ẩn panel kết quả
        }

        inventoryPanel.SetActive(true); // Hiện lại inventory chính

        // Mở khóa chuyển động nhân vật về trạng thái khóa (vì Inventory vẫn đang mở)
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        UpdateUI();
        // Không cần gọi ResetCraftingMode() ở đây nữa vì nó đã được gọi trong ShowCraftingResult
    }

    void UpdateUI()
    {
        // Cập nhật Sprite và màu sắc của các slot
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

            // Đánh dấu slot đã chọn đầu tiên bằng màu xanh nhạt
            if (isCraftingMode && i == firstCraftingIndex)
            {
                slotImages[i].color = new Color(0.5f, 0.5f, 1f);
            }
        }

        UpdateSlotSelectionUI();
    }

    void UpdateSlotSelectionUI()
    {
        // Hiệu ứng viền sáng chỉ hiển thị khi inventory đang mở VÀ không xem kết quả
        if (inventoryPanel.activeSelf && !isShowingResult)
        {
            selectedSlotEffect.gameObject.SetActive(true);
            selectedSlotEffect.transform.position = slotImages[selectedSlotIndex].transform.position;
        }
        else
        {
            selectedSlotEffect.gameObject.SetActive(false);
        }

        // Điều chỉnh màu sắc của hiệu ứng viền sáng
        if (isCraftingMode && selectedSlotEffect != null)
        {
            selectedSlotEffect.color = craftingEffectColor; // Màu Xanh Lá
        }
        else if (selectedSlotEffect != null)
        {
            selectedSlotEffect.color = defaultEffectColor; // Màu Vàng mặc định
        }

        // Cập nhật mô tả text
        string statusMessage = "";
        if (isCraftingMode)
        {
            if (selectedSlotIndex == firstCraftingIndex)
            {
                statusMessage = "Item selected. Choose another item and press 'K' to combine.";
            }
            else if (selectedSlotIndex < InventoryManager.instance.items.Count)
            {
                statusMessage = "Press 'K' to combine with the selected item.";
            }
            else // Slot trống trong chế độ crafting
            {
                statusMessage = "Empty slot. Press 'K' here to cancel crafting";
            }
        }
        else // Chế độ bình thường
        {
            if (selectedSlotIndex < InventoryManager.instance.items.Count)
            {
                Item selectedItem = InventoryManager.instance.items[selectedSlotIndex];
                statusMessage = selectedItem.itemDescription;
                statusMessage += "\n\n(Press 'K' to start combining)";
            }
            else
            {
                statusMessage = "Empty slot. Press 'C' to close";
            }
        }

        descriptionText.text = statusMessage;
    }
}
