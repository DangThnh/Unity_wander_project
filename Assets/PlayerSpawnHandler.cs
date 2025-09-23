using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    void Start()
    {
        // Kiểm tra xem có một điểm đến đã được lưu từ scene trước không
        if (GameManager.instance != null && !string.IsNullOrEmpty(GameManager.instance.desiredSpawnPointName))
        {
            // Tìm điểm đến trong scene hiện tại
            Transform spawnPoint = GameManager.instance.spawnPointManager.GetSpawnPoint(GameManager.instance.desiredSpawnPointName);
            if (spawnPoint != null)
            {
                // Dịch chuyển nhân vật đến điểm đó
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;

                // Xóa tên điểm đến để tránh dịch chuyển lại
                GameManager.instance.desiredSpawnPointName = "";
            }
            else
            {
                Debug.LogError("Không tìm thấy điểm đến '" + GameManager.instance.desiredSpawnPointName + "' trong scene hiện tại. Vui lòng kiểm tra lại cấu hình.");
            }
        }
    }
}
