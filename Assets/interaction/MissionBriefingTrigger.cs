using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionBriefingTrigger : MonoBehaviour
{
    [Header("Briefing UI Settings")]
    public TextMeshProUGUI briefingTextUI;
    [TextArea(3, 10)]
    public string[] briefingTexts;
    [Range(0.01f, 0.1f)]
    public float typingSpeed = 0.05f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip typingSound;

    [Header("Camera Settings")]
    public GameObject[] briefingCameras;
    [Range(1, 10)]
    public int textsPerCameraChange = 2;

    // State tracking
    private int currentTextIndex = 0;
    private int currentCameraIndex = 0;
    private bool isBriefingActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    // Static variable to ensure only plays once per scene
    private static bool hasBeenActivatedInThisScene = false;

    private Camera mainGameCamera;
    private Character_movement playerController;
    private Animator playerAnimator;

    void Start()
    {
        if (briefingTextUI != null) briefingTextUI.gameObject.SetActive(false);

        // Cấu hình Audio Source thành 2D tự động
        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra Player và đảm bảo chỉ chạy 1 lần duy nhất trong Scene
        if (other.CompareTag("Player") && !hasBeenActivatedInThisScene)
        {
            hasBeenActivatedInThisScene = true;
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();

            if (Camera.main != null) mainGameCamera = Camera.main;

            StartBriefing();
        }
    }

    void Update()
    {
        if (isBriefingActive && !isTyping)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ShowNextText();
            }
        }
    }

    private void StartBriefing()
    {
        isBriefingActive = true;
        GameState.isInputLocked = true;

        // Vô hiệu hóa CameraManager
        if (CameraManager.instance != null)
        {
            CameraManager.instance.isCutscenePlaying = true;
            CameraManager.instance.enabled = false;
        }

        // --- DỪNG NHÂN VẬT TRIỆT ĐỂ ---
        if (playerController != null) playerController.enabled = false;
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMoving", false);
            playerAnimator.SetFloat("Speed", 0f); // Nếu bạn dùng Blend Tree
            // Ép Animator về trạng thái Idle ngay lập tức để không bị kẹt animation chạy
            playerAnimator.Play("Armature|Idle_main", 0, 0f);
        }

        if (briefingTextUI != null) briefingTextUI.gameObject.SetActive(true);

        currentTextIndex = 0;
        currentCameraIndex = 0;
        ShowNextText();
    }

    private void ShowNextText()
    {
        if (currentTextIndex >= briefingTexts.Length)
        {
            EndBriefing();
            return;
        }

        // Chuyển camera nếu đến lượt
        if (briefingCameras.Length > 0 && currentTextIndex % textsPerCameraChange == 0)
        {
            SwitchCamera();
        }

        // Bắt đầu hiệu ứng đánh máy
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(briefingTexts[currentTextIndex]));

        currentTextIndex++;
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        briefingTextUI.text = "";

        foreach (char letter in line.ToCharArray())
        {
            briefingTextUI.text += letter;

            // Phát âm thanh typing (2D)
            if (audioSource != null && typingSound != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f); // Một chút ngẫu nhiên cho tự nhiên
                audioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void SwitchCamera()
    {
        if (mainGameCamera != null) mainGameCamera.enabled = false;

        foreach (GameObject camObj in briefingCameras)
        {
            if (camObj != null) camObj.SetActive(false);
        }

        if (briefingCameras.Length > currentCameraIndex && briefingCameras[currentCameraIndex] != null)
        {
            GameObject targetObj = briefingCameras[currentCameraIndex];
            targetObj.SetActive(true);
            Camera targetCam = targetObj.GetComponent<Camera>();
            if (targetCam != null)
            {
                targetCam.enabled = true;
                targetCam.depth = 100;
            }
            currentCameraIndex++;
        }
    }

    private void EndBriefing()
    {
        isBriefingActive = false;

        foreach (GameObject camObj in briefingCameras)
        {
            if (camObj != null) camObj.SetActive(false);
        }

        if (CameraManager.instance != null)
        {
            CameraManager.instance.enabled = true;
            CameraManager.instance.isCutscenePlaying = false;
            if (mainGameCamera != null) mainGameCamera.enabled = true;
            CameraManager.instance.InitializeCamerasForNewScene();
        }

        // Trả lại quyền di chuyển cho Player
        if (playerController != null) playerController.enabled = true;
        GameState.isInputLocked = false;

        if (briefingTextUI != null) briefingTextUI.gameObject.SetActive(false);

        // Vô hiệu hóa script này để không bao giờ chạy lại trong scene này
        this.enabled = false;
    }
}