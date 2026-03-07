using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SimpleSaveSystem : MonoBehaviour
{
    public static SimpleSaveSystem Instance;

    [Header("Cấu hình UI")]
    [Tooltip("Prefab chứa Icon và Text (đã gắn script AutoSaveFlicker)")]
    public GameObject saveIconPrefab;
    [Tooltip("Thời gian icon hiển thị trên màn hình")]
    public float displayDuration = 3f;

    [Header("Cấu hình Điều kiện Lưu")]
    [Tooltip("Danh sách Index của các Scene được phép tự động lưu (Ví dụ: 1, 2, 3)")]
    public List<int> validSaveSceneIndices = new List<int>();

    private string filePath;

    private void Awake()
    {
        // Khởi tạo Singleton để quản lý xuyên suốt các Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Đường dẫn lưu file: C:/Users/Name/AppData/LocalLow/CompanyName/ProjectName
            filePath = Path.Combine(Application.persistentDataPath, "autosave_data.txt");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện khi một Scene được load xong
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh lỗi bộ nhớ
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kiểm tra xem Index của Scene hiện tại có nằm trong danh sách cho phép không
        if (validSaveSceneIndices.Contains(scene.buildIndex))
        {
            ExecuteAutoSave(scene.buildIndex);
        }
    }

    private void ExecuteAutoSave(int sceneIndex)
    {
        try
        {
            // Ghi dữ liệu Index vào file (Bạn có thể mở rộng để lưu tọa độ, máu, vật phẩm...)
            File.WriteAllText(filePath, sceneIndex.ToString());

            Debug.Log($"<color=green>Auto Save Thành Công!</color> Tại Scene Index: {sceneIndex}");

            // Hiển thị hiệu ứng UI nhấp nháy
            if (saveIconPrefab != null)
            {
                StartCoroutine(ShowSaveIconRoutine());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi ghi file save: " + e.Message);
        }
    }

    private IEnumerator ShowSaveIconRoutine()
    {
        // Sinh ra UI từ Prefab (Prefab này nên chứa script Flicker)
        GameObject iconInstance = Instantiate(saveIconPrefab);

        // Chờ trong khoảng thời gian quy định
        yield return new WaitForSeconds(displayDuration);

        // Xóa UI sau khi hiển thị xong
        if (iconInstance != null)
        {
            Destroy(iconInstance);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            if (int.TryParse(content, out int savedIndex))
            {
                SceneManager.LoadScene(savedIndex);
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy file save để load!");
        }
    }
}