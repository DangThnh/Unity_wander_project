using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class DialogueTypewriter : MonoBehaviour
{
    [Header("Cấu hình văn bản")]
    [TextArea(3, 10)]
    public string dialogueText = "Chào bạn! Đây là hiệu ứng chữ chạy từng ký tự một...";
    public float typingSpeed = 0.05f; // Tốc độ chạy chữ (giây/ký tự)
    public float displayDurationAfterDone = 2.0f; // Chờ thêm bao lâu sau khi chạy xong chữ mới tắt

    [Header("Tham chiếu UI")]
    public TextMeshProUGUI uiTextElement;
    public GameObject dialoguePanel;

    [Header("Âm thanh (Tùy chọn)")]
    public AudioSource typingAudioSource; // Kéo AudioSource vào đây
    public AudioClip typingClip;          // Kéo file âm thanh ngắn vào đây

    private bool hasBeenActivated = false;
    private bool isTyping = false;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (uiTextElement != null) uiTextElement.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasBeenActivated && other.CompareTag("Player"))
        {
            StartCoroutine(PlayDialogueRoutine());
        }
    }

    private IEnumerator PlayDialogueRoutine()
    {
        hasBeenActivated = true;
        isTyping = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        uiTextElement.text = "";

        // Lặp qua từng ký tự của chuỗi
        foreach (char letter in dialogueText.ToCharArray())
        {
            uiTextElement.text += letter;

            // Phát âm thanh nhẹ mỗi khi hiện chữ
            if (typingAudioSource != null && typingClip != null)
            {
                typingAudioSource.PlayOneShot(typingClip);
            }

            // Chờ một khoảng thời gian trước khi hiện chữ tiếp theo
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Chờ thêm một khoảng thời gian sau khi chữ đã hiện hết để người chơi kịp đọc
        yield return new WaitForSeconds(displayDurationAfterDone);

        // Tắt UI
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        uiTextElement.text = "";
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        BoxCollider col = GetComponent<BoxCollider>();
        Gizmos.DrawCube(transform.position + col.center, col.size);
    }
}