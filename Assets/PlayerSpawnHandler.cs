using UnityEngine;

// Script này trước đây dùng để xử lý việc dịch chuyển nhân vật đến một điểm spawn cụ thể 
// sau khi chuyển scene. Vì logic điểm spawn đã được loại bỏ theo yêu cầu, 
// script này giờ đây không thực hiện bất kỳ hành động dịch chuyển nào khi scene tải.
public class PlayerSpawnHandler : MonoBehaviour
{
    void Start()
    {
        // Toàn bộ logic kiểm tra và dịch chuyển dựa trên 'desiredSpawnPointName' đã được loại bỏ.
        // Nhân vật sẽ xuất hiện tại vị trí được xác định sẵn trong Scene đích.

        // Fix lỗi: Sử dụng 'instance' thay vì 'Instance' để truy cập Singleton, dựa trên lỗi CS0117.
        // Nếu GameManager được định nghĩa là public static GameManager instance;
        if (GameManager.instance == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy GameManager.instance. Hãy đảm bảo GameManager đã được khởi tạo.");
        }

        // Nếu cần thay đổi vị trí nhân vật sau khi chuyển cảnh, bạn nên thực hiện điều đó
        // trong một SceneController ở Scene đích để tránh phụ thuộc vào biến toàn cục.
    }
}