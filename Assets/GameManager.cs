using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

// GameManager quản lý trạng thái toàn cục và các tham chiếu scene-specific
// Nó sử dụng DontDestroyOnLoad để duy trì trạng thái xuyên suốt game.
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // --- CÁC THAM CHIẾU SCENE (Cần được gán lại khi tải Scene mới) ---
    [Header("Scene References (Reassigned On Load)")]
    [Tooltip("Văn bản tương tác UI.")]
    public TextMeshProUGUI interactionText;
    [Tooltip("Panel hỏi Yes/No UI.")]
    public GameObject questionPanel;
    public TextMeshProUGUI yesText;
    public TextMeshProUGUI noText;

    // Tham chiếu đến SpawnPointManager của Scene hiện tại (Giữ lại cho teleport TRONG scene).
    [Tooltip("Manager điểm spawn của Scene hiện tại. Được tự động gán lại.")]
    public SpawnPointManager spawnPointManager;

    // --- THAM CHIẾU PLAYER ---
    [Header("Player Control")]
    [Tooltip("Thể hiện Player đang hoạt động trong Scene. Được tìm lại trong mỗi Scene mới.")]
    // activePlayerInstance giờ đây sẽ chứa Player ĐƯỢC TÌM THẤY trong Scene MỚI.
    private GameObject activePlayerInstance;

    // Biến lưu trữ component điều khiển (giả sử tên là 'Character_movement')
    private Character_movement playerMovementScript;

    // --- TRẠNG THÁI DUY TRÌ (Được giữ lại qua các Scene) ---
    [Header("Persistent State")]
    public HashSet<string> collectedItemIds = new HashSet<string>();
    public HashSet<string> completedPuzzles = new HashSet<string>();
    public HashSet<string> completedSpawnActions = new HashSet<string>();

    public static bool isInputLocked = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Đăng ký sự kiện để tự động tìm lại các đối tượng scene-specific
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi đối tượng bị hủy để tránh lỗi
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Phương thức được gọi mỗi khi một Scene mới được tải hoàn tất
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("GameManager: Scene loaded. Reassigning scene-specific references and activating Player.");

        // 1. Gán lại SpawnPointManager (Tự động tìm trong Scene mới)
        SpawnPointManager manager = FindObjectOfType<SpawnPointManager>();
        if (manager != null)
        {
            spawnPointManager = manager;
            Debug.Log("SpawnPointManager successfully reassigned: " + manager.gameObject.name);
        }
        else
        {
            spawnPointManager = null;
            Debug.LogWarning("SpawnPointManager not found in scene: " + scene.name);
        }

        // 2. LOGIC QUAN TRỌNG: TÌM PLAYER TẠI CHỖ VÀ TRAO QUYỀN ĐIỀU KHIỂN!
        ActivatePlayerInNewScene();

        // LƯU Ý: Các tham chiếu UI (interactionText, questionPanel...)
        // cần được gán lại bằng cách gọi các hàm Set...() công khai
        // từ một script khác trong Scene mới (ví dụ: SceneUIManager).
    }

    // --- LOGIC MỚI: TÌM VÀ KÍCH HOẠT PLAYER TẠI CHỖ ---
    private void ActivatePlayerInNewScene()
    {
        // 1. Vô hiệu hóa Player cũ (để đề phòng, mặc dù nó sẽ bị hủy cùng Scene cũ)
        if (activePlayerInstance != null)
        {
            SetPlayerControlEnabled(activePlayerInstance, false);
            activePlayerInstance = null;
            playerMovementScript = null;
            Debug.Log("[GameManager] Vô hiệu hóa (Nếu tồn tại) PlayerInstance cũ.");
        }

        // 2. Tìm Player mới (phải được tag là "Player" và đã có sẵn trong Scene)
        activePlayerInstance = GameObject.FindGameObjectWithTag("Player");

        if (activePlayerInstance != null)
        {
            // Cần có script điều khiển Player (giả sử tên là 'Character_movement')
            playerMovementScript = activePlayerInstance.GetComponent<Character_movement>();

            if (playerMovementScript != null)
            {
                // Kích hoạt quyền điều khiển
                SetPlayerControlEnabled(activePlayerInstance, true);
                Debug.Log($"[GameManager] Player mới '{activePlayerInstance.name}' được tìm thấy và đã kích hoạt quyền điều khiển.");
            }
            else
            {
                Debug.LogError("[GameManager] KHÔNG THỂ KÍCH HOẠT PLAYER: Script 'Character_movement' không tìm thấy trên đối tượng Player!");
            }
        }
        else
        {
            Debug.LogError("[GameManager] KHÔNG THỂ KÍCH HOẠT PLAYER: Không tìm thấy GameObject với Tag 'Player' trong Scene mới!");
        }
    }

    // Giúp bật/tắt quyền điều khiển của Player (Cần Character_movement hỗ trợ phương thức này)
    public void SetPlayerControlEnabled(GameObject playerObject, bool isEnabled)
    {
        // LƯU Ý QUAN TRỌNG: Script 'Character_movement' CẦN phải có phương thức public 
        // để bật/tắt quyền điều khiển của nó. Ví dụ: playerMovementScript.SetControl(isEnabled);

        // Hiện tại, chúng ta chỉ thay đổi Component MonoBehaviour để kiểm soát, 
        // nhưng cách tốt nhất là tạo một phương thức chuyên biệt trong Character_movement.

        Character_movement movement = playerObject.GetComponent<Character_movement>();
        if (movement != null)
        {
            // GIẢ ĐỊNH: Character_movement có property 'isControlEnabled' hoặc tương đương
            // Nếu không, bạn có thể chỉ cần bật/tắt script:
            movement.enabled = isEnabled;

            // Nếu có thêm Input Component:
            // InputComponent input = playerObject.GetComponent<InputComponent>();
            // if (input != null) input.enabled = isEnabled;
        }
    }

    // --- LOGIC TELEPORT TRONG SCENE (Vẫn giữ lại) ---
    public void TeleportPlayerInScene(string targetPointName)
    {
        if (activePlayerInstance == null || spawnPointManager == null)
        {
            Debug.LogError("[GameManager] Teleport thất bại: Player hoặc SpawnPointManager không tồn tại.");
            return;
        }

        Transform targetSpawnTransform = spawnPointManager.GetSpawnPoint(targetPointName);

        if (targetSpawnTransform != null)
        {
            // Tắt/bật CharacterController để thực hiện dịch chuyển tức thời
            CharacterController cc = activePlayerInstance.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            activePlayerInstance.transform.position = targetSpawnTransform.position;
            activePlayerInstance.transform.rotation = targetSpawnTransform.rotation;

            if (cc != null) cc.enabled = true;

            Debug.Log($"[GameManager] Player teleported to: {targetPointName}");
        }
        else
        {
            Debug.LogWarning($"[GameManager] Teleport thất bại: SpawnPoint '{targetPointName}' không tìm thấy trong Scene hiện tại.");
        }
    }

    // --- Các Phương Thức Gán Lại Tham Chiếu ---

    public void SetInteractionText(TextMeshProUGUI text)
    {
        interactionText = text;
    }

    public void SetQuestionUI(GameObject panel, TextMeshProUGUI yes, TextMeshProUGUI no)
    {
        questionPanel = panel;
        yesText = yes;
        noText = no;
    }

    public void SetSpawnPointManager(SpawnPointManager manager)
    {
        spawnPointManager = manager;
    }

    // --- Các Phương Thức Quản Lý Trạng Thái ---

    public void CollectItem(string id)
    {
        collectedItemIds.Add(id);
    }

    public bool HasCollectedItem(string id)
    {
        return collectedItemIds.Contains(id);
    }

    // Phương thức này hiện KHÔNG LÀM GÌ, nó được giữ lại để tránh lỗi nếu các script khác gọi nó,
    // nhưng việc chuyển cảnh hiện không yêu cầu nó.
    public void SetNextSpawnPoint(string nextPointName)
    {
        Debug.LogWarning("[GameManager] SetNextSpawnPoint được gọi nhưng đã bị vô hiệu hóa vì logic chuyển cảnh mới.");
    }
}