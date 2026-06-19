using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraSwitcher : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera freelookCam; // Reference to your default exploration/free-look virtual camera.
    [SerializeField] private CinemachineCamera aimCam; // Reference to your tight over-the-shoulder aiming virtual camera.
    [SerializeField] private CinemachineInputAxisController inputAxisController; // Component that feeds user input into the Freelook camera.

    [Header("References")]
    [SerializeField] private Camera mainCamera; // Reference to the actual physical Unity Camera rendering the viewport.
    [SerializeField] private PlayerController player; // Reference to your player movement script to pass state data.
    [SerializeField] private GameObject crosshairUI; // Reference to the UI crosshair object to toggle on/off (currently unused in this code).
    [SerializeField] private PlayerActions input; // Reference to your auto-generated C# class handling player controls.
    //Changed this from a basic GameObject reference to your specific crosshair script component
    [SerializeField] private WorldCrossHairController crosshairController;

    private InputAction aimAction; // Stores the specific action map reference responsible for aiming.
    private bool isAiming = false; // Flag to track whether the player is currently inside the aim state.
    private Transform yawTarget; // Stores a reference to a horizontal rotation target (currently unused here).
    private Transform pitchTarget; // Stores a reference to a vertical rotation target (currently unused here).

    private AimCameraController aimCameraController; // Reference to the aim script we fixed earlier to sync tracking angles.

    // Runs on the first frame before any Update loop executes.
    void Start()
    {
        // Finds and stores the custom AimCameraController script attached directly to your aim camera object.
        aimCameraController = aimCam.GetComponent<AimCameraController>();

        // Finds and stores the input axis handler attached directly to your exploration camera object.
        inputAxisController = freelookCam.GetComponent<CinemachineInputAxisController>();

        // Instantiates a brand new instance of your custom control setup config.
        input = new PlayerActions();

        // Turns on the entire input map asset layout so it can register key presses.
        input.Enable();

        // Pinpoints and links to the specific "Aim" button action grouped inside your "Gameplay" action list.
        aimAction = input.Gameplay.Aim;
    }

    // Runs automatically every single frame to check button inputs and switch states.
    void Update()
    {
        // Checks the input system to see if the player is holding down the aim button right now (true or false).
        bool aimPressed = aimAction.IsPressed();

        // Sends this true/false aiming state directly into your PlayerController script to update player animations/mechanics.
        player.isAiming = aimPressed;

        // EVALUATION: If they are holding the button, but the code system hasn't activated aiming yet, trigger the switch.
        if (aimPressed && !isAiming)
        {
            EnterAimMode(); // Call the function to setup and focus the over-the-shoulder view.
        }
        // EVALUATION: If they released the button, but the code system is still marked as aiming, revert the state.
        else if (!aimPressed && isAiming)
        {
            ExitAimMode(); // Call the function to clean up and return to standard exploration mode.
        }
    }

    // Handles everything required to cleanly enter the aim state.
    private void EnterAimMode()
    {
        // Raises our state flag to show the player is actively aiming down sights.
        isAiming = true;

        // Aligns our target variables to face your current forward-facing view before switching views.
        SnapAimCameraToPlayerForward();

        //Turn the crosshair ON immediately when transitioning to the aim camera
        if (crosshairController != null) crosshairController.SetCrosshairVisibility(true);

        // Swaps priorities: sets the aim camera high and freelook camera low so Cinemachine smoothly cuts or blends to the shoulder.
        aimCam.Priority = 20;
        freelookCam.Priority = 10;

        // Disables the exploration camera's separate look inputs so your two camera configurations don't fight each other.
        inputAxisController.enabled = false;
    }

    // Handles everything required to cleanly leave the aim state.
    private void ExitAimMode()
    {
        // Lowers the tracking flag to show the player has stopped aiming.
        isAiming = false;

        // Calls our synchronization calculation so the freelook camera swings directly behind where you were just aiming.
        SnapFreeLookToBehindPlayer();

        //Turn the crosshair OFF immediately when dropping out of aim mode
        if (crosshairController != null) crosshairController.SetCrosshairVisibility(false);

        // Sets the aim camera priority low and the freelook priority high, causing Cinemachine to blend back to exploration.
        aimCam.Priority = 10;
        freelookCam.Priority = 20;

        // Turns mouse/joystick tracking back on for the freelook camera so you can spin it freely again.
        inputAxisController.enabled = true;
    }

    // Forces the Freelook camera's angle to match the direction the player was pointing while aiming.
    private void SnapFreeLookToBehindPlayer()
    {
        // Fetches the orbital track component belonging to your exploration camera setup.
        CinemachineOrbitalFollow orbitalFollow = freelookCam.GetComponent<CinemachineOrbitalFollow>();

        // Obtains the forward directional line of the aim camera that the player was just looking down.
        Vector3 forward = aimCam.transform.forward;

        // Math calculation: Uses trigonometry to convert the 3D X/Z forward vector into a 360-degree flat horizontal angle.
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

        // Snaps the horizontal tracking orbit parameter directly to this angle so the camera instantly sits behind the player.
        orbitalFollow.HorizontalAxis.Value = angle;
    }

    // Tells your custom aim script to look straight down the path of your exploration camera.
    private void SnapAimCameraToPlayerForward()
    {
        // Passes the freelook camera's positioning data into the SetYawPitch method we fixed earlier to cleanly reset aim targets.
        aimCameraController.SetYawPitchFromCameraForward(freelookCam.transform);
    }

}
