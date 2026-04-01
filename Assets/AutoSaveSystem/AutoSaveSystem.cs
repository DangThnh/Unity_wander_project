using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SimpleSaveSystem : MonoBehaviour
{
    public static SimpleSaveSystem Instance;

    [Header("Cấu hình Prefab")]
    public GameObject saveCanvasPrefab;
    public float displayDuration = 3.0f;

    [Header("Cấu hình Camera Tag")]
    public string uiCameraTag = "UICamera";

    [Header("Danh sách Scene ID cho phép Save")]
    public List<int> validSaveSceneIndices = new List<int>();

    private string filePath;
    private GameObject currentActiveUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.persistentDataPath, "autosave_data.txt");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (validSaveSceneIndices.Contains(scene.buildIndex))
        {
            StartCoroutine(ExecuteAutoSaveProcess(scene.buildIndex));
        }
    }

    private IEnumerator ExecuteAutoSaveProcess(int sceneIndex)
    {
        bool saveSuccess = false;
        try
        {
            File.WriteAllText(filePath, sceneIndex.ToString());
            saveSuccess = true;
            Debug.Log($"[SaveSystem] Auto Save thành công tại Scene {sceneIndex}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveSystem] Lỗi khi ghi file Save: " + e.Message);
        }

        if (saveSuccess && saveCanvasPrefab != null)
        {
            // Đợi 2 frame để đảm bảo hệ thống Camera của Scene mới hoàn toàn ổn định
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            yield return StartCoroutine(ShowSaveUIAndHandleCamera());
        }
    }

    private IEnumerator ShowSaveUIAndHandleCamera()
    {
        if (currentActiveUI != null) Destroy(currentActiveUI);

        currentActiveUI = Instantiate(saveCanvasPrefab);
        DontDestroyOnLoad(currentActiveUI);

        Canvas canvas = currentActiveUI.GetComponent<Canvas>();
        RectTransform canvasRect = currentActiveUI.GetComponent<RectTransform>();

        if (canvas != null && canvasRect != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            Camera targetCamera = null;
            int retryCount = 0;

            while (targetCamera == null && retryCount < 10) // Tăng số lần thử lên 10
            {
                GameObject camObj = GameObject.FindWithTag(uiCameraTag);
                if (camObj != null) targetCamera = camObj.GetComponent<Camera>();

                if (targetCamera == null) targetCamera = Camera.main;

                if (targetCamera == null)
                {
                    retryCount++;
                    yield return new WaitForSeconds(0.1f);
                }
            }

            if (targetCamera != null)
            {
                canvas.worldCamera = targetCamera;
                canvas.sortingOrder = 999;

                // QUAN TRỌNG: Đặt planeDistance cực thấp để nó hiện ngay trước mắt Camera
                canvas.planeDistance = 0.5f;

                // KHẮC PHỤC LỖI POS Z: 
                // Sau khi gán worldCamera, ta phải reset localPosition để Canvas khớp hoàn hảo với Camera
                canvasRect.localPosition = Vector3.zero;
                canvasRect.localRotation = Quaternion.identity;
                canvasRect.localScale = Vector3.one;

                Debug.Log($"[SaveSystem] Đã gán Camera {targetCamera.name} và reset Position Z.");
            }
            else
            {
                Debug.LogWarning("[SaveSystem] Không tìm thấy Camera. Chuyển về Overlay làm phương án dự phòng.");
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        yield return new WaitForSeconds(displayDuration);

        if (currentActiveUI != null) Destroy(currentActiveUI);
    }
}