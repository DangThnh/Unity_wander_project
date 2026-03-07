using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class FootstepSystem : MonoBehaviour
{
    [System.Serializable]
    public class TerrainAudio
    {
        public string tag; // Ví dụ: "Grass", "Stone"
        public List<AudioClip> clips; // Danh sách các tiếng bước chân cho địa hình này
    }

    [Header("Cấu hình âm thanh")]
    public List<TerrainAudio> terrainAudios; // Kéo thả các loại địa hình vào đây trong Inspector

    [Header("Cài đặt Raycast")]
    public float rayDistance = 0.5f; // Khoảng cách tia bắn xuống đất
    public LayerMask floorLayer; // Layer của mặt đất để tránh bắn trúng chính nhân vật

    private AudioSource audioSource;
    private Character_movement movementScript; // Tham chiếu đến script di chuyển của bạn

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        movementScript = GetComponent<Character_movement>();
    }

    // Hàm này sẽ được gọi thông qua Animation Event (Sự kiện trong Animation)
    // Hoặc gọi thủ công nếu bạn không dùng Animation
    public void PlayFootstepSound()
    {
        // Chỉ phát tiếng khi nhân vật đang di chuyển (dựa trên script di chuyển của bạn)
        if (movementScript != null && !movementScript.canMove) return;

        RaycastHit hit;
        // Bắn một tia từ vị trí chân (transform.position) xuống dưới
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, rayDistance, floorLayer))
        {
            string surfaceTag = hit.collider.tag;
            PlaySoundFromTag(surfaceTag);
        }
    }

    private void PlaySoundFromTag(string tag)
    {
        // Tìm loại âm thanh khớp với Tag
        foreach (var terrain in terrainAudios)
        {
            if (terrain.tag == tag && terrain.clips.Count > 0)
            {
                // Chọn ngẫu nhiên một clip trong danh sách để âm thanh tự nhiên hơn
                AudioClip randomClip = terrain.clips[Random.Range(0, terrain.clips.Count)];

                // Chỉnh âm lượng và độ cao (pitch) ngẫu nhiên một chút để không bị nhàm chán
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(randomClip);
                return;
            }
        }
    }
}