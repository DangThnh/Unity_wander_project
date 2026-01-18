using System.Collections;
using UnityEngine;

public class Character_movement : MonoBehaviour
{
    // Điểm xuất hiện mặc định (dùng khi lần đầu load scene).
    public Transform defaultSpawnPoint;

    // Cờ bật/tắt khả năng di chuyển.
    public bool canMove = true;

    // Tốc độ di chuyển. Đặt mặc định là 5.0f để đảm bảo nhân vật di chuyển được.
    public float speed = 5.0f;

    // Tốc độ xoay.
    public float rotationSpeed = 500.0f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("CẢNH BÁO: Không tìm thấy Animator component trên GameObject này.");
        }

        // Loại bỏ logic GameManager gây lỗi biên dịch CS1061.
        // Nếu có điểm spawn mặc định, dịch chuyển nhân vật tới đó.
        if (defaultSpawnPoint != null)
        {
            transform.position = defaultSpawnPoint.position;
            transform.rotation = defaultSpawnPoint.rotation;
        }
    }

    void Update()
    {
        if (GameState.isInputLocked) return;

        // Đảm bảo có Animator trước khi cố gắng gọi các hàm của nó
        if (animator == null) return;

        // Định nghĩa trạng thái di chuyển
        bool canMoveForward = Input.GetKey(KeyCode.W);
        bool canMoveBackward = Input.GetKey(KeyCode.S);

        if (canMove)
        {
            // Lấy input từ bàn phím (không cần dùng cho animation nữa)
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // ************** LOGIC ANIMATION MỚI **************
            // Gán trạng thái animation Tiến/Lùi
            animator.SetBool("isMovingForward", canMoveForward);
            animator.SetBool("isMovingBackward", canMoveBackward);
            // ************************************************

            // Xử lý xoay nhân vật (A/D)
            if (Input.GetKey(KeyCode.A))
            {
                // Xoay nhân vật sang trái
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                // Xoay nhân vật sang phải
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }

            // Xử lý di chuyển tiến/lùi
            if (canMoveForward)
            {
                // Di chuyển tiến về phía trước (hướng hiện tại của nhân vật)
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
            else if (canMoveBackward)
            {
                // Di chuyển lùi (hướng ngược lại của hướng hiện tại)
                transform.Translate(-Vector3.forward * speed * Time.deltaTime);
            }
        }
        else
        {
            // Nếu canMove = false, đảm bảo cả hai animation dừng lại (Idle).
            animator.SetBool("isMovingForward", false);
            animator.SetBool("isMovingBackward", false);
        }
    }
}