using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject introCamera;

    void Awake()
    {
        // Khóa Input ngay khi vào Scene
        GameState.isInputLocked = true;
        
        if (CameraManager.instance != null)
            CameraManager.instance.isCutscenePlaying = true;
    }

    void Start()
    {
        director.stopped += OnCutsceneFinished;
    }

    void OnCutsceneFinished(PlayableDirector aDirector)
    {
        // 1. Tắt Intro Camera trước
        if (introCamera != null) introCamera.SetActive(false);

        // 2. Mở khóa Input
        GameState.isInputLocked = false;

        // 3. Xử lý CameraManager qua một Coroutine để đảm bảo tính ổn định
        StartCoroutine(EnableGameplayCamera());

        director.Stop();
    }

    IEnumerator EnableGameplayCamera()
    {
        if (CameraManager.instance != null)
        {
            // 1. Mở khóa trạng thái trước
            CameraManager.instance.isCutscenePlaying = false;

            // 2. Ép CameraManager quét lại và bật MainCamera TRƯỚC
            CameraManager.instance.InitializeCamerasForNewScene();

            // 3. Đợi một nhịp cực ngắn để Unity đăng ký Camera mới
            yield return new WaitForEndOfFrame();

            // 4. SAU ĐÓ MỚI TẮT Intro Camera
            if (introCamera != null)
            {
                introCamera.SetActive(false);
                Debug.Log("Intro Camera đã tắt.");
            }
        }
    }
}