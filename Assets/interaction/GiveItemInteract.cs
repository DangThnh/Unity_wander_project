using UnityEngine;
using TMPro;

public class ItemGiver : MonoBehaviour
{
    // Kéo và thả TextMeshProUGUI từ Inspector vào đây
    public TextMeshProUGUI interactionText;

    // Item to give to the player
    public Item itemToGive;

    // Yêu cầu vật phẩm để tương tác
    public bool requiresItemCondition = false;
    public string requiredItemId;

    // Dòng text nếu không có item yêu cầu
    public string requiredItemFailureLine = "Thật tiếc, bạn không có chìa khóa để mở nó.";

    // The first line of dialogue when interacting
    public string firstLine = "Khoan, hình như có gì đó...";

    // The second line of dialogue, shown before giving the item
    public string secondLine = "Bạn đã nhận được một vật phẩm, hãy kiểm tra kho đồ!";

    // The third line of dialogue to confirm the item has been added
    public string thirdLine = "Bạn đã nhận được một vật phẩm, hãy kiểm tra kho đồ!";

    private bool playerInRange = false;
    private bool hasGivenItem = false;
    private int interactionStep = 0;

    void Start()
    {
        // Check if the item has already been given on scene load
        if (InventoryManager.instance != null && itemToGive != null && InventoryManager.instance.HasItem(itemToGive.uniqueId))
        {
            hasGivenItem = true;
        }

        if (interactionText != null)
        {
            interactionText.text = "";
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!hasGivenItem)
            {
                HandleFirstInteractionSequence();
            }
            else
            {
                HandleSubsequentInteraction();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            EndInteraction();
        }
    }

    private void HandleFirstInteractionSequence()
    {
        // Kiểm tra điều kiện cần item nếu được bật
        if (requiresItemCondition && !InventoryManager.instance.HasItem(requiredItemId))
        {
            if (interactionText != null)
            {
                interactionText.text = requiredItemFailureLine;
            }
            // Không đánh dấu đã tương tác để người chơi có thể thử lại sau khi có vật phẩm
            return;
        }

        // Thực hiện tương tác bình thường nếu không có điều kiện hoặc điều kiện đã thỏa mãn
        switch (interactionStep)
        {
            case 0:
                if (interactionText != null)
                {
                    interactionText.text = firstLine;
                }
                interactionStep = 1;
                break;
            case 1:
                if (interactionText != null)
                {
                    interactionText.text = secondLine;
                }
                interactionStep = 2;
                break;
            case 2:
                if (interactionText != null)
                {
                    // Cập nhật text với tên vật phẩm
                    interactionText.text = thirdLine.Replace("vật phẩm", $"'{itemToGive.itemName}'");
                }

                if (InventoryManager.instance != null && itemToGive != null)
                {
                    InventoryManager.instance.AddItem(itemToGive);
                    hasGivenItem = true;
                    Debug.Log($"Item '{itemToGive.itemName}' has been given to the player.");

                    // Nếu tương tác yêu cầu vật phẩm, xóa vật phẩm đó khỏi kho đồ
                    if (requiresItemCondition)
                    {
                        InventoryManager.instance.RemoveItem(requiredItemId);
                        Debug.Log($"Required item '{requiredItemId}' has been removed from inventory.");
                    }
                }

                interactionStep = 3;
                break;
            case 3:
                EndInteraction();
                break;
        }
    }

    private void HandleSubsequentInteraction()
    {
        switch (interactionStep)
        {
            case 0:
                if (interactionText != null)
                {
                    interactionText.text = firstLine;
                }
                interactionStep = 1;
                break;
            case 1:
                EndInteraction();
                break;
        }
    }

    private void EndInteraction()
    {
        if (interactionText != null)
        {
            interactionText.text = "";
        }
        interactionStep = 0;
    }
}