using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BookInteraction : MonoBehaviour
{
    [System.Serializable]
    public class BookData
    {
        public string bookName;
        public Sprite coverImage; // Ảnh bìa
        public List<Sprite> pageImages; // Danh sách ảnh các trang (đã có sẵn chữ)
    }

    [Header("Dữ liệu sách")]
    public BookData bookData;

    [Header("Tham chiếu UI")]
    public GameObject bookUIPanel;
    public Image displayImage; // Image component chính để hiển thị ảnh

    [Header("Âm thanh")]
    public AudioSource audioSource;
    public AudioClip flipSound; // Âm thanh lật sách
    public AudioClip openSound; // Âm thanh mở sách

    private bool isPlayerInRange = false;
    private bool isBookOpen = false;
    private int currentPageIndex = -1; // -1 là đang ở bìa, 0 trở đi là các trang

    void Start()
    {
        if (bookUIPanel != null) bookUIPanel.SetActive(false);

        // Tự động lấy AudioSource nếu chưa kéo vào
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Sử dụng phím E cho tất cả mọi thao tác
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            AdvanceBookLogic();
        }
    }

    void AdvanceBookLogic()
    {
        // Trường hợp 1: Sách đang đóng -> Mở bìa
        if (!isBookOpen)
        {
            OpenBook();
        }
        // Trường hợp 2: Đang ở bìa -> Sang trang đầu tiên
        else if (currentPageIndex == -1)
        {
            NextPage();
        }
        // Trường hợp 3: Đang đọc các trang nội dung
        else
        {
            if (currentPageIndex < bookData.pageImages.Count - 1)
            {
                NextPage();
            }
            else
            {
                // Nếu đã ở trang cuối cùng -> Đóng sách
                CloseBook();
            }
        }
    }

    void OpenBook()
    {
        isBookOpen = true;
        currentPageIndex = -1; // Thiết lập trạng thái đang ở bìa
        bookUIPanel.SetActive(true);
        displayImage.sprite = bookData.coverImage;

        PlaySound(openSound);
        Debug.Log("Đã mở sách: " + bookData.bookName);
    }

    void NextPage()
    {
        currentPageIndex++;
        if (currentPageIndex < bookData.pageImages.Count)
        {
            displayImage.sprite = bookData.pageImages[currentPageIndex];
            PlaySound(flipSound);
            Debug.Log("Sang trang: " + (currentPageIndex + 1));
        }
    }

    void CloseBook()
    {
        isBookOpen = false;
        currentPageIndex = -1;
        bookUIPanel.SetActive(false);

        PlaySound(flipSound); // Tiếng gấp sách
        Debug.Log("Đã đóng sách");
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Bạn có thể hiện thông báo "Bấm E để đọc" tại đây
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (isBookOpen) CloseBook();
        }
    }
}