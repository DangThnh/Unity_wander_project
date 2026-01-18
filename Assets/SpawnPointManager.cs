using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Đảm bảo script này được đính kèm vào một GameObject trong mỗi Scene.
// Tất cả các Spawn Point (GameObjects) nên là con của đối tượng này.
public class SpawnPointManager : MonoBehaviour
{
    // Singleton instance, cho phép truy cập từ bất cứ đâu.
    public static SpawnPointManager instance;

    // Dictionary để lưu trữ các Transform (vị trí + xoay) của các điểm spawn.
    public Dictionary<string, Transform> spawnPoints = new Dictionary<string, Transform>();

    void Awake()
    {
        // Khởi tạo Singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // Tránh có nhiều manager trong cùng một Scene
            Destroy(gameObject);
            return;
        }

        // --- Logic quan trọng: Tự động đăng ký các điểm spawn ---
        // Lặp qua tất cả các đối tượng con của SpawnPointManager hiện tại.
        // Các đối tượng con này chính là các điểm Spawn Point vật lý trong Scene.
        foreach (Transform child in transform)
        {
            if (!spawnPoints.ContainsKey(child.name))
            {
                // Thêm tên và Transform của con vào dictionary.
                // Transform này chứa vị trí X, Y, Z mà bạn đã thay đổi trong Editor!
                spawnPoints.Add(child.name, child);
                Debug.Log($"[SpawnManager] Đã đăng ký điểm spawn: {child.name} tại vị trí: {child.position}");
            }
            else
            {
                Debug.LogWarning("Có nhiều SpawnPoint với tên: " + child.name + ". Chỉ cái đầu tiên sẽ được sử dụng.");
            }
        }
    }

    // Hàm public được GameManager gọi để lấy vị trí spawn mong muốn.
    public Transform GetSpawnPoint(string spawnPointName)
    {
        if (spawnPoints.ContainsKey(spawnPointName))
        {
            return spawnPoints[spawnPointName];
        }

        // Lưới an toàn: Trả về null nếu không tìm thấy. GameManager phải xử lý việc này!
        Debug.LogError("Không tìm thấy SpawnPoint với tên: " + spawnPointName + ". Kiểm tra lỗi chính tả.");
        return null;
    }
}