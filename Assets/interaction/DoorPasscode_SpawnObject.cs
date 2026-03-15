using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoorPasscodeVendingMachine : MonoBehaviour
{
    public TextMeshProUGUI interactionText;
    public Transform hingePoint;
    public float openAngle = 90f;
    public float rotationSpeed = 2.0f;
    public PasscodePanelVendingMachine passcodePanel;

    [Header("Spawn Object Settings")]
    public bool isSpawnInteraction = false;
    public string spawnActionId;
    public GameObject objectToSpawnPrefab;
    public Transform spawnPoint;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip openDoorClip; // Tiếng cửa mở

    private bool playerInRange = false;
    private bool isOpened = false;
    private bool isRotating = false;
    private BoxCollider doorCollider;

    void Awake()
    {
        doorCollider = GetComponent<BoxCollider>();
        // Tự động tìm AudioSource nếu chưa gán
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (isSpawnInteraction)
        {
            if (string.IsNullOrEmpty(spawnActionId)) Debug.LogWarning($"Door {gameObject.name}: spawnActionId is missing.");
            if (objectToSpawnPrefab == null || spawnPoint == null) Debug.LogWarning($"Door {gameObject.name}: Missing spawn references.");
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isOpened)
        {
            if (passcodePanel != null)
            {
                passcodePanel.ShowKeypad();
                if (interactionText != null) interactionText.text = "Press Enter to confirm order\nPress E to close";
            }
        }

        if (isRotating)
        {
            Quaternion targetRotation = Quaternion.Euler(0, openAngle, 0);
            hingePoint.localRotation = Quaternion.Slerp(hingePoint.localRotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Quaternion.Angle(hingePoint.localRotation, targetRotation) < 0.1f)
            {
                isRotating = false;
                hingePoint.localRotation = targetRotation;
            }
        }
    }

    public void UnlockDoor()
    {
        if (isOpened) return;
        isOpened = true;
        isRotating = true;

        // PHÁT TIẾNG MỞ CỬA
        if (audioSource != null && openDoorClip != null)
        {
            audioSource.PlayOneShot(openDoorClip);
        }

        if (doorCollider != null) doorCollider.enabled = false;

        // Logic Spawn (Giữ nguyên)
        if (isSpawnInteraction && objectToSpawnPrefab != null && spawnPoint != null && GameManager.instance != null)
        {
            if (!string.IsNullOrEmpty(spawnActionId) && !GameManager.instance.completedSpawnActions.Contains(spawnActionId))
            {
                GameObject spawnedObject = Instantiate(objectToSpawnPrefab, spawnPoint.position, spawnPoint.rotation);
                DontDestroyOnLoad(spawnedObject);
                GameManager.instance.completedSpawnActions.Add(spawnActionId);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionText != null && !isOpened) interactionText.text = "Press E to interact";
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            EndInteraction();
        }
    }

    public void EndInteraction()
    {
        if (interactionText != null) interactionText.text = "";
    }
}