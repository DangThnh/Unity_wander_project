using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement; // Thêm thư viện quản lý Scene

public class CameraManager : MonoBehaviour
{
    // Singleton instance
    public static CameraManager instance;

    // Danh sách camera trong scene hiện tại.
    // LƯU Ý: Danh sách này sẽ được CẬP NHẬT tự động khi chuyển Scene,
    // KHÔNG cần gán thủ công trong Inspector (bây giờ có thể ẩn đi).
    public List<Camera> allCameras = new List<Camera>();

    // Khu vực hiện tại mà người chơi đang ở
    [HideInInspector]
    public CameraZone currentZone;

    private Camera activeCamera;
    private int cameraIndex = 0;

    public bool isCutscenePlaying = false;

    void Awake()
    {
        // Thiết lập Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Đăng ký sự kiện: Khi Scene mới được tải, gọi hàm OnSceneLoaded
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
        // Quan trọng: Hủy đăng ký để tránh lỗi khi đối tượng bị hủy
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Hàm này được gọi MỖI KHI một Scene mới hoàn tất tải
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name + ". Rebuilding camera list.");

        // 1. XÓA tất cả các tham chiếu camera cũ.
        allCameras.Clear();

        // 2. TÌM KIẾM VÀ THÊM tất cả các camera mới trong Scene vừa tải.
        Camera[] sceneCameras = FindObjectsOfType<Camera>();

        foreach (Camera cam in sceneCameras)
        {
            // Kiểm tra: Nếu camera là CameraManager, bỏ qua.
            // Điều này áp dụng nếu CameraManager cũng có thành phần Camera.
            if (cam.gameObject != gameObject)
            {
                allCameras.Add(cam);
            }
        }

        // 3. Khởi tạo trạng thái camera trong scene mới
        InitializeCamerasForNewScene();

        // Đảm bảo currentZone được đặt lại
        currentZone = null;
    }

    // Logic khởi tạo camera khi bắt đầu Scene (hoặc tải Scene mới)
    public void InitializeCamerasForNewScene()
    {
        if (allCameras.Count == 0) return;

        // 1. Tìm camera chính (MainCamera) trước
        Camera mainCam = allCameras.FirstOrDefault(cam => cam.CompareTag("MainCamera"));

        // 2. Tắt tất cả các camera khác, RIÊNG MainCamera thì xử lý đặc biệt
        foreach (Camera cam in allCameras)
        {
            if (cam == mainCam) continue; // Bỏ qua, không tắt MainCamera ở đây
            cam.enabled = false;
        }

        // 3. Logic quyết định bật cái nào
        if (isCutscenePlaying)
        {
            // Nếu đang đóng phim, PHẢI bật Main Camera để Cinemachine Brain chạy được
            if (mainCam != null)
            {
                mainCam.enabled = true;
                activeCamera = mainCam;
                Debug.Log("<color=cyan>CameraManager: Cutscene mode - Main Camera enabled for Cinemachine.</color>");
            }
            return;
        }

        // 4. Nếu không phải cutscene (lúc chơi bình thường)
        if (mainCam != null)
        {
            activeCamera = mainCam;
        }
        else
        {
            activeCamera = allCameras[0];
        }

        activeCamera.enabled = true;
        cameraIndex = 0;
    }

    // Thay thế logic trong Start() bằng InitializeCamerasForNewScene()
    void Start()
    {
        // Logic trong Start() được thay thế bởi OnSceneLoaded và InitializeCamerasForNewScene()
        // để đảm bảo nó hoạt động ngay cả khi Manager đã tồn tại từ Scene trước.
        // Tuy nhiên, ta vẫn gọi nó lần đầu tiên nếu manager được tạo ra trong Scene 1
        if (SceneManager.GetActiveScene().buildIndex == 0) // Giả sử scene đầu tiên là 0
        {
            InitializeCamerasForNewScene();
        }
    }

    void Update()
    {
        // --- SỬA LỖI: Đặt kiểm tra trạng thái tĩnh lên ĐẦU hàm Update ---
        // Nếu Puzzle đang hoạt động, thoát ngay lập tức và không xử lý bất kỳ Input nào bên dưới.
        if (GameState.isInputLocked || HexaPuzzleManager.IsPuzzleActiveStatic)
        {
            return;
        }
        // ---------------------------------------------------------------

        // Chuyển đổi camera trong cùng khu vực bằng phím M (CHỈ KHI KHÔNG GIẢI ĐỐ)
        if (Input.GetKeyDown(KeyCode.M))
        {
            SwitchToNextCameraInZone();
        }
    }

    // Phương thức được gọi từ CameraZone khi người chơi đi vào
    public void EnterZone(CameraZone newZone)
    {
        // Đảm bảo rằng camera đang hoạt động không phải là null trước khi chuyển
        if (activeCamera == null) return;

        if (currentZone == newZone)
        {
            return;
        }

        currentZone = newZone;

        // Chuyển ngay lập tức đến camera chính của khu vực mới
        if (currentZone.mainCamera != null)
        {
            SwitchCamera(currentZone.mainCamera);
        }
        else
        {
            // Xử lý trường hợp CameraZone không có mainCamera (chuyển về camera mặc định của scene)
            Debug.LogWarning("CameraZone " + currentZone.name + " is missing its mainCamera reference.");
        }

        // Cập nhật chỉ số camera để việc chuyển đổi tiếp theo là đúng
        if (currentZone.zoneCameras != null)
        {
            int index = currentZone.zoneCameras.IndexOf(activeCamera);
            if (index != -1)
            {
                cameraIndex = index;
            }
            else
            {
                cameraIndex = 0;
            }
        }
    }

    // Chuyển đổi giữa các camera (Hàm cần tham số Camera)
    private void SwitchCamera(Camera newCamera)
    {
        if (newCamera == null)
        {
            Debug.LogError("Attempted to switch to a null camera.");
            return;
        }

        if (activeCamera != null && activeCamera != newCamera)
        {
            activeCamera.enabled = false;
        }

        activeCamera = newCamera;
        activeCamera.enabled = true;
    }

    // Chuyển đổi đến camera tiếp theo trong khu vực hiện tại
    private void SwitchToNextCameraInZone()
    {
        if (currentZone == null || currentZone.zoneCameras == null || currentZone.zoneCameras.Count <= 1)
        {
            // Không có gì để chuyển đổi hoặc chỉ có một camera
            return;
        }

        // Tăng chỉ số và lặp lại nếu vượt quá số lượng
        cameraIndex = (cameraIndex + 1) % currentZone.zoneCameras.Count;

        Camera nextCamera = currentZone.zoneCameras[cameraIndex];

        // Đảm bảo camera tiếp theo không null trước khi chuyển
        if (nextCamera != null)
        {
            SwitchCamera(nextCamera);
        }
        else
        {
            Debug.LogWarning("Next camera in zone is null at index: " + cameraIndex);
        }
    }
}