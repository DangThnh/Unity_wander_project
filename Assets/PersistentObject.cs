using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObject : MonoBehaviour
{
    // Cờ để kiểm tra xem đối tượng này có nên được giữ lại không
    private bool isPersistent = false;

    // Tên của scene mà đối tượng này nên xuất hiện
    public string targetSceneName;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetPersistent(string sceneName)
    {
        isPersistent = true;
        targetSceneName = sceneName;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isPersistent)
        {
            // Chỉ hiển thị đối tượng nếu nó đang ở đúng scene
            if (scene.name == targetSceneName)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}