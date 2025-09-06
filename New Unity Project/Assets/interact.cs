using UnityEngine;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    // Cần phải gán một ID duy nhất cho mỗi vật phẩm trong Inspector
    // Ví dụ: "sach_cu", "chia_khoa", v.v.
    public string uniqueId;

    // Cài đặt cho tương tác đặc biệt
    public bool isSpecialInteraction = false;
    public string requiredItemId; // ID của item cần có trong kho đồ để tương tác đặc biệt
    public string specialInteractionText = "Bạn đã có một chiếc chìa khóa, hãy dùng nó.";

    // Cài đặt UI và dữ liệu
    public Item itemData; // Nếu là một món đồ có thể nhặt, gán itemData vào đây
    public TextMeshProUGUI interactionText;
    public string myText = "Đây là tủ sách của tôi, mặc dù tôi không hay đọc sách lắm.";

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private bool isInteracting = false;
    private int interactionState = 0; // 0: không tương tác, 1: thoại đầu, 2: thoại thứ hai

    // Tham chiếu đến script nhân vật
    private Character_movement playerController;
    private Animator playerAnimator;

    void Start()
    {
        // Kiểm tra xem vật phẩm này đã được nhặt chưa
        if (GameManager.instance.collectedItemIds.Contains(uniqueId))
        {
            // Nếu đã nhặt, hủy bỏ đối tượng ngay lập tức
            Destroy(gameObject);
            return;
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();
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

    void Update()
    {
        // Chỉ tương tác khi nhân vật ở trong vùng và bấm E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteracting)
            {
                // Bắt đầu quá trình tương tác
                StartInteraction();
            }
            else
            {
                // Tiếp tục/kết thúc tương tác dựa vào trạng thái hiện tại
                ContinueInteraction();
            }
        }
    }

    void StartInteraction()
    {
        isInteracting = true;
        interactionState = 1;

        // Hiển thị text đầu tiên
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = myText;
        }

        // Khóa chuyển động nhân vật
        if (playerController != null)
        {
            playerController.canMove = false;
        }
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMoving", false);
        }
    }

    void ContinueInteraction()
    {
        // Nếu là vật phẩm và đang ở trạng thái 1, chuyển sang trạng thái 2
        if (itemData != null && interactionState == 1)
        {
            interactionState = 2;
            if (interactionText != null)
            {
                interactionText.text = "Do you want to take" + itemData.itemName + "? (Press E to take)";
            }
        }
        // Nếu đang ở trạng thái 2 (với vật phẩm), thực hiện nhặt đồ
        else if (itemData != null && interactionState == 2)
        {
            InventoryManager.instance.AddItem(itemData);

            // Thêm ID của vật phẩm vào danh sách đã nhặt
            GameManager.instance.collectedItemIds.Add(uniqueId);

            Destroy(gameObject); // Hoặc gameObject.SetActive(false);
            EndInteraction();
        }
        // Xử lý tương tác đặc biệt
        else if (isSpecialInteraction)
        {
            if (interactionState == 1)
            {
                // Kiểm tra xem người chơi có item cần thiết không
                bool hasRequiredItem = InventoryManager.instance.HasItem(requiredItemId);
                if (hasRequiredItem)
                {
                    interactionState = 3;
                    if (interactionText != null)
                    {
                        interactionText.text = specialInteractionText;
                    }
                }
                else
                {
                    // Nếu không có item, kết thúc tương tác
                    EndInteraction();
                }
            }
            else if (interactionState == 3)
            {
                // Thực hiện hành động đặc biệt
                Debug.Log("Sử dụng item và thực hiện hành động đặc biệt!");
                // Ví dụ: Mở khóa cửa
                Destroy(gameObject); // Hủy đối tượng cửa sau khi mở khóa
                EndInteraction();
            }
        }
        // Nếu không phải vật phẩm hoặc đã ở trạng thái 2, kết thúc tương tác
        else
        {
            EndInteraction();
        }
    }

    void EndInteraction()
    {
        isInteracting = false;
        interactionState = 0;

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        // Mở khóa chuyển động nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }
}
