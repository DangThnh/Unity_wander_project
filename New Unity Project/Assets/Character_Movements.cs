using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_movement : MonoBehaviour
{
    public Transform defaultSpawnPoint;
    public bool canMove = true;
    public float speed;
    public float rotationSpeed;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {   
        //if (GameManager.instance != null)
        //{
        //    // Dịch chuyển nhân vật đến vị trí đã lưu
        //    transform.position = GameManager.instance.spawnPosition;
        //    transform.rotation = GameManager.instance.spawnRotation;
        //}
        animator = GetComponent<Animator>();

        if (GameManager.instance != null && GameManager.instance.isFirstLoad)
        {
            // Dịch chuyển nhân vật đến vị trí mặc định
            if (defaultSpawnPoint != null)
            {
                transform.position = defaultSpawnPoint.position;
                transform.rotation = defaultSpawnPoint.rotation;
            }
            // Đặt lại biến để các lần tải sau sẽ dùng vị trí của GameManager
            GameManager.instance.isFirstLoad = false;
        }
        else if (GameManager.instance != null)
        {
            // Dịch chuyển nhân vật đến vị trí đã lưu từ scene trước
            transform.position = GameManager.instance.spawnPosition;
            transform.rotation = GameManager.instance.spawnRotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            // Lấy input từ bàn phím
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // Tạo vector di chuyển
            Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);

            // Kiểm tra xem nhân vật có di chuyển không
            bool isMoving = movementDirection.magnitude > 0;

            // Cập nhật tham số "IsMoving" của Animator
            animator.SetBool("IsMoving", isMoving);

            // Xử lý xoay nhân vật
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
            if (Input.GetKey(KeyCode.W))
            {
                // Di chuyển tiến về phía trước (hướng hiện tại của nhân vật)
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                // Di chuyển lùi (hướng ngược lại của hướng hiện tại)
                transform.Translate(-Vector3.forward * speed * Time.deltaTime);
            }

        }
    }
}