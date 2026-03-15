using UnityEngine;

public class CameraInterScene3 : MonoBehaviour
{
    public enum CameraMode { FollowOffset, FixedPath }

    [Header("Cấu hình mục tiêu")]
    public Transform player;
    public Transform mainCamera;

    [Header("Chế độ hoạt động")]
    public CameraMode currentMode = CameraMode.FollowOffset;

    [Header("Thông số Offset (Dành cho FollowOffset)")]
    public Vector3 defaultOffset = new Vector3(0, 5, -10);
    public Vector3 climaxOffset = new Vector3(15, 2, 0);
    public Vector3 defaultRotation = new Vector3(30, 0, 0);
    public Vector3 climaxRotation = new Vector3(10, -90, 0);

    [Header("Thông số Path (Dành cho FixedPath)")]
    public Transform pathStart;
    public Transform pathEnd;

    [Range(0, 1)]
    public float transitionProgress = 0f;
    public float smoothSpeed = 10f; // Tăng tốc độ mượt để bám sát hơn

    private Vector3 targetPos;
    private Quaternion targetRot;

    public void SetProgress(float value)
    {
        transitionProgress = Mathf.Clamp01(value);
    }

    public void SwitchMode(CameraMode mode)
    {
        currentMode = mode;
    }

    void LateUpdate()
    {
        if (player == null || mainCamera == null) return;

        CalculateCameraLogic();
        ApplySmoothMovement();

        if (mainCamera != null)
        {
            Camera cam = mainCamera.GetComponent<Camera>();
            if (cam != null)
            {
                // Đảm bảo cullingMask bao gồm cả Layer của nhân vật
                // Giả sử nhân vật ở Layer "Player" hoặc Layer 0 (Default)
                // cam.cullingMask |= (1 << LayerMask.NameToLayer("YourCharacterLayer"));

                Debug.Log("Camera " + cam.name + " đang render với FOV: " + cam.fieldOfView);
            }
        }
    }

    void CalculateCameraLogic()
    {
        // Tính toán t để nội suy giữa Default và Climax
        float t = Mathf.SmoothStep(0, 1, 1f - Mathf.Abs(2f * transitionProgress - 1f));

        if (currentMode == CameraMode.FollowOffset)
        {
            // Chế độ 1: Camera di chuyển dựa trên vị trí Player + Offset cố định
            Vector3 currentOffset = Vector3.Lerp(defaultOffset, climaxOffset, t);
            targetPos = player.position + currentOffset;

            Vector3 currentRotEuler = Vector3.Lerp(defaultRotation, climaxRotation, t);
            targetRot = Quaternion.Euler(currentRotEuler);
        }
        else if (currentMode == CameraMode.FixedPath)
        {
            // Chế độ 2: Camera trượt trên một đường thẳng cố định (Path)
            if (pathStart != null && pathEnd != null)
            {
                targetPos = Vector3.Lerp(pathStart.position, pathEnd.position, transitionProgress);
                // Camera luôn nhìn về phía Player khi ở trên Path
                Vector3 direction = player.position - targetPos;
                if (direction != Vector3.zero)
                {
                    targetRot = Quaternion.LookRotation(direction);
                }
            }
        }
    }

    void ApplySmoothMovement()
    {
        // Sử dụng SmoothDamp hoặc Lerp nhưng với tốc độ cao để tránh trễ
        mainCamera.position = Vector3.Lerp(mainCamera.position, targetPos, Time.deltaTime * smoothSpeed);
        mainCamera.rotation = Quaternion.Slerp(mainCamera.rotation, targetRot, Time.deltaTime * smoothSpeed);
    }
}