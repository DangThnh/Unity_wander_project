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
        // Kiểm tra tương tác
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!isReading)
            {
                OpenBook();
            }
            else
            {
                NextPage();
            }
        }
    }

    void OpenBook()
    {
        if (pages.Count == 0) return;

        isReading = true;
        currentPageIndex = 0;

        // Cập nhật hình ảnh và nội dung riêng của object này lên UI chung
        if (backgroundImage != null) backgroundImage.sprite = bookCover;

        UpdateUIContent();
        uiPanel.SetActive(true);

        // (Tùy chọn) Khóa di chuyển nhân vật ở đây nếu cần
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
        uiPanel.SetActive(false);
        // (Tùy chọn) Mở khóa di chuyển nhân vật ở đây
    }

    // Phát hiện người chơi đi vào vùng tương tác
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // Bạn có thể hiện một icon "Press E" nhỏ ở đây
        }
    }

    // Phát hiện người chơi đi ra khỏi vùng tương tác
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (isReading) CloseBook(); // Tự đóng sách nếu người chơi bỏ chạy
        }
    }
}