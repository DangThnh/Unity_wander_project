using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionBriefingTrigger : MonoBehaviour
{
    // Public variables to be set in the Inspector
    [Header("Briefing UI Settings")]
    [Tooltip("UI Text element to display the mission briefing.")]
    public TextMeshProUGUI briefingTextUI;
    [Tooltip("The text lines for the mission briefing. Press E to advance.")]
    [TextArea(3, 10)]
    public string[] briefingTexts;

    [Header("Camera Settings")]
    [Tooltip("The list of cameras to switch between during the briefing.")]
    public GameObject[] briefingCameras;
    [Tooltip("Number of text lines to read before switching to the next camera.")]
    [Range(1, 10)]
    public int textsPerCameraChange = 2;

    // Private variables for tracking state
    private int currentTextIndex = 0;
    private int currentCameraIndex = 0;
    private bool isBriefingActive = false;
    private bool hasBeenActivated = false;
    private Camera mainGameCamera;
    private Character_movement playerController;
    private Animator playerAnimator;

    void Start()
    {
        // Hide the briefing UI at the start
        if (briefingTextUI != null)
        {
            briefingTextUI.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player has entered the trigger and the briefing hasn't run yet
        if (other.CompareTag("Player") && !hasBeenActivated)
        {
            hasBeenActivated = true;
            // Get references to the player's components
            playerController = other.GetComponent<Character_movement>();
            playerAnimator = other.GetComponent<Animator>();

            // Find and store the main game camera
            if (Camera.main != null)
            {
                mainGameCamera = Camera.main;
            }

            StartBriefing();
        }
    }

    void Update()
    {
        // Only listen for input if the briefing is active
        if (isBriefingActive)
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

        // Lock player movement and set to idle animation
        if (playerController != null)
        {
            playerController.canMove = false;
        }
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMoving", false);
        }

        // Show briefing UI
        if (briefingTextUI != null)
        {
            briefingTextUI.gameObject.SetActive(true);
        }

        // Start with the first text and camera
        currentTextIndex = 0;
        currentCameraIndex = 0;
        ShowNextText();
    }

    private void ShowNextText()
    {
        // Check if all text lines have been displayed
        if (currentTextIndex >= briefingTexts.Length)
        {
            EndBriefing();
            return;
        }

        // Display the current text line
        if (briefingTextUI != null)
        {
            briefingTextUI.text = briefingTexts[currentTextIndex];
        }

        // Check if it's time to switch the camera
        if (briefingCameras.Length > 0 && currentTextIndex % textsPerCameraChange == 0)
        {
            SwitchCamera();
        }

        // Move to the next text line for the next key press
        currentTextIndex++;
    }

    private void SwitchCamera()
    {
        // Disable the current camera if one is active
        if (currentCameraIndex > 0)
        {
            briefingCameras[currentCameraIndex - 1].SetActive(false);
        }
        else if (mainGameCamera != null)
        {
            mainGameCamera.gameObject.SetActive(false);
        }

        // Activate the next camera in the list
        if (briefingCameras.Length > currentCameraIndex)
        {
            briefingCameras[currentCameraIndex].SetActive(true);
        }

        // Increment camera index, looping if necessary
        currentCameraIndex++;
    }

    private void EndBriefing()
    {
        isBriefingActive = false;

        // Restore player movement
        if (playerController != null)
        {
            playerController.canMove = true;
        }

        // Restore original camera
        if (briefingCameras.Length > 0 && currentCameraIndex > 0)
        {
            briefingCameras[currentCameraIndex - 1].SetActive(false);
        }
        if (mainGameCamera != null)
        {
            mainGameCamera.gameObject.SetActive(true);
        }

        // Hide briefing UI
        if (briefingTextUI != null)
        {
            briefingTextUI.gameObject.SetActive(false);
        }

        // Permanently disable this script so it can't be used again
        this.enabled = false;
    }
}