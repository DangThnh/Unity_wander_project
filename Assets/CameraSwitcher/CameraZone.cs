using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
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
            if (CameraManager.instance != null)
            {
                CameraManager.instance.EnterZone(this);
            }
        }
    }
}