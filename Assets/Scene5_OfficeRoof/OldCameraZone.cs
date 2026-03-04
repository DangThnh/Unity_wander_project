using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneOld : MonoBehaviour
{
    // Gán các camera của khu vực này trong Inspector
    public List<Camera> zoneCameras;

    // Camera chính của khu vực này
    public Camera mainCamera;

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là người chơi không
        if (other.CompareTag("Player"))
        {
            if (CameraManagerOld.instance != null)
            {
                CameraManagerOld.instance.EnterZone(this);
            }
        }
    }
}