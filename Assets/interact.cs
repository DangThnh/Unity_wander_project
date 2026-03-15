using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InteractableObject : MonoBehaviour
{
    // === Cài đặt ID và Trạng thái ===
    public string uniqueId;
    public string spawnActionId;

    // === Cài đặt Âm thanh Linh hoạt (Cập nhật) ===
    [Header("Audio Settings")]
    [Tooltip("AudioSource dùng để phát âm thanh. Nếu để trống, sẽ tự tìm trên object này.")]
    public AudioSource audioSource;

    [Tooltip("Âm thanh khi bắt đầu tương tác (mở hộp thoại).")]
    public AudioClip startInteractionSound;

    [Tooltip("Âm thanh khi nhặt được item mới vào Inventory.")]
    public AudioClip pickupSound;

    [Tooltip("Âm thanh khi sử dụng/mất một item từ Inventory (ví dụ dùng chìa khóa).")]
    public AudioClip useItemSound;

    [Tooltip("Âm thanh khi một vật thể được triệu hồi (Spawn).")]
    public AudioClip spawnObjectSound;

    // === Tham chiếu UI Text ===
    [Header("UI Text Settings")]
    public TMP_Text localInteractionText;
    public string myText = "My bookshelf.";
    public string requirementFailureText = "It seems you are missing a key item to proceed.";
    public string specialInteractionText = "You have something, use it.";
    public string afterInteractionText = "You put it down, would you like to use it";

    // === Cài đặt Item và Tương tác Đặc biệt ===
    [Header("Special Interaction Settings")]
    public Item itemData;
    public bool isSpecialInteraction = false;
    public string requiredItemId;
    public bool isSpecialSpawnInteraction = false;
    public GameObject objectToSpawnPrefab;
    public Transform spawnPoint;

    private bool playerInRange = false;
    private bool isInteracting = false;
    private int interactionState = 0;
    private Character_movement playerController;
    private Animator playerAnimator;

    private TMP_Text GetActiveTextComponent()
    {
        if (localInteractionText != null) return localInteractionText;
        if (GameManager.instance != null) return GameManager.instance.interactionText;
        return null;
    }

    // Hàm hỗ trợ phát âm thanh tổng quát
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
    }

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (GameManager.instance != null && GameManager.instance.collectedItemIds.Contains(uniqueId))
        {
            Destroy(gameObject);
            return;
        }

        if (GameManager.instance != null && GameManager.instance.completedSpawnActions.Contains(spawnActionId))
        {
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
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteracting) StartInteraction();
            else ContinueInteraction();
        }
    }

    void StartInteraction()
    {
        isInteracting = true;
        interactionState = 1;

        // Phát âm thanh khi bắt đầu tương tác
        PlaySound(startInteractionSound);

        TMP_Text textComponent = GetActiveTextComponent();
        if (textComponent != null)
        {
            textComponent.gameObject.SetActive(true);
            textComponent.text = myText;
        }

        if (playerController != null) playerController.canMove = false;
        if (playerAnimator != null) playerAnimator.SetBool("IsMoving", false);
    }

    void ContinueInteraction()
    {
        TMP_Text textComponent = GetActiveTextComponent();

        // 1. DẠNG NHẶT ĐỒ THÔNG THƯỜNG
        if (itemData != null && !isSpecialInteraction && !isSpecialSpawnInteraction)
        {
            if (interactionState == 1)
            {
                bool isRequired = !string.IsNullOrEmpty(requiredItemId);
                bool hasRequiredItem = isRequired ? InventoryManager.instance.HasItem(requiredItemId) : true;

                if (hasRequiredItem)
                {
                    interactionState = 2;
                    if (textComponent != null)
                        textComponent.text = "Do you want to take this " + itemData.itemName + "? (Press E to take)";
                }
                else
                {
                    interactionState = 5;
                    if (textComponent != null) textComponent.text = requirementFailureText;
                }
            }
            else if (interactionState == 2)
            {
                InventoryManager.instance.AddItem(itemData);
                PlaySound(pickupSound); // PHÁT ÂM THANH NHẶT ĐỒ

                GameManager.instance.collectedItemIds.Add(uniqueId);
                Destroy(gameObject);
                EndInteraction();
            }
            else if (interactionState == 5) EndInteraction();
        }
        // 2. DẠNG TƯƠNG TÁC ĐẶC BIỆT CÓ THỂ NHẬN ĐỒ
        else if (isSpecialInteraction && !isSpecialSpawnInteraction)
        {
            if (interactionState == 1)
            {
                if (InventoryManager.instance.HasItem(requiredItemId))
                {
                    interactionState = 3;
                    if (textComponent != null) textComponent.text = specialInteractionText;
                }
                else
                {
                    interactionState = 5;
                    if (textComponent != null) textComponent.text = requirementFailureText;
                }
            }
            else if (interactionState == 3)
            {
                // Sử dụng item yêu cầu
                InventoryManager.instance.RemoveItem(requiredItemId);
                PlaySound(useItemSound); // PHÁT ÂM THANH SỬ DỤNG ITEM

                if (itemData != null)
                {
                    InventoryManager.instance.AddItem(itemData);
                    PlaySound(pickupSound); // PHÁT ÂM THANH NHẬN ITEM MỚI
                }

                GameManager.instance.collectedItemIds.Add(uniqueId);
                Destroy(gameObject);
                EndInteraction();
            }
            else if (interactionState == 5) EndInteraction();
        }
        // 3. DẠNG SPAWN VẬT THỂ
        else if (isSpecialSpawnInteraction)
        {
            if (interactionState == 1)
            {
                if (InventoryManager.instance.HasItem(requiredItemId))
                {
                    interactionState = 4;
                    if (textComponent != null) textComponent.text = afterInteractionText;
                }
                else
                {
                    interactionState = 5;
                    if (textComponent != null) textComponent.text = requirementFailureText;
                }
            }
            else if (interactionState == 4)
            {
                if (objectToSpawnPrefab != null && spawnPoint != null)
                {
                    Instantiate(objectToSpawnPrefab, spawnPoint.position, spawnPoint.rotation);
                    PlaySound(spawnObjectSound); // PHÁT ÂM THANH KHI VẬT THỂ XUẤT HIỆN
                    GameManager.instance.completedSpawnActions.Add(spawnActionId);
                }

                InventoryManager.instance.RemoveItem(requiredItemId);
                PlaySound(useItemSound); // PHÁT ÂM THANH KHI TIÊU TỐN ITEM YÊU CẦU

                EndInteraction();
            }
            else if (interactionState == 5) EndInteraction();
        }
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
        if (textComponent != null) textComponent.gameObject.SetActive(false);
        if (playerController != null) playerController.canMove = true;
    }

    void OnEnable() { HexaPuzzleManager.OnPuzzleCompleted += ForceTextRefresh; }
    void OnDisable() { HexaPuzzleManager.OnPuzzleCompleted -= ForceTextRefresh; }

    void ForceTextRefresh()
    {
        TMP_Text textComponent = GetActiveTextComponent();
        if (playerInRange && textComponent != null)
        {
            textComponent.text = myText;
            textComponent.gameObject.SetActive(true);
        }
    }
}