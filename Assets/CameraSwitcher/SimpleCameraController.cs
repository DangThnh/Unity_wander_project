using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    public float movementSpeed = 10.0f;
    public float rotationSpeed = 100.0f;

    void Update()
    {
        // Di chuyển camera theo các phím WASD
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        transform.position += transform.forward * verticalInput * movementSpeed * Time.deltaTime;
        transform.position += transform.right * horizontalInput * movementSpeed * Time.deltaTime;

        // Xoay camera bằng chuột
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        transform.eulerAngles += new Vector3(-mouseY * rotationSpeed * Time.deltaTime, mouseX * rotationSpeed * Time.deltaTime, 0);
    }
}