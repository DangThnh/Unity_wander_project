using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Sử dụng Linq để dễ dàng tìm kiếm

public class CameraManager : MonoBehaviour
{
    // Singleton instance
    public static CameraManager instance;

    // Các camera trong scene được gán thủ công trong Unity Inspector
    public List<Camera> allCameras;

    // Khu vực hiện tại mà người chơi đang ở
    [HideInInspector]
    public CameraZone currentZone;

    private Camera activeCamera;
    private int cameraIndex = 0;

    void Awake()
    {
        // Thiết lập Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Vô hiệu hóa tất cả các camera ban đầu
        foreach (Camera cam in allCameras)
        {
            if (cam != null)
            {
                cam.enabled = false;
            }
        }

        // Kích hoạt camera đầu tiên
        if (allCameras.Count > 0)
        {
            activeCamera = allCameras[0];
            activeCamera.enabled = true;
        }
    }

    void Update()
    {
        // --- SỬA LỖI: Đặt kiểm tra trạng thái tĩnh lên ĐẦU hàm Update ---
        // Nếu Puzzle đang hoạt động, thoát ngay lập tức và không xử lý bất kỳ Input nào bên dưới.
        if (HexaPuzzleManager.IsPuzzleActiveStatic)
        {
            return;
        }
        // ---------------------------------------------------------------

        // Chuyển đổi camera trong cùng khu vực bằng phím M (CHỈ KHI KHÔNG GIẢI ĐỐ)
        if (Input.GetKeyDown(KeyCode.M))
        {
            SwitchToNextCameraInZone();
        }

        // LƯU Ý: Khối code dưới đây đã bị XÓA vì nó là dư thừa và gây lỗi logic:
        /*
        if (HexaPuzzleManager.IsPuzzleActiveStatic)
        {
            return; // Dừng xử lý Input nếu Puzzle đang hoạt động
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            SwitchCamera(); // Lỗi: Hàm SwitchCamera() cần tham số
        }
        */
    }

    // Phương thức được gọi từ CameraZone khi người chơi đi vào
    public void EnterZone(CameraZone newZone)
    {
        if (currentZone == newZone)
        {
            return;
        }

        currentZone = newZone;

        // Chuyển ngay lập tức đến camera chính của khu vực mới
        SwitchCamera(currentZone.mainCamera);

        // Cập nhật chỉ số camera để việc chuyển đổi tiếp theo là đúng
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

    // Chuyển đổi giữa các camera (Hàm cần tham số Camera)
    private void SwitchCamera(Camera newCamera)
    {
        if (activeCamera != null)
        {
            activeCamera.enabled = false;
        }

        activeCamera = newCamera;
        activeCamera.enabled = true;
    }

    // Chuyển đổi đến camera tiếp theo trong khu vực hiện tại
    private void SwitchToNextCameraInZone()
    {
        if (currentZone == null || currentZone.zoneCameras.Count <= 1)
        {
            // Không có gì để chuyển đổi hoặc chỉ có một camera
            return;
        }

        cameraIndex = (cameraIndex + 1) % currentZone.zoneCameras.Count;

        Camera nextCamera = currentZone.zoneCameras[cameraIndex];

        // Đảm bảo không chuyển về camera hiện tại (trên thực tế không cần thiết do logic modulo)
        // Nhưng để an toàn, ta vẫn giữ
        if (nextCamera != activeCamera)
        {
            SwitchCamera(nextCamera);
        }
    }
}