using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System
using UnityEngine.UI;         // Required for UI Image

public class OpenMenuPressX : MonoBehaviour
{
    [Header("Reference to the action listening for the X button on the left controller")]
    [SerializeField] private InputActionReference openMenuAction;

    [Header("Action for emergency exit (hold)")]
    [SerializeField] private InputActionReference emergencyExitAction;

    [Header("UI Image used to display the hold fill")]
    [SerializeField] private Image emergencyFillImage;

    [Header("Canvas containing the hold fill UI")]
    [SerializeField] private GameObject canvasCloseApp;

    [Header("Duration (in seconds) required to hold for emergency exit")]
    [SerializeField] private float holdDuration = 7f;

    [Header("Menu to show/hide")]
    [SerializeField] private GameObject menuToToggle;

    private bool isHolding = false;
    private float holdTimer = 0f;

    private void OnEnable()
    {
        // Enable and subscribe to the menu toggle action
        openMenuAction.action.Enable();
        openMenuAction.action.performed += OnOpenMenu;

        // Enable and subscribe to the emergency exit hold actions
        emergencyExitAction.action.Enable();
        emergencyExitAction.action.started += OnEmergencyExitStarted;
        emergencyExitAction.action.canceled += OnEmergencyExitCanceled;

        // Ensure the hold UI is hidden initially
        if (canvasCloseApp != null)
            canvasCloseApp.SetActive(false);

        // Initialize fill image
        if (emergencyFillImage != null)
            emergencyFillImage.fillAmount = 0f;
    }

    private void OnDisable()
    {
        // Unsubscribe and disable menu action
        openMenuAction.action.performed -= OnOpenMenu;
        openMenuAction.action.Disable();

        // Unsubscribe and disable emergency exit actions
        emergencyExitAction.action.started -= OnEmergencyExitStarted;
        emergencyExitAction.action.canceled -= OnEmergencyExitCanceled;
        emergencyExitAction.action.Disable();
    }

    private void Update()
    {
        if (isHolding)
        {
            // Accumulate hold time
            holdTimer += Time.deltaTime;

            // Update the UI fill (0 to 1)
            if (emergencyFillImage != null)
                emergencyFillImage.fillAmount = Mathf.Clamp01(holdTimer / holdDuration);

            // If held long enough, trigger exit
            if (holdTimer >= holdDuration)
            {
                isHolding = false;
                Debug.Log("EmergencyExit hold complete. Closing application...");

                // Hide the hold UI
                if (canvasCloseApp != null)
                    canvasCloseApp.SetActive(false);

                // Quit application in build
                Application.Quit();

                // Stop play mode in the Unity Editor
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
        }
    }

    private void OnOpenMenu(InputAction.CallbackContext context)
    {
        // Toggle the menu's active state
        bool isActive = menuToToggle.activeSelf;
        menuToToggle.SetActive(!isActive);
        Debug.Log("X button pressed. Menu is now: " + (!isActive ? "Active" : "Inactive"));
    }

    private void OnEmergencyExitStarted(InputAction.CallbackContext context)
    {
        // Begin holding
        isHolding = true;
        holdTimer = 0f;

        // Show the hold UI and reset fill
        if (canvasCloseApp != null)
            canvasCloseApp.SetActive(true);

        if (emergencyFillImage != null)
            emergencyFillImage.fillAmount = 0f;

        Debug.Log("EmergencyExit hold started.");
    }

    private void OnEmergencyExitCanceled(InputAction.CallbackContext context)
    {
        // Cancel hold and reset
        isHolding = false;
        holdTimer = 0f;

        // Hide the hold UI and reset fill
        if (canvasCloseApp != null)
            canvasCloseApp.SetActive(false);

        if (emergencyFillImage != null)
            emergencyFillImage.fillAmount = 0f;

        Debug.Log("EmergencyExit hold canceled.");
    }
}
