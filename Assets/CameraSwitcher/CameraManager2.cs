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
            cam.enabled = false;
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
        // Chuyển đổi camera trong cùng khu vực bằng phím M
        if (Input.GetKeyDown(KeyCode.M))
        {
            SwitchToNextCameraInZone();
        }
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
        cameraIndex = currentZone.zoneCameras.IndexOf(activeCamera);
    }

    // Chuyển đổi giữa các camera
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

        // Đảm bảo không chuyển về camera hiện tại
        if (nextCamera != activeCamera)
        {
            SwitchCamera(nextCamera);
        }
    }
}