using UnityEngine;

public class HexaPuzzleAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip rotateClip;     // Tiếng xoay hình trụ
    public AudioClip switchClip;     // Tiếng đổi hình trụ (Trái/Phải)
    public AudioClip correctClip;    // Tiếng báo đúng (Ting!)
    public AudioClip errorClip;      // Tiếng báo sai (Bíp)

    public void PlayRotate() => PlaySound(rotateClip);
    public void PlaySwitch() => PlaySound(switchClip);
    public void PlayCorrect() => PlaySound(correctClip);
    public void PlayError() => PlaySound(errorClip);

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            // Thay đổi pitch nhẹ để âm thanh không bị nhàm chán
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clip);
        }
    }
}