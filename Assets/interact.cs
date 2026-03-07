using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InteractableObject : MonoBehaviour
{
    // === Cài đặt ID và Trạng thái ===
    // Cần phải gán một ID duy nhất cho mỗi vật phẩm trong Inspector
    public string uniqueId;
    public string spawnActionId; // ID duy nhất cho hành động spawn này

    // === Tham chiếu UI Text ===
    [Header("UI Text Settings")]
    [Tooltip("Gán Text UI Component (TextMeshProUGUI) trực tiếp nếu không muốn dùng tham chiếu toàn cục từ GameManager.")]
    public TMP_Text localInteractionText; // TRƯỜNG MỚI ĐƯỢC THÊM VÀO
    public string myText = "My bookshelf.";
    [Tooltip("Thông báo hiển thị nếu thiếu item yêu cầu cho cả tương tác đặc biệt và nhặt item.")]
    public string requirementFailureText = "It seems you are missing a key item to proceed.";
    public string specialInteractionText = "You have something, use it.";
    public string afterInteractionText = "You put it down, would you like to use it";

    // === Cài đặt Item và Tương tác Đặc biệt ===
    [Header("Special Interaction Settings")]
    public Item itemData; // Nếu là một món đồ có thể nhặt, gán itemData vào đây

    // Cài đặt cho tương tác đặc biệt (ví dụ: mở cửa, hoặc nhặt item bằng key)
    public bool isSpecialInteraction = false;
    [Tooltip("ID của item cần có trong kho đồ để tương tác/nhặt item này.")]
    public string requiredItemId; // ID của item cần có trong kho đồ để tương tác đặc biệt hoặc nhặt item này

    // Cài đặt cho tương tác đặc biệt mới (tạo vật thể)
    public bool isSpecialSpawnInteraction = false;
    public GameObject objectToSpawnPrefab;
    public Transform spawnPoint;

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private bool isInteracting = false;
    // 0: không tương tác, 1: thoại đầu, 2: nhắc nhở nhặt item (Thành công - Không tiêu thụ requiredItemId)
    // 3: special interaction (Thành công - TIÊU THỤ requiredItemId & CÓ THỂ nhặt item)
    // 4: special spawn interaction (Thành công)
    // 5: Thông báo thất bại (Cần bấm E lần nữa để kết thúc).
    private int interactionState = 0;

    // Tham chiếu đến script nhân vật
    private Character_movement playerController;
    private Animator playerAnimator;

    // PHƯƠNG THỨC TRỢ GIÚP MỚI: Ưu tiên tham chiếu cục bộ, sau đó là tham chiếu toàn cục
    private TMP_Text GetActiveTextComponent()
    {
        if (localInteractionText != null)
        {
            return localInteractionText;
        }
        if (GameManager.instance != null)
        {
            return GameManager.instance.interactionText;
        }
        return null;
    }

    void Start()
    {
        // Kiểm tra xem vật phẩm này đã được nhặt chưa
        if (GameManager.instance != null && GameManager.instance.collectedItemIds.Contains(uniqueId))
        {
            // Nếu đã nhặt, hủy bỏ đối tượng ngay lập tức
            Destroy(gameObject);
            return;
        }

        // Kiểm tra xem hành động spawn đã hoàn thành chưa
        if (GameManager.instance != null && GameManager.instance.completedSpawnActions.Contains(spawnActionId))
        {
            // Nếu đã spawn, hủy đối tượng ban đầu để tránh trùng lặp
            Destroy(gameObject);
            return;
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

        TMP_Text textComponent = GetActiveTextComponent();

        // Hiển thị text đầu tiên
        if (textComponent != null)
        {
            textComponent.gameObject.SetActive(true);
            textComponent.text = myText;
            // Giữ lại debug log để kiểm tra tham chiếu
            Debug.Log($"Interactable '{gameObject.name}' is setting text on: {textComponent.gameObject.name}");
        }
        else
        {
            Debug.LogError("Interaction Text Component is missing!");
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
        TMP_Text textComponent = GetActiveTextComponent();

        // Xử lý tương tác nhặt đồ (có itemData và KHÔNG phải tương tác đặc biệt khác)
        if (itemData != null && !isSpecialInteraction && !isSpecialSpawnInteraction)
        {
            // Lần nhấn E thứ hai (trạng thái 1)
            if (interactionState == 1)
            {
                // Kiểm tra xem có yêu cầu item không
                bool isRequired = !string.IsNullOrEmpty(requiredItemId);
                // Kiểm tra xem người chơi có item yêu cầu không (hoặc không cần item)
                bool hasRequiredItem = isRequired ? InventoryManager.instance.HasItem(requiredItemId) : true;

                if (hasRequiredItem)
                {
                    // THÀNH CÔNG: Chuyển sang nhắc nhở nhặt item (state 2) - KHÔNG TIÊU THỤ item kích hoạt
                    interactionState = 2;
                    if (textComponent != null)
                    {
                        // Hiển thị itemData.itemName thay vì requiredItemId
                        textComponent.text = "Do you want to take this " + itemData.itemName + "? (Press E to take)";
                    }
                }
                else
                {
                    // THẤT BẠI: Chuyển sang trạng thái thông báo thất bại (state 5)
                    interactionState = 5;
                    if (textComponent != null)
                    {
                        textComponent.text = requirementFailureText;
                    }
                }
            }
            // Lần nhấn E thứ ba (trạng thái 2 - Thành công)
            else if (interactionState == 2)
            {
                // Hành động nhặt đồ (Không xóa requiredItemId)
                InventoryManager.instance.AddItem(itemData);
                GameManager.instance.collectedItemIds.Add(uniqueId);

                Destroy(gameObject);
                EndInteraction();
            }
            // Lần nhấn E thứ ba (trạng thái 5 - Thất bại)
            else if (interactionState == 5)
            {
                // Nhấn E để kết thúc sau khi xem thông báo thất bại
                EndInteraction();
            }
        }
        // Xử lý tương tác đặc biệt (TIÊU THỤ requiredItemId, CÓ THỂ nhặt itemData HOẶC chỉ kích hoạt)
        else if (isSpecialInteraction && !isSpecialSpawnInteraction)
        {
            if (interactionState == 1)
            {
                bool hasRequiredItem = InventoryManager.instance.HasItem(requiredItemId);
                if (hasRequiredItem)
                {
                    interactionState = 3;
                    if (textComponent != null)
                    {
                        // Hiển thị text đặc biệt, có thể đề cập đến việc sử dụng item
                        textComponent.text = specialInteractionText;
                    }
                }
                else
                {
                    // Thất bại: Chuyển sang trạng thái thông báo thất bại (state 5)
                    interactionState = 5;
                    if (textComponent != null)
                    {
                        textComponent.text = requirementFailureText;
                    }
                }
            }
            else if (interactionState == 3)
            {
                // 1. LUÔN LUÔN xóa item kích hoạt (requiredItemId)
                InventoryManager.instance.RemoveItem(requiredItemId);

                // 2. Nhận item mới (itemData) nếu nó được gán cho vật thể
                if (itemData != null)
                {
                    InventoryManager.instance.AddItem(itemData);
                }

                // 3. Đánh dấu đã hoàn thành và hủy đối tượng
                GameManager.instance.collectedItemIds.Add(uniqueId);
                Destroy(gameObject);
                EndInteraction();
            }
            // Thêm xử lý để kết thúc sau khi xem thông báo thất bại
            else if (interactionState == 5)
            {
                EndInteraction();
            }
        }
        // Xử lý tương tác đặc biệt (tạo vật thể)
        else if (isSpecialSpawnInteraction)
        {
            // Kiểm tra lần nhấn E thứ hai để hiển thị afterInteractionText
            if (interactionState == 1)
            {
                bool hasRequiredItem = InventoryManager.instance.HasItem(requiredItemId);
                if (hasRequiredItem)
                {
                    interactionState = 4;
                    if (textComponent != null)
                    {
                        textComponent.text = afterInteractionText;
                    }
                }
                else
                {
                    // Thất bại: Chuyển sang trạng thái thông báo thất bại (state 5)
                    interactionState = 5;
                    if (textComponent != null)
                    {
                        textComponent.text = requirementFailureText;
                    }
                }
            }
            // Kiểm tra lần nhấn E thứ ba để tạo vật thể
            else if (interactionState == 4)
            {
                if (objectToSpawnPrefab != null && spawnPoint != null)
                {
                    GameObject spawnedObject = Instantiate(objectToSpawnPrefab, spawnPoint.position, spawnPoint.rotation);
                    // Giữ lại vật thể đã được sinh ra
                    //DontDestroyOnLoad(spawnedObject);
                    // Đánh dấu hành động spawn đã hoàn thành
                    GameManager.instance.completedSpawnActions.Add(spawnActionId);
                }
                InventoryManager.instance.RemoveItem(requiredItemId);
                EndInteraction();
            }
            // Thêm xử lý để kết thúc sau khi xem thông báo thất bại
            else if (interactionState == 5)
            {
                EndInteraction();
            }
            else
            {
                EndInteraction();
            }
        }
        // Nếu không phải vật phẩm hoặc đã ở trạng thái cuối, kết thúc tương tác
        else
        {
            EndInteraction();
        }
    }

    void EndInteraction()
    {
        isInteracting = false;
        interactionState = 0;

        TMP_Text textComponent = GetActiveTextComponent();

        // Tắt text UI
        if (textComponent != null)
        {
            textComponent.gameObject.SetActive(false);
        }

        // Mở khóa chuyển động nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }

    }
    void OnEnable()
    {
        HexaPuzzleManager.OnPuzzleCompleted += ForceTextRefresh;
    }

    void OnDisable()
    {
        HexaPuzzleManager.OnPuzzleCompleted -= ForceTextRefresh;
    }

    // Phương thức này sẽ được gọi khi câu đố kết thúc
    void ForceTextRefresh()
    {
        TMP_Text textComponent = GetActiveTextComponent();

        // Chạy lại logic kiểm tra OnTriggerEnter/OnTriggerStay của vật thể này
        if (playerInRange)
        {
            // Cập nhật text UI ngay lập tức
            if (textComponent != null)
            {
                textComponent.text = myText; // myText của đối tượng tương tác này
                textComponent.gameObject.SetActive(true);
            }
        }
    }

}