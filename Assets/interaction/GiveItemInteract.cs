using UnityEngine;
using TMPro;

public class ItemGiver : MonoBehaviour
{
    // Kéo và thả TextMeshProUGUI từ Inspector vào đây
    public TextMeshProUGUI interactionText;

    // Item to give to the player
    public Item itemToGive;

    // === Cài đặt Âm thanh (Mới) ===
    [Header("Audio Settings")]
    [Tooltip("AudioSource dùng để phát âm thanh. Nếu để trống, sẽ tự tìm trên object này.")]
    public AudioSource audioSource;
    [Tooltip("Âm thanh phát ra khi nhận được item mới.")]
    public AudioClip pickupSound;

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
        // Tự động tìm AudioSource nếu chưa được gán
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

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

    // Hàm hỗ trợ phát âm thanh
    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            else
            {
                // Phát âm thanh tại vị trí vật thể nếu không có AudioSource
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
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
            return;
        }

        switch (interactionStep)
        {
            case 0:
                if (interactionText != null) interactionText.text = firstLine;
                interactionStep = 1;
                break;
            case 1:
                if (interactionText != null) interactionText.text = secondLine;
                interactionStep = 2;
                break;
            case 2:
                if (interactionText != null)
                {
                    interactionText.text = thirdLine.Replace("vật phẩm", $"'{itemToGive.itemName}'");
                }

                if (InventoryManager.instance != null && itemToGive != null)
                {
                    // THỰC HIỆN NHẬN ITEM VÀ PHÁT ÂM THANH
                    InventoryManager.instance.AddItem(itemToGive);
                    PlayPickupSound(); // <--- PHÁT ÂM THANH TẠI ĐÂY

                    hasGivenItem = true;
                    Debug.Log($"Item '{itemToGive.itemName}' has been given to the player.");

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
                if (interactionText != null) interactionText.text = firstLine;
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