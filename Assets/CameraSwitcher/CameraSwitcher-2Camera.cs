using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSwitcher2Camera : MonoBehaviour
{
    private GameObject firstCamera;
    private GameObject secondCamera;

    void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Tìm và gán lại tham chiếu đến các camera trong scene mới
        firstCamera = GameObject.Find("Main Camera");
        secondCamera = GameObject.Find("Second Camera");

        // Bật camera chính và tắt camera phụ khi scene được tải
        if (firstCamera != null)
        {
            firstCamera.SetActive(true);
        }
        if (secondCamera != null)
        {
            secondCamera.SetActive(false);
        }
    }

    void Update()
    {
        // Nhấn phím M để chuyển đổi camera
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Kiểm tra xem camera hiện tại có đang bật không
            if (firstCamera != null && firstCamera.activeSelf)
            {
                // Nếu camera 2 có tồn tại, bật nó lên trước khi tắt camera 1
                if (secondCamera != null)
                {
                    firstCamera.SetActive(false);
                    secondCamera.SetActive(true);
                }
            }
            // Ngược lại, nếu camera 2 đang bật
            else if (secondCamera != null && secondCamera.activeSelf)
            {
                // Nếu camera 1 có tồn tại, bật nó lên trước khi tắt camera 2
                if (firstCamera != null)
                {
                    secondCamera.SetActive(false);
                    firstCamera.SetActive(true);
                    
                }
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}