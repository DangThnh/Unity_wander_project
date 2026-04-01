using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class SceneAspectRatioFixer : MonoBehaviour
{
    private Camera targetCamera;
    private const float TARGET_ASPECT = 4.0f / 3.0f;

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
        ApplyAspectRatio();

        // Đảm bảo khi load scene mới, camera vẫn giữ tỉ lệ này
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyAspectRatio();
    }

    public void ApplyAspectRatio()
    {
        if (targetCamera == null) return;

        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / TARGET_ASPECT;

        if (scaleHeight < 1.0f)
        {
            Rect rect = targetCamera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            targetCamera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = targetCamera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            targetCamera.rect = rect;
        }
    }

#if UNITY_EDITOR
    void Update() => ApplyAspectRatio();
#endif
}