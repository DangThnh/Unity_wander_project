using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Tham chiếu đến các thành phần UI
    public TextMeshProUGUI interactionText;
    public GameObject questionPanel;
    public TextMeshProUGUI yesText;
    public TextMeshProUGUI noText;

    // Tham chiếu đến SpawnPointManager
    public SpawnPointManager spawnPointManager;

    // Dùng HashSet để lưu ID, giúp kiểm tra nhanh hơn so với List
    public HashSet<string> collectedItemIds = new HashSet<string>();

    // Vị trí và hướng xuất hiện của nhân vật
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;

    // Biến này sẽ theo dõi lần tải scene đầu tiên
    public bool isFirstLoad = true;

    // Lưu tên điểm đến mong muốn
    public string desiredSpawnPointName;

    // HashSet mới để lưu ID của các hành động spawn đã hoàn thành
    public HashSet<string> completedSpawnActions = new HashSet<string>();


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

    // Các phương thức công khai để thiết lập lại các tham chiếu UI
    public void SetInteractionText(TextMeshProUGUI text)
    {
        interactionText = text;
    }

    public void SetQuestionUI(GameObject panel, TextMeshProUGUI yes, TextMeshProUGUI no)
    {
        questionPanel = panel;
        yesText = yes;
        noText = no;
    }

    // Phương thức mới để thiết lập SpawnPointManager của scene hiện tại
    public void SetSpawnPointManager(SpawnPointManager manager)
    {
        spawnPointManager = manager;
    }
}