using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TimedDialogueSequence : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;
        public float typingSpeed = 0.05f;      // Tốc độ gõ riêng cho dòng này
        public float delayBeforeStart = 0.5f; // Chờ bao lâu trước khi bắt đầu hiện dòng này
        public float displayDuration = 2.0f;  // Hiện trong bao lâu sau khi gõ xong
    }

    [Header("Danh sách câu thoại")]
    public List<DialogueLine> dialogueSequence;

    [Header("Tham chiếu UI")]
    public TextMeshProUGUI uiTextElement;
    public GameObject dialoguePanel;

    [Header("Âm thanh (Tùy chọn)")]
    public AudioSource typingAudioSource;
    public AudioClip typingClip;

    private bool isPlaying = false;

    private void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (uiTextElement != null) uiTextElement.text = "";

        // Bắt đầu trình tự tự động
        StartCoroutine(PlayFullSequence());
    }

    private IEnumerator PlayFullSequence()
    {
        if (isPlaying || dialogueSequence == null || dialogueSequence.Count == 0) yield break;

        isPlaying = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        foreach (DialogueLine line in dialogueSequence)
        {
            // 1. Chờ trước khi hiện dòng chữ này
            uiTextElement.text = "";
            yield return new WaitForSeconds(line.delayBeforeStart);

            // 2. Hiệu ứng đánh máy (Typewriter)
            foreach (char letter in line.text.ToCharArray())
            {
                uiTextElement.text += letter;

                if (typingAudioSource != null && typingClip != null)
                {
                    typingAudioSource.PlayOneShot(typingClip);
                }

                yield return new WaitForSeconds(line.typingSpeed);
            }

            // 3. Chờ sau khi gõ xong để người chơi đọc
            yield return new WaitForSeconds(line.displayDuration);
        }

        // Kết thúc toàn bộ trình tự
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        uiTextElement.text = "";
        isPlaying = false;

        Debug.Log("Hoàn thành trình tự hiển thị văn bản.");
    }
}