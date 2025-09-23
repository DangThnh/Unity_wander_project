using UnityEngine;
using TMPro;

public class SceneUIRegistrar : MonoBehaviour
{
    // Kéo và thả các thành phần UI từ Inspector vào đây
    public TextMeshProUGUI interactionText;
    public GameObject questionPanel;
    public TextMeshProUGUI yesText;
    public TextMeshProUGUI noText;

    // Tham chiếu đến SpawnPointManager của scene này
    public SpawnPointManager sceneSpawnPointManager;

    void Start()
    {
        // Kiểm tra xem GameManager có tồn tại không
        if (GameManager.instance != null)
        {
            // Cung cấp các tham chiếu UI cho GameManager
            GameManager.instance.SetInteractionText(interactionText);
            GameManager.instance.SetQuestionUI(questionPanel, yesText, noText);

            // Cung cấp tham chiếu SpawnPointManager cho GameManager
            GameManager.instance.SetSpawnPointManager(sceneSpawnPointManager);

            // Ẩn panel ngay khi scene mới tải
            if (questionPanel != null)
            {
                questionPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Không tìm thấy GameManager instance. Vui lòng đảm bảo có một GameManager trong scene và có DontDestroyOnLoad.");
        }
    }
}