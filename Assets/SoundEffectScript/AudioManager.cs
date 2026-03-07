using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource musicSource;
    private AudioClip currentClip;

    [Header("Cấu hình chuyển nhạc")]
    public float fadeDuration = 1.0f; // Thời gian nhỏ dần/to dần khi đổi nhạc

    void Awake()
    {
        // Singleton Pattern: Đảm bảo chỉ có 1 AudioManager tồn tại xuyên suốt các Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = GetComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (newClip == null || currentClip == newClip) return;

        StopAllCoroutines();
        StartCoroutine(FadeMusicTransition(newClip));
    }

    private IEnumerator FadeMusicTransition(AudioClip newClip)
    {
        // Nhỏ dần nhạc cũ
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
                yield return null;
            }
        }

        // Đổi clip và phát nhạc mới
        currentClip = newClip;
        musicSource.clip = newClip;
        musicSource.Play();

        // To dần nhạc mới
        while (musicSource.volume < 1.0f)
        {
            musicSource.volume += Time.deltaTime / fadeDuration;
            yield return null;
        }
        musicSource.volume = 1.0f;
    }
}