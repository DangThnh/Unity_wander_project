using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Đảm bảo chỉ có một bản sao của GameManager
    public static GameManager instance;

    // Dùng HashSet để lưu ID, giúp kiểm tra nhanh hơn so với List
    public HashSet<string> collectedItemIds = new HashSet<string>();

    // Vị trí và hướng xuất hiện của nhân vật
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;

    // Biến này sẽ theo dõi lần tải scene đầu tiên
    public bool isFirstLoad = true;

    void Awake()
    {
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
}