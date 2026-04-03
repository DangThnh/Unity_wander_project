using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ImageReader : MonoBehaviour
{
    [Header("Cấu hình UI (Kéo từ Canvas vào đây)")]
    public GameObject uiPanel;          // Panel chứa toàn bộ UI đọc sách
    public Image backgroundImage;       // Image hiển thị ảnh bìa
    public Image darkOverlay;          // Lớp màu đen mờ (nằm trên ảnh bìa)
    public TMP_Text contentText;        // Chữ nội dung chính
    public TMP_Text promptText;         // Chữ hướng dẫn (Nhấn E để...)

    [Header("Dữ liệu của riêng đối tượng này")]
    public Sprite bookCover;            // Ảnh bìa riêng cho cuốn sách này
    [TextArea(5, 10)]
    public List<string> pages = new List<string>(); // Danh sách các trang nội dung

    // Biến static để các script khác có thể truy cập mà không cần tham chiếu trực tiếp
    // Giúp kiểm tra: if (ImageReader.IsReading) { // Khóa phím C, M }
    public static bool IsReadingStatus = false;

    private int currentPageIndex = 0;
    private bool isPlayerNearby = false;
    private bool isReading = false;

    void Start()
    {
        // Đảm bảo UI luôn tắt khi bắt đầu game
        if (uiPanel != null) uiPanel.SetActive(false);

        // Cấu hình lớp đen mờ nếu có
        if (darkOverlay != null)
        {
            Color c = darkOverlay.color;
            c.a = 0.5f; // Độ đậm nhạt của lớp phủ đen (0 đến 1)
            darkOverlay.color = c;
        }
    }

    void Update()
    {
        // Kiểm tra tương tác mở sách
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Nếu Inventory hoặc Bản đồ đang mở (giả sử bạn có biến check ở nơi khác)
            // thì có thể thêm logic ngăn cản mở sách ở đây.

            if (!isReading)
            {
                OpenBook();
            }
            else
            {
                NextPage();
            }
        }

        // Logic bổ sung: Nếu đang đọc, vô hiệu hóa các hành động khác
        if (isReading)
        {
            // Chúng ta không cần làm gì ở đây nếu các script khác (Inventory) 
            // chủ động kiểm tra biến ImageReader.IsReadingStatus
        }
    }

    void OpenBook()
    {
        if (pages.Count == 0) return;

        isReading = true;
        IsReadingStatus = true; // Cập nhật trạng thái toàn cục
        currentPageIndex = 0;

        // Cập nhật hình ảnh và nội dung riêng của object này lên UI chung
        if (backgroundImage != null) backgroundImage.sprite = bookCover;

        UpdateUIContent();
        uiPanel.SetActive(true);

        // Vô hiệu hóa di chuyển nhân vật (Nếu script Character_movement có biến canMove)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var moveScript = player.GetComponent<Character_movement>();
            if (moveScript != null) moveScript.canMove = false;
        }
    }

    void NextPage()
    {
        currentPageIndex++;

        if (currentPageIndex < pages.Count)
        {
            UpdateUIContent();
        }
        else
        {
            CloseBook();
        }
    }

    void UpdateUIContent()
    {
        if (contentText != null) contentText.text = pages[currentPageIndex];

        if (promptText != null)
        {
            promptText.text = (currentPageIndex == pages.Count - 1)
                ? "Press E to close"
                : "Press E to continue";
        }
    }

    public void CloseBook()
    {
        isReading = false;
        IsReadingStatus = false; // Giải phóng trạng thái toàn cục
        uiPanel.SetActive(false);

        // Mở khóa di chuyển nhân vật
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var moveScript = player.GetComponent<Character_movement>();
            if (moveScript != null) moveScript.canMove = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (isReading) CloseBook();
        }
    }
}