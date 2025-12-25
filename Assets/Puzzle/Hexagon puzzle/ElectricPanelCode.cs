using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ElectricPanelInteract : MonoBehaviour
{
    // Cần phải gán một ID duy nhất cho mỗi vật phẩm trong Inspector
    // Ví dụ: "sach_cu", "chia_khoa", v.v.
    public string uniqueId;
    public string spawnActionId; // ID duy nhất cho hành động spawn này

    // Cài đặt cho tương tác đặc biệt (ví dụ: mở cửa, hoặc nhặt item bằng key)
    public bool isSpecialInteraction = false;
    [Tooltip("ID của item cần có trong kho đồ để tương tác/nhặt item này.")]
    public string requiredItemId; // ID của item cần có trong kho đồ để tương tác đặc biệt hoặc nhặt item này
    public string specialInteractionText = "You have something, use it.";
    [Tooltip("Thông báo hiển thị nếu thiếu item yêu cầu cho cả tương tác đặc biệt và nhặt item.")]
    public string requirementFailureText = "It seems you are missing a key item to proceed.";


    // Cài đặt cho tương tác đặc biệt mới (tạo vật thể)
    public bool isSpecialSpawnInteraction = false;
    public GameObject objectToSpawnPrefab;
    public Transform spawnPoint;
    public string afterInteractionText = "You put it down, would you like to use it";

    // Đối tượng bị hủy sau khi spawn thành công (ví dụ: hủy bản mạch cũ)
    [Tooltip("Đối tượng bị hủy sau khi Special Spawn Interaction thành công. Thường là chính vật thể này.")]
    public GameObject objectToDestroyAfterSpawn;

    // Cài đặt UI và dữ liệu
    public Item itemData; // Nếu là một món đồ có thể nhặt, gán itemData vào đây
    public string myText = "My bookshelf.";

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private bool isInteracting = false;
    // 0: không tương tác, 1: thoại đầu, 2: nhắc nhở nhặt item (Thành công - Không tiêu thụ requiredItemId)
    // 3: special interaction (Thành công - TIÊU THỤ requiredItemId & CÓ THỂ nhặt item)
    // 4: special spawn interaction (Thành công - đang chờ bấm E lần cuối)
    // 5: Thông báo thất bại (Cần bấm E lần nữa để kết thúc).
    private int interactionState = 0;

    // Tham chiếu đến script nhân vật
    private Character_movement playerController;
    private Animator playerAnimator;

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
        // Nếu đã spawn, đối tượng ban đầu (Bảng mạch hỏng) cần bị hủy
        if (!string.IsNullOrEmpty(spawnActionId) && GameManager.instance != null && GameManager.instance.completedSpawnActions.Contains(spawnActionId))
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

    /// <summary>
    /// Chạy sau tất cả các hàm Update() và LateUpdate() khác. 
    /// Phương thức này được sử dụng để ÉP BUỘC text hiển thị (Ghi đè tuyệt đối)
    /// nhằm ghi đè lên các script khác (như Character_movement) có thể 
    /// vô hiệu hóa text ngay lập tức khi canMove = false.
    /// </summary>
    void LateUpdate()
    {
        if (isInteracting && GameManager.instance != null && GameManager.instance.interactionText != null)
        {
            // Ép buộc bật UI text. Đây là cơ chế ghi đè chính.
            GameManager.instance.interactionText.gameObject.SetActive(true);
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
        // Xử lý tương tác nhặt đồ (có itemData và KHÔNG phải tương tác đặc biệt khác)
        if (itemData != null && !isSpecialInteraction && !isSpecialSpawnInteraction)
        {
            // Luồng logic tương tác nhặt đồ
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
                // Hành động nhặt item
                InventoryManager.instance.AddItem(itemData);
                if (GameManager.instance != null) GameManager.instance.collectedItemIds.Add(uniqueId);

                // Kết thúc tương tác và hủy vật thể
                EndInteraction();
                Destroy(gameObject);
            }
            else if (interactionState == 5)
            {
                EndInteraction();
            }
        }
        // Xử lý tương tác đặc biệt (TIÊU THỤ requiredItemId, CÓ THỂ nhặt itemData HOẶC chỉ kích hoạt)
        else if (isSpecialInteraction && !isSpecialSpawnInteraction)
        {
            // Luồng logic tương tác đặc biệt
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
                if (GameManager.instance != null) GameManager.instance.collectedItemIds.Add(uniqueId);

                EndInteraction();
                Destroy(gameObject);
            }
            else if (interactionState == 5)
            {
                EndInteraction();
            }
        }
        // Xử lý tương tác đặc biệt (tạo vật thể - Electric Panel Logic)
        else if (isSpecialSpawnInteraction)
        {
            // State 1: Bắt đầu, hiển thị myText
            if (interactionState == 1)
            {
                bool hasRequiredItem = InventoryManager.instance.HasItem(requiredItemId);
                if (hasRequiredItem)
                {
                    // Chuyển sang State 4: Thực hiện hành động spawn
                    interactionState = 4;

                    // --- THỰC HIỆN HÀNH ĐỘNG SPAWN ---
                    if (objectToSpawnPrefab != null && spawnPoint != null)
                    {
                        GameObject spawnedObject = Instantiate(objectToSpawnPrefab, spawnPoint.position, spawnPoint.rotation);

                        // Gán đối tượng cha (InteractableObject này) vào trigger
                        FusePuzzleTrigger trigger = spawnedObject.GetComponent<FusePuzzleTrigger>();
                        if (trigger != null)
                        {
                            trigger.parentInteractableObject = this.gameObject;
                        }

                        // Đánh dấu hành động spawn đã hoàn thành
                        if (GameManager.instance != null) GameManager.instance.completedSpawnActions.Add(spawnActionId);
                    }

                    // Tiêu thụ item kích hoạt
                    InventoryManager.instance.RemoveItem(requiredItemId);

                    // Hủy đối tượng ban đầu (Bảng mạch hỏng)
                    if (objectToDestroyAfterSpawn != null)
                    {
                        Destroy(objectToDestroyAfterSpawn);
                    }
                    // --- KẾT THÚC HÀNH ĐỘNG SPAWN ---

                    // Hiển thị text sau khi hành động hoàn tất
                    if (GameManager.instance != null && GameManager.instance.interactionText != null)
                    {
                        GameManager.instance.interactionText.text = afterInteractionText;
                    }

                    // LƯU Ý: KHÔNG GỌI EndInteraction() Ở ĐÂY.
                    // Việc hủy objectToDestroyAfterSpawn (thường là this.gameObject)
                    // sẽ làm script này dừng lại trong frame hiện tại.
                    // Nếu objectToDestroyAfterSpawn KHÔNG phải là chính nó, ta sẽ chờ nhấn E lần nữa.
                    if (objectToDestroyAfterSpawn == this.gameObject)
                    {
                        // Nếu đối tượng bị hủy là chính nó, ta phải gọi EndInteraction ngay lập tức
                        EndInteraction();
                        // Hàm Destroy(gameObject) được gọi trên objectToDestroyAfterSpawn
                        // Dù sao thì đối tượng này cũng sẽ biến mất sau frame này.

                        // Thêm Log để xác nhận việc hủy đối tượng và EndInteraction đã xảy ra.
                        Debug.Log("Electric Panel: Spawn successful and object destroyed. Interaction ended.");
                    }
                    else
                    {
                        // Nếu đối tượng bị hủy không phải là chính nó, ta chờ người dùng bấm E lần cuối để EndInteraction
                        interactionState = 6; // Trạng thái chờ End Interaction sau khi hoàn thành hành động
                    }

                }
                else
                {
                    // Thất bại: Chuyển sang trạng thái thông báo thất bại (state 5)
                    interactionState = 5;
                    if (GameManager.instance != null && GameManager.instance.interactionText != null)
                    {
                        GameManager.instance.interactionText.text = requirementFailureText;
                    }
                }
            }
            // State 6: Trạng thái chờ bấm E lần cuối sau khi Special Spawn thành công (chỉ khi không hủy chính nó)
            else if (interactionState == 6)
            {
                EndInteraction();
            }
            // State 5: Trạng thái thông báo thất bại, chờ bấm E để kết thúc
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