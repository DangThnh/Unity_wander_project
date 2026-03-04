using UnityEngine;

public class ProximityMultiLight : MonoBehaviour
{
    [Header("Cấu hình Đèn")]
    [Tooltip("Danh sách các đèn cần điều khiển. Nhấn dấu + để thêm đèn.")]
    public Light[] targetLights;

    [Header("Điều kiện kích hoạt")]
    [Tooltip("Bán kính tối đa để bật đèn")]
    public float detectionRadius = 5f;

    [Tooltip("Thời gian chờ (giây) trước khi đèn tắt sau khi điều kiện không còn thỏa mãn")]
    public float turnOffDelay = 2.0f;

    [Header("Tham chiếu")]
    [Tooltip("Kéo Player vào đây, nếu để trống script sẽ tự tìm qua Tag 'Player'")]
    public Transform playerTransform;

    private float timer;
    private bool shouldBeOn;

    void Start()
    {
        // Tự động tìm Player nếu chưa gán
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // Khởi tạo trạng thái ban đầu cho tất cả đèn trong danh sách
        SetLightsState(false);
    }

    void Update()
    {
        if (playerTransform == null || targetLights == null || targetLights.Length == 0) return;

        // 1. Kiểm tra các điều kiện
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool isInRange = distance <= detectionRadius;
        bool isLowerThanLight = playerTransform.position.y < transform.position.y;

        // Điều kiện tổng hợp
        bool currentConditionsMet = isInRange && isLowerThanLight;

        if (currentConditionsMet)
        {
            // Nếu thỏa mãn, bật tất cả đèn và reset bộ đếm
            SetLightsState(true);
            timer = turnOffDelay;
            shouldBeOn = true;
        }
        else
        {
            // Nếu không thỏa mãn, bắt đầu đếm ngược
            if (shouldBeOn)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    SetLightsState(false);
                    shouldBeOn = false;
                }
            }
        }
    }

    // Hàm bổ trợ để bật/tắt toàn bộ danh sách đèn
    private void SetLightsState(bool state)
    {
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                light.enabled = state;
            }
        }
    }

    // Vẽ công cụ hỗ trợ trực quan trong Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Vector3 planeSize = new Vector3(detectionRadius * 2, 0.01f, detectionRadius * 2);
        Gizmos.DrawCube(transform.position, planeSize);
    }
}