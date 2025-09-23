using UnityEngine;
using TMPro; // Thêm namespace này để sử dụng TextMeshPro

public class InteractableObject2 : MonoBehaviour
{
    public TextMeshProUGUI interactionText2;
    public string myText = "My laptop, don't wanna touch it for now.";

    private bool playerInRange = false;

    void Start()
    {
        if (interactionText2 != null)
        {
            // Đảm bảo ban đầu text không hiển thị
            interactionText2.gameObject.SetActive(false);
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
            if (interactionText2 != null)
            {
                interactionText2.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Chỉ bật text nếu nhân vật ở trong vùng và người chơi nhấn phím E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (interactionText2 != null)
            {
                // Bật text và gán nội dung mong muốn
                interactionText2.gameObject.SetActive(true);
                interactionText2.text = myText;
            }
        }
    }
}