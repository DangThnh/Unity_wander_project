using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    [Header("Cấu hình Material")]
    public Renderer targetRenderer;
    public string colorPropertyName = "_EmissionColor"; // Tên biến trong Shader

    [Header("Cấu hình Nhấp nháy")]
    public Color glowColor = Color.white;
    public float minIntensity = 0.5f;
    public float maxIntensity = 3.0f;
    public float pulseSpeed = 2.0f;

    private Material targetMaterial;

    void Start()
    {
        // Khởi tạo material thực thể để không làm thay đổi file gốc
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        targetMaterial = targetRenderer.material;

        // Kích hoạt từ khóa Emission (quan trọng đối với Standard Shader)
        targetMaterial.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // Tính toán độ sáng dựa trên hàm Sin (tạo vòng lặp mượt mà)
        float lerp = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, lerp);

        // Áp dụng màu sắc với cường độ HDR
        // Trong Unity, màu Emission thực tế là Màu * Cường độ (2^intensity)
        Color finalColor = glowColor * Mathf.LinearToGammaSpace(currentIntensity);

        targetMaterial.SetColor(colorPropertyName, finalColor);
    }
}