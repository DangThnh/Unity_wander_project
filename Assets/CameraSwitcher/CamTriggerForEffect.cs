using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    public CameraInterScene3 cameraSystem;
    public Transform startPoint; // Điểm bắt đầu vùng
    public Transform endPoint;   // Điểm kết thúc vùng

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tính toán vị trí nhân vật trong đoạn từ Start đến End (0 -> 1)
            float totalDistance = Vector3.Distance(startPoint.position, endPoint.position);
            float currentDistance = Vector3.Distance(startPoint.position, other.transform.position);

            float progress = currentDistance / totalDistance;
            cameraSystem.SetProgress(progress);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraSystem.SetProgress(0f); // Reset khi ra khỏi vùng
        }
    }
}