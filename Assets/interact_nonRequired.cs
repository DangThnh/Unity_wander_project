using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class InteractionManager_NonRequiredButton : MonoBehaviour
{
    // Cài đặt chuyển phòng
    public string destinationSceneName;

    // Cài đặt UI
    // Điểm xuất hiện trong scene mới
    public string destinationSpawnPointName;
    //public GameObject questionPanel;
    //public TextMeshProUGUI yesText;
    //public TextMeshProUGUI noText;
    public string myQuestion = "Do you want to step across?";
    public string myYesText = "Yes";
    public string myNoText = "No";

    // Cài đặt trạng thái
    private bool playerInRange = false;
    private int selectedOption = 0; // 0 = Yes, 1 = No
    private bool isInteracting = false; // Trạng thái tương tác

    // Tham chiếu đến script điều khiển nhân vật
    private Character_movement playerController;

    private Animator playerAnimator;

    // Thiết lập kích thước font chữ
    private float defaultFontSize = 36f;
    private float selectedFontSize = 48f;

    void Start()
    {
        //if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        //{
        //    GameManager.instance.questionPanel.SetActive(false);
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // Lấy tham chiếu đến script điều khiển nhân vật
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();

            // Bắt đầu coroutine để đợi UI sẵn sàng trước khi hiển thị
            StartCoroutine(WaitForUIReadyAndShow());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Tắt UI nếu nhân vật rời đi và không tương tác
            if (!isInteracting)
            {
                HideUI();
            }
        }
    }

    void Update()
    {
        //// Bắt đầu tương tác khi nhân vật ở trong vùng và bấm E
        if (playerInRange && !isInteracting)
        {
            ShowUI();
        }
        // Kết thúc tương tác khi UI đang bật và người chơi bấm E
        else if (isInteracting && Input.GetKeyDown(KeyCode.Return))
        {
            // Thoát khỏi tương tác mà không cần chuyển scene
            if (selectedOption == 1) // Kiểm tra nếu chọn No
            {
                EndInteraction();
            }
        }

        // Xử lý khi UI đang bật
        if (isInteracting)
        {
            HandleUIInput();
        }
    }

    // Coroutine để đợi UI được gán tham chiếu an toàn
    private IEnumerator WaitForUIReadyAndShow()
    {
        // Chờ cho đến khi GameManager và các tham chiếu UI của nó không còn rỗng
        while (GameManager.instance == null || GameManager.instance.questionPanel == null)
        {
            yield return null; // Đợi khung hình tiếp theo
        }

        // Bây giờ việc hiển thị UI đã an toàn
        if (!isInteracting)
        {
            ShowUI();
        }
    }

    //void ShowUI()
    //{
    //    isInteracting = true;
    //    questionPanel.SetActive(true);
    //    selectedOption = 0; // Mặc định chọn Yes
    //    UpdateSelectionUI();

    //    // Khóa chuyển động của nhân vật
    //    if (playerController != null)
    //    {
    //        playerController.canMove = false;
    //    }
    //    if (playerAnimator != null)
    //    {
    //        // Dừng mọi animation bằng cách đặt biến trạng thái di chuyển thành false
    //        playerAnimator.SetBool("IsMoving", false);
    //    }
    //}
    void ShowUI()
    {
        isInteracting = true;

        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(true);
            selectedOption = 0; // Mặc định chọn Yes
            UpdateSelectionUI();

            // Cập nhật text hiển thị trên các nút
            if (GameManager.instance.yesText != null)
            {
                GameManager.instance.yesText.text = myYesText;
            }
            if (GameManager.instance.noText != null)
            {
                GameManager.instance.noText.text = myNoText;
            }

            // Khóa chuyển động của nhân vật
            if (playerController != null)
            {
                playerController.canMove = false;
            }
            if (playerAnimator != null)
            {
                // Dừng mọi animation bằng cách đặt biến trạng thái di chuyển thành false
                playerAnimator.SetBool("IsMoving", false);
            }
        }
    }

    //void HideUI()
    //{
    //    isInteracting = false;
    //    questionPanel.SetActive(false);

    //    // Mở khóa chuyển động của nhân vật
    //    if (playerController != null)
    //    {
    //        playerController.canMove = true;
    //    }
    //}

    void HideUI()
    {
        isInteracting = false;
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(false);
        }

        // Mở khóa chuyển động của nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }

    void UpdateSelectionUI()
    {
        if (GameManager.instance != null)
        {
            if (GameManager.instance.yesText != null)
            {
                GameManager.instance.yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
            }
            if (GameManager.instance.noText != null)
            {
                GameManager.instance.noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
            }
        }
    }



    //void UpdateSelectionUI()
    //{
    //    if (yesText != null)
    //    {
    //        yesText.fontSize = (selectedOption == 0) ? selectedFontSize : defaultFontSize;
    //    }
    //    if (noText != null)
    //    {
    //        noText.fontSize = (selectedOption == 1) ? selectedFontSize : defaultFontSize;
    //    }
    //}

    //void HandleUIInput()
    //{
    //    // Chuyển đổi lựa chọn bằng phím mũi tên trái/phải
    //    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
    //    {
    //        selectedOption = (selectedOption + 1) % 2; // Đảo ngược lựa chọn
    //        UpdateSelectionUI();
    //    }

    //    // Xác nhận lựa chọn bằng phím Enter
    //    if (Input.GetKeyDown(KeyCode.Return))
    //    {
    //        // Trong hàm HandleUIInput() của DoorInteractionManager
    //        if (selectedOption == 0)
    //        {
    //            if (GameManager.instance != null)
    //            {
    //                // Lưu vị trí và hướng của cửa vào GameManager
    //                GameManager.instance.spawnPosition = destinationSpawnPoint.position;
    //                GameManager.instance.spawnRotation = destinationSpawnPoint.rotation;

    //                // Đảm bảo biến isFirstLoad đã được đặt thành false
    //                GameManager.instance.isFirstLoad = false;
    //            }
    //            SceneManager.LoadScene(destinationSceneName);
    //            HideUI();
    //        }
    //    }
    //}

    void HandleUIInput()
    {
        // Chuyển đổi lựa chọn bằng phím mũi tên trái/phải
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedOption = (selectedOption + 1) % 2; // Đảo ngược lựa chọn
            UpdateSelectionUI();
        }

        // Xác nhận lựa chọn bằng phím Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (selectedOption == 0) // Kiểm tra nếu chọn Yes
            {
                if (GameManager.instance != null)
                {
                    //// Lấy SpawnPoint từ SpawnPointManager bằng tên
                    //Transform destination = GameManager.instance.spawnPointManager.GetSpawnPoint(destinationSpawnPointName);
                    //if (destination != null)
                    //{
                    //    // Lưu vị trí và hướng của điểm đến vào GameManager
                    //    GameManager.instance.spawnPosition = destination.position;
                    //    GameManager.instance.spawnRotation = destination.rotation;

                    //    // Đảm bảo biến isFirstLoad đã được đặt thành false
                    //    GameManager.instance.isFirstLoad = false;
                    //    SceneManager.LoadScene(destinationSceneName);
                    //}
                    //else
                    //{
                    //    Debug.LogError("Điểm đến không được tìm thấy. Vui lòng kiểm tra tên đã nhập trong Prefab.");
                    //}

                    // LƯU tên điểm đến vào GameManager và TẢI scene.
                    GameManager.instance.desiredSpawnPointName = destinationSpawnPointName;
                    GameManager.instance.isFirstLoad = false;
                    SceneManager.LoadScene(destinationSceneName);
                }
                HideUI();
            }
            else // Nếu chọn No
            {
                EndInteraction();
            }
        }
    }

    void EndInteraction()
    {
        // Tắt UI và tiếp tục chơi
        //HideUI();

        isInteracting = false;

        // Tắt text UI
        if (GameManager.instance != null && GameManager.instance.questionPanel != null)
        {
            GameManager.instance.questionPanel.SetActive(false);
        }

        // Mở khóa chuyển động nhân vật
        if (playerController != null)
        {
            playerController.canMove = true;
        }
    }
}