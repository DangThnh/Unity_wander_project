using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    public PlayableDirector director;

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
        // Mở khóa Input
        GameState.isInputLocked = false;

        if (CameraManager.instance != null)
        {
            CameraManager.instance.isCutscenePlaying = false;
            CameraManager.instance.InitializeCamerasForNewScene();
        }

        director.Stop(); // Giải phóng Camera
    }
}