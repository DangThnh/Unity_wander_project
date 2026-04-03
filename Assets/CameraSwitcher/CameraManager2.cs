using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public List<Camera> allCameras = new List<Camera>();
    [HideInInspector] public CameraZone currentZone;

    private Camera activeCamera;
    private int cameraIndex = 0;
    public bool isCutscenePlaying = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        allCameras.Clear();
        // Tìm tất cả camera, kể cả đang bị tắt
        Camera[] sceneCameras = FindObjectsOfType<Camera>(true);
        foreach (Camera cam in sceneCameras)
        {
            if (cam.gameObject != gameObject) allCameras.Add(cam);
        }

        InitializeCamerasForNewScene();
        currentZone = null;
    }

    public void InitializeCamerasForNewScene()
    {
        // BƯỚC QUAN TRỌNG: Quét lại toàn bộ camera trong Scene một lần nữa 
        // để đảm bảo không bỏ sót Gameplay Camera sau khi chuyển cảnh
        allCameras = FindObjectsOfType<Camera>(true).Where(c => c.gameObject != gameObject).ToList();

        if (allCameras.Count == 0)
        {
            Debug.LogError("CameraManager: KHÔNG TÌM THẤY BẤT KỲ CAMERA NÀO!");
            return;
        }

        // Tìm Camera có Tag MainCamera
        Camera mainCam = allCameras.FirstOrDefault(cam => cam.CompareTag("MainCamera"));

        if (isCutscenePlaying)
        {
            // TRONG CUTSCENE: Chỉ bật camera có Cinemachine Brain
            foreach (Camera cam in allCameras)
            {
                // BỎ QUA: Nếu là UICamera thì không can thiệp
                if (cam.CompareTag("UICamera")) continue;

                // QUAN TRỌNG: Nếu thấy camera có CinemachineBrain, TUYỆT ĐỐI không tắt nó ở đây
                if (cam.GetComponent<Cinemachine.CinemachineBrain>() != null)
                {
                    cam.enabled = true;
                    continue;
                }

                // Chỉ tắt các camera bình thường khác
                cam.enabled = false;
            }
            Debug.Log("<color=cyan>CameraManager: Đang chạy Cutscene...</color>");
        }
        else
        {
            // KHI KẾT THÚC CUTSCENE: Tắt hết (trừ UICamera) và chỉ bật MainCamera
            foreach (Camera cam in allCameras)
            {
                if (!cam.CompareTag("UICamera"))
                {
                    cam.enabled = false;
                }
            }

            if (mainCam != null)
            {
                mainCam.enabled = true;
                activeCamera = mainCam;
                Debug.Log("<color=green>CameraManager: Đã kích hoạt Gameplay Camera thành công!</color>");
            }
            else
            {
                // Nếu không tìm thấy tag MainCamera, bật đại cái đầu tiên (không phải UI) để tránh màn hình đen
                Camera backupCam = allCameras.FirstOrDefault(c => !c.CompareTag("UICamera"));
                if (backupCam != null)
                {
                    backupCam.enabled = true;
                    activeCamera = backupCam;
                }
                Debug.LogWarning("CameraManager: Không tìm thấy tag MainCamera, bật camera dự phòng.");
            }
        }
    }

    void Start()
    {
        InitializeCamerasForNewScene();
    }

    void Update()
    {
        if (GameState.isInputLocked || HexaPuzzleManager.IsPuzzleActiveStatic) return;

        if (Input.GetKeyDown(KeyCode.M)) SwitchToNextCameraInZone();
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
            // CHỈ TẮT: Nếu activeCamera cũ không phải là UICamera
            if (!activeCamera.CompareTag("UICamera"))
            {
                activeCamera.enabled = false;
            }
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