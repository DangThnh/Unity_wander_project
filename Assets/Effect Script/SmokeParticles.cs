using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiSmokeController : MonoBehaviour
{
    [Header("Danh sách các Particle Systems")]
    [Tooltip("Kéo tất cả các Particle System (ống khói 1, ống khói 2...) vào đây")]
    public List<ParticleSystem> smokeSystems = new List<ParticleSystem>();

    [Header("Cấu hình Nhịp điệu (Giây)")]
    public float sprayDuration = 3.0f;
    public float restDuration = 2.0f;

    [Header("Tùy chọn Âm thanh (Nếu có)")]
    public AudioSource spraySound;

    [Header("Tùy chọn Ngẫu nhiên")]
    [Range(0, 1)]
    public float randomness = 0.2f;

    void Start()
    {
        // Nếu danh sách trống, thử tìm Particle trên chính Object này
        if (smokeSystems.Count == 0)
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();
            if (ps != null) smokeSystems.Add(ps);
        }

        if (smokeSystems.Count > 0)
        {
            StartCoroutine(SmokeRoutine());
        }
        else
        {
            Debug.LogWarning("Chưa gán Particle System nào vào MultiSmokeController!");
        }
    }

    IEnumerator SmokeRoutine()
    {
        while (true)
        {
            // Bước 1: Bắt đầu phun khói cho tất cả các ống
            SetAllEmission(true);

            if (spraySound != null) spraySound.Play();

            // Thêm một chút ngẫu nhiên để không quá máy móc
            float actualSprayTime = sprayDuration + Random.Range(-randomness, randomness);
            yield return new WaitForSeconds(Mathf.Max(0.1f, actualSprayTime));

            // Bước 2: Ngừng phun khói
            SetAllEmission(false);

            if (spraySound != null) spraySound.Stop();

            float actualRestTime = restDuration + Random.Range(-randomness, randomness);
            yield return new WaitForSeconds(Mathf.Max(0.1f, actualRestTime));
        }
    }

    void SetAllEmission(bool enable)
    {
        foreach (var ps in smokeSystems)
        {
            if (ps == null) continue;

            var emission = ps.emission;
            emission.enabled = enable;

            // Đảm bảo hệ thống đang chạy để xử lý các hạt cũ
            if (enable && !ps.isPlaying)
            {
                ps.Play();
            }
        }
    }

    // Hàm để kích hoạt thủ công từ các script khác (ví dụ: khi xe khởi động)
    public void ActivateSmoke()
    {
        StopAllCoroutines();
        StartCoroutine(SmokeRoutine());
    }

    public void DeactivateSmoke()
    {
        StopAllCoroutines();
        SetAllEmission(false);
        if (spraySound != null) spraySound.Stop();
    }
}