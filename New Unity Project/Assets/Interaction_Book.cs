using UnityEngine;
using TMPro; // Thêm namespace này để sử dụng TextMeshPro

public class Bookshelf : MonoBehaviour
{
    public TextMeshProUGUI interactionText_Book;
    public string myText = "Books, just be there for decoration.";

    private bool playerInRange = false;

    void Start()
    {
        if (interactionText_Book != null)
        {
            // Đảm bảo ban đầu text không hiển thị
            interactionText_Book.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu đối tượng va chạm là nhân vật (dựa vào tag)
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Ở đây, chúng ta sẽ KHÔNG BẬT text ngay lập tức
            // Chúng ta chỉ đơn giản là xác nhận nhân vật đã vào vùng
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Tắt text khi nhân vật rời đi, bất kể nó có đang hiển thị hay không
            if (interactionText_Book != null)
            {
                interactionText_Book.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Chỉ bật text nếu nhân vật ở trong vùng và người chơi nhấn phím E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (interactionText_Book != null)
            {
                // Bật text và gán nội dung mong muốn
                interactionText_Book.gameObject.SetActive(true);
                interactionText_Book.text = myText;
            }
        }
    }
}