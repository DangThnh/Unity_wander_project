using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class InteractableObjectEndCredit : MonoBehaviour
{
    // === Cần phải gán trong Inspector (Thêm UIManager vào đây) ===
    [Header("Manager References")]
    [Tooltip("Kéo UIManager Component vào đây để thực hiện hiệu ứng Fade.")]
    public UIManager uiManager;

    // Cần phải gán một ID duy nhất cho mỗi vật phẩm trong Inspector
    public string uniqueId;
    public string spawnActionId; // ID duy nhất cho hành động spawn này

    // Cài đặt cho tương tác đặc biệt (ví dụ: mở cửa, hoặc nhặt item bằng key)
    [Header("Special Interaction Settings")]
    public bool isSpecialInteraction = false;
    [Tooltip("ID của item cần có trong kho đồ để tương tác/nhặt item này.")]
    public string requiredItemId;
    public string specialInteractionText = "You have something, use it.";
    [Tooltip("Thông báo hiển thị nếu thiếu item yêu cầu cho cả tương tác đặc biệt và nhặt item.")]
    public string requirementFailureText = "It seems you are missing a key item to proceed.";

    // Yêu cầu mới: Cài đặt chuyển cảnh
    [Tooltip("Đánh dấu để kích hoạt chuyển Scene 6 sau khi tương tác thành công.")]
    public bool triggersSceneChange = false;
    [Tooltip("Index của Scene cần chuyển đến sau khi tương tác đặc biệt thành công.")]
    public int targetSceneIndex = 6;
    [Tooltip("Thời gian (giây) để màn hình tối dần trước khi chuyển Scene.")]
    public float fadeOutDuration = 1.5f;


    // Cài đặt cho tương tác đặc biệt mới (tạo vật thể)
    [Header("Spawn Interaction Settings")]
    public bool isSpecialSpawnInteraction = false;
    public GameObject objectToSpawnPrefab;
    public Transform spawnPoint;
    public string afterInteractionText = "You put it down, would you like to use it";

    // Cài đặt UI và dữ liệu
    [Header("Item Data")]
    public Item itemData; // Nếu là một món đồ có thể nhặt, gán itemData vào đây
    public string myText = "My bookshelf.";

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private bool isInteracting = false;
    // 0: không tương tác, 1: thoại đầu, 2: nhắc nhở nhặt item (Thành công - Không tiêu thụ requiredItemId)
    // 3: special interaction (Thành công - TIÊU THỤ requiredItemId, CÓ THỂ nhặt item, CÓ THỂ chuyển Scene)
    // 4: special spawn interaction (Thành công)
    // 5: Thông báo thất bại (Cần bấm E lần nữa để kết thúc).
    private int interactionState = 0;

    // Tham chiếu đến script nhân vật (Giả định Character_movement tồn tại)
    private Character_movement playerController;
    private Animator playerAnimator;

    void Start()
    {
        // Kiểm tra vật phẩm đã nhặt hoặc hành động spawn đã hoàn thành chưa
        // (Giả định GameManager và InventoryManager là Singleton và tồn tại)
        if (GameManager.instance.collectedItemIds.Contains(uniqueId))
        {
            Destroy(gameObject);
            return;
        }

        if (GameManager.instance.completedSpawnActions.Contains(spawnActionId))
        {
            Destroy(gameObject);
            return;
        }

        // Kiểm tra UIManager cần thiết cho việc Fade Out
        if (triggersSceneChange && uiManager == null)
        {
            Debug.LogError("InteractableObject: UIManager bị thiếu! Không thể thực hiện hiệu ứng Fade Out và chuyển Scene.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Lấy component từ nhân vật
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
                StartInteraction();
            }
            else
            {
                ContinueInteraction();
            }
        }
    }

    void StartInteraction()
    {
        isInteracting = true;
        interactionState = 1;

        // Hiển thị text đầu tiên
        if (GameManager.instance != null && GameManager.instance.interactionText != null)
        {
            GameManager.instance.interactionText.gameObject.SetActive(true);
            GameManager.instance.interactionText.text = myText;
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
        // Xử lý tương tác nhặt đồ (state 2) và thông báo thất bại (state 5)
        if (itemData != null && !isSpecialInteraction && !isSpecialSpawnInteraction)
        {
            if (interactionState == 1)
            {
                bool isRequired = !string.IsNullOrEmpty(requiredItemId);
                bool hasRequiredItem = isRequired ? InventoryManager.instance.HasItem(requiredItemId) : true;

                if (hasRequiredItem)
                {
                    interactionState = 2;
                    if (GameManager.instance != null && GameManager.instance.interactionText != null)
                    {
                        GameManager.instance.interactionText.text = "Do you want to take this " + itemData.itemName + "? (Press E to take)";
                    }
                }
                else
                {
                    interactionState = 5;
                    if (GameManager.instance != null && GameManager.instance.interactionText != null)
                    {
                        GameManager.instance.interactionText.text = requirementFailureText;
                    }
                }
            }
            else if (interactionState == 2)
            {
                // Nhặt đồ
                InventoryManager.instance.AddItem(itemData);
                GameManager.instance.collectedItemIds.Add(uniqueId);
                Destroy(gameObject);
                EndInteraction();
            }
            else if (interactionState == 5)
            {
                EndInteraction();
            }
        }
        // Xử lý tương tác đặc biệt (Special Interaction) VÀ KÍCH HOẠT CHUYỂN SCENE
        else if (isSpecialInteraction && !isSpecialSpawnInteraction)
        {
            if (interactionState == 1)
            {
                bool hasRequiredItem = InventoryManager.instance.HasItem(requiredItemId);
                if (hasRequiredItem)
                {
                    interactionState = 3;
                    if (GameManager.instance != null && GameManager.instance.interactionText != null)
                    {
                        GameManager.instance.interactionText.text = specialInteractionText;
                    }
                }
                else
                {
                    interactionState = 5;
                    if (GameManager.instance != null && GameManager.instance.interactionText != null)
                    {
                        GameManager.instance.interactionText.text = requirementFailureText;
                    }
                }
            }
            // Kích hoạt hành động chính
            else if (interactionState == 3)
            {
                // 1. LUÔN LUÔN xóa item kích hoạt (requiredItemId)
                InventoryManager.instance.RemoveItem(requiredItemId);

                // 2. Nhận item mới (itemData) nếu có
                if (itemData != null)
                {
                    InventoryManager.instance.AddItem(itemData);
                }

                // 3. Đánh dấu đã hoàn thành
                GameManager.instance.collectedItemIds.Add(uniqueId);

                // 4. KIỂM TRA VÀ THỰC HIỆN CHUYỂN SCENE
                if (triggersSceneChange && uiManager != null)
                {
                    // Chuyển Scene bằng hiệu ứng Fade Out
                    uiManager.FadeOutAndLoadScene(targetSceneIndex, fadeOutDuration);

                    // Do chuyển Scene sẽ phá hủy GameObject, chúng ta chỉ cần EndInteraction
                    // Tuy nhiên, không Destroy(gameObject) ngay lập tức để Coroutine chạy
                }
                else
                {
                    // Nếu không có hiệu ứng chuyển Scene đặc biệt, thì hủy đối tượng và kết thúc tương tác
                    Destroy(gameObject);
                    EndInteraction();
                }
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
            // ... (Logic Spawn giữ nguyên) ...
            if (interactionState == 1)
            {
                bool hasRequiredItem = InventoryManager.instance.HasItem(requiredItemId);
                if (hasRequiredItem)
                {
                    interactionState = 4;
                    if (GameManager.instance != null && GameManager.instance.interactionText != null)
                    {
                        GameManager.instance.interactionText.text = afterInteractionText;
                    }
                }
                else
                {
                    interactionState = 5;
                    if (GameManager.instance != null && GameManager.instance.interactionText != null)
                    {
                        GameManager.instance.interactionText.text = requirementFailureText;
                    }
                }
            }
            else if (interactionState == 4)
            {
                if (objectToSpawnPrefab != null && spawnPoint != null)
                {
                    GameObject spawnedObject = Instantiate(objectToSpawnPrefab, spawnPoint.position, spawnPoint.rotation);
                    DontDestroyOnLoad(spawnedObject);
                    GameManager.instance.completedSpawnActions.Add(spawnActionId);
                }
                InventoryManager.instance.RemoveItem(requiredItemId);
                EndInteraction();
            }
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

        // Tắt text UI
        if (GameManager.instance != null && GameManager.instance.interactionText != null)
        {
            GameManager.instance.interactionText.gameObject.SetActive(false);
        }

        // Mở khóa chuyển động nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }
}

// Giả định các Class cần thiết tồn tại (không bao gồm trong file này)
// public class Item : ScriptableObject { public string itemName; }
// public class InventoryManager : MonoBehaviour { public static InventoryManager instance; public bool HasItem(string id) { return true; } public void AddItem(Item item) { } public void RemoveItem(string id) { } }
// public class GameManager : MonoBehaviour { public static GameManager instance; public TextMeshProUGUI interactionText; public List<string> collectedItemIds = new List<string>(); public List<string> completedSpawnActions = new List<string>(); }
// public class Character_movement : MonoBehaviour { public bool canMove = true; }
