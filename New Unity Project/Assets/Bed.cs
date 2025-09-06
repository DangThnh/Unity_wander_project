using UnityEngine;
using TMPro; // Thêm namespace này để sử dụng TextMeshPro

public class Bed : MonoBehaviour
{
    public TextMeshProUGUI interactionText_Bed;
    public string myText = "I don't think i want to sleep now.";

    private bool playerInRange = false;

    void Start()
    {
        if (interactionText_Bed != null)
        {
            // Đảm bảo ban đầu text không hiển thị
            interactionText_Bed.gameObject.SetActive(false);
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
            if (interactionText_Bed != null)
            {
                interactionText_Bed.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Chỉ bật text nếu nhân vật ở trong vùng và người chơi nhấn phím E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (interactionText_Bed != null)
            {
                // Bật text và gán nội dung mong muốn
                interactionText_Bed.gameObject.SetActive(true);
                interactionText_Bed.text = myText;
            }
        }
    }
}