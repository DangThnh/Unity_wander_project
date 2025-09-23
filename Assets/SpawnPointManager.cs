using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager instance;

    // Dictionary để lưu trữ các điểm xuất hiện bằng tên
    public Dictionary<string, Transform> spawnPoints = new Dictionary<string, Transform>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Tự động tìm tất cả các đối tượng con và thêm vào Dictionary
        foreach (Transform child in transform)
        {
            if (!spawnPoints.ContainsKey(child.name))
            {
                spawnPoints.Add(child.name, child);
            }
            else
            {
                Debug.LogWarning("Có nhiều SpawnPoint với tên: " + child.name + ". Chỉ cái đầu tiên sẽ được sử dụng.");
            }
        }
    }

    // Hàm để lấy Transform của SpawnPoint theo tên
    public Transform GetSpawnPoint(string spawnPointName)
    {
        if (spawnPoints.ContainsKey(spawnPointName))
        {
            return spawnPoints[spawnPointName];
        }
        Debug.LogError("Không tìm thấy SpawnPoint với tên: " + spawnPointName);
        return null;
    }
}
