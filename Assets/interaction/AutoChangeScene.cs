using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AutoChangeScene : MonoBehaviour
{
    private const string FADER_GROUP_NAME = "Panel";

    [Header("Teleport Settings")]
    public string destinationSceneName;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.0f;
    public float blackScreenDuration = 0.5f;

    // Trạng thái hệ thống
    private bool isSceneTransitionActive = false;
    private CanvasGroup faderCanvasGroup;
    private Character_movement playerController;
    private Animator playerAnimator;

    void Start()
    {
        // Tìm Fader CanvasGroup để làm hiệu ứng tối màn hình
        GameObject faderObj = GameObject.Find(FADER_GROUP_NAME);
        if (faderObj != null)
        {
            faderCanvasGroup = faderObj.GetComponent<CanvasGroup>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu là Player và chưa bắt đầu chuyển cảnh
        if (other.CompareTag("Player") && !isSceneTransitionActive)
        {
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();

            // Khóa di chuyển và bắt đầu hiệu ứng chuyển cảnh ngay lập tức
            //LockPlayerActions();
            StartCoroutine(FadeAndLoadScene());
        }
    }

    //private void LockPlayerActions()
    //{
    //    if (playerController != null)
    //        playerController.canMove = false;

    //    if (playerAnimator != null)
    //        playerAnimator.SetBool("IsMoving", false);
    //}

    private IEnumerator FadeAndLoadScene()
    {
        isSceneTransitionActive = true;

        if (faderCanvasGroup != null)
        {
            // Bắt đầu làm tối màn hình
            faderCanvasGroup.blocksRaycasts = true;
            while (faderCanvasGroup.alpha < 1)
            {
                faderCanvasGroup.alpha += Time.deltaTime / fadeSpeed;
                yield return null;
            }
            faderCanvasGroup.alpha = 1;

            // Giữ màn hình đen trong một khoảng thời gian ngắn
            yield return new WaitForSeconds(blackScreenDuration);
        }

        // Chuyển sang Scene mới
        SceneManager.LoadScene(destinationSceneName);
    }
}