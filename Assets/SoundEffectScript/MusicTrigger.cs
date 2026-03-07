using UnityEngine;

public class SceneMusicTrigger : MonoBehaviour
{
    [Header("Kéo file nhạc cho Scene này vào đây")]
    public AudioClip sceneBGM;

    void Start()
    {
        // Ngay khi Scene load xong, gọi AudioManager để phát nhạc
        if (AudioManager.Instance != null && sceneBGM != null)
        {
            AudioManager.Instance.PlayMusic(sceneBGM);
        }
        else if (AudioManager.Instance == null)
        {
            Debug.LogWarning("SceneMusicTrigger: Không tìm thấy AudioManager trong Scene đầu tiên!");
        }
    }
}