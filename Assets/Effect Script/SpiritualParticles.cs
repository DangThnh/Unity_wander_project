using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class SpiritualHintTrigger : MonoBehaviour
{
    [Header("Đối tượng linh hồn")]
    [Tooltip("Kéo Particle System linh hồn vào đây.")]
    public ParticleSystem spiritParticle;

    [Header("Điểm xuất hiện (Tùy chọn)")]
    [Tooltip("Nếu để trống, linh hồn sẽ hiện ra tại vị trí của Particle System hiện tại.")]
    public Transform spawnPoint;

    [Header("Lộ trình di chuyển")]
    [Tooltip("Danh sách các điểm mốc mà linh hồn sẽ bay qua.")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Thông số chuyển động")]
    public float moveSpeed = 5f;
    public float arrivalDistance = 0.5f;

    private int currentWaypointIndex = 0;
    private bool isRunning = false;
    private bool hasTriggered = false;

    void Start()
    {
        // Đảm bảo Box Collider là Trigger
        GetComponent<BoxCollider>().isTrigger = true;

        if (spiritParticle != null)
        {
            spiritParticle.Stop();
        }
    }

    void Update()
    {
        if (isRunning && spiritParticle != null)
        {
            MoveAlongWaypoints();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra Tag Player và chỉ kích hoạt 1 lần
        if (other.CompareTag("Player") && !hasTriggered)
        {
            ActivateHint();
        }
    }

    private void ActivateHint()
    {
        if (spiritParticle == null || waypoints.Count < 1) return;

        hasTriggered = true;

        // Nếu có điểm spawn riêng, dịch chuyển linh hồn tới đó trước khi phát hạt
        if (spawnPoint != null)
        {
            spiritParticle.transform.position = spawnPoint.position;
        }
        else if (waypoints.Count > 0)
        {
            // Nếu không có spawnPoint, mặc định bắt đầu tại waypoint đầu tiên
            spiritParticle.transform.position = waypoints[0].position;
        }

        currentWaypointIndex = 0;
        isRunning = true;
        spiritParticle.Play();
    }

    private void MoveAlongWaypoints()
    {
        if (currentWaypointIndex >= waypoints.Count)
        {
            StopHint();
            return;
        }

        Transform target = waypoints[currentWaypointIndex];

        // Di chuyển Particle System theo từng mốc
        spiritParticle.transform.position = Vector3.MoveTowards(
            spiritParticle.transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // Kiểm tra nếu đã chạm mốc
        if (Vector3.Distance(spiritParticle.transform.position, target.position) < arrivalDistance)
        {
            currentWaypointIndex++;
        }
    }

    private void StopHint()
    {
        isRunning = false;
        if (spiritParticle != null)
        {
            spiritParticle.Stop();
        }
    }

    // Vẽ đường nối trong Scene để dễ quan sát lộ trình
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] && waypoints[i + 1])
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        // Vẽ biểu tượng điểm Spawn nếu có
        if (spawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            if (waypoints.Count > 0)
                Gizmos.DrawLine(spawnPoint.position, waypoints[0].position);
        }
    }
}