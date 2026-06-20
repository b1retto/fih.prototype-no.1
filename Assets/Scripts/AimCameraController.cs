using System;
using Mono.Cecil;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimCameraController : MonoBehaviour
{
    [Header("Camera Targets")]
    [SerializeField] private Transform yawTarget; // The target object responsible for turning left and right.
    [SerializeField] private Transform pitchTarget; // The target object responsible for looking up and down.

    [Header("Input Action References")]
    [SerializeField] private InputActionReference lookInput; // Pointer to the New Input System vector data for mouse/joystick look.
    [SerializeField] private InputActionReference switchShoulderInput; // Pointer to the button action used to swap shoulders.

    [Header("Sensitivity Settings")]
    [SerializeField] private float mouseSensitivity = 0.05f; // Multiplier to downscale raw mouse delta inputs.
    [SerializeField] private float sensitivity = 1.5f; // General sensitivity scaling factor for all look movement.

    [Header("Rotation Constraints")]
    [SerializeField] private float pitchMin = -40f; // Maximum angle allowed when looking down.
    [SerializeField] private float pitchMax = 80f; // Maximum angle allowed when looking up (prevents flipping).

    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineThirdPersonFollow aimCam; // Reference to the Cinemachine extension handling positioning.
    [SerializeField] private float shoulderSwitchSpeed = 5f; // The interpolation speed (Lerp) used to slide between shoulders.

    private float yaw; // Stores the accumulated horizontal look rotation.
    private float pitch; // Stores the accumulated vertical look rotation.
    private float targetCameraSide; // Stores the target shoulder value (0 for left side, 1 for right side).

    // Runs immediately when the script instance is initialized, before Start().
    void Awake()
    {
        // Finds and caches the CinemachineThirdPersonFollow component attached to this GameObject.
        aimCam = GetComponent<CinemachineThirdPersonFollow>();

        // Sets our target shoulder to match whatever value the camera currently has configured in the Inspector.
        targetCameraSide = aimCam.CameraSide;
    }

    // Runs on the first frame before any Update loop executes.
    void Start()
    {
        // Extracts the current rotation angles from your horizontal tracking target.
        Vector3 angles = yawTarget.rotation.eulerAngles;

        // Synchronizes internal code track variables with the initial rotations present in your Unity scene.
        yaw = angles.y;
        pitch = angles.x;

        // Normalizes Unity's 0-360 angle layout into a -180 to 180 coordinate setup to cleanly match pitch limits.
        if (pitch > 180) pitch -= 360;

        // Activates the Look input map asset so it actively starts reading user input changes.
        lookInput.asset.Enable();
    }

    // Runs whenever this GameObject is activated/enabled in the hierarchy.
    void OnEnable()
    {
        // Activates the shoulder-swapping control scheme.
        switchShoulderInput.action.Enable();

        // Hooks up (subscribes) our "OnSwitchShoulder" function to fire whenever the shoulder button is hit.
        switchShoulderInput.action.performed += OnSwitchShoulder;
    }

    // Runs whenever this GameObject is deactivated/disabled in the hierarchy.
    void OnDisable()
    {
        // Turns off the shoulder-swapping control scheme to preserve resources.
        switchShoulderInput.action.Disable();

        // Unhooks (unsubscribes) our function from the event to prevent memory leaks in your project.
        switchShoulderInput.action.performed -= OnSwitchShoulder;
    }

    // Runs automatically every single frame. Deals with mouse tracking and object movements.
    void Update()
    {
        // Reads the look vector (X: horizontal change, Y: vertical change) coming from your input device.
        Vector2 look = lookInput.action.ReadValue<Vector2>();

        // Determines if a mouse is connected and if it has physically moved across your desk.
        if (Mouse.current != null && Mouse.current.delta.IsActuated())
        {
            // Reduces raw mouse hardware speed so the look behavior doesn't wildly snap out of control.
            look *= mouseSensitivity;
        }

        // Adds horizontal movement input directly to your persistent yaw tracker variable.
        yaw += look.x * sensitivity;

        // Subtracts vertical movement input from your pitch tracker variable (subtraction prevents inverted controls).
        pitch -= look.y * sensitivity;

        // CRITICAL FIX: Forces the vertical track angle to stay within your safe boundaries, stopping screen flips.
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Creates a clean, upright 3D rotation using the updated yaw value and applies it to the horizontal target object.
        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Creates a local 3D tilt using the clamped pitch value and applies it directly to the vertical target object.
        pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Smoothly interpolates the current Cinemachine CameraSide value closer to your target shoulder position over time.
        aimCam.CameraSide = Mathf.Lerp(aimCam.CameraSide, targetCameraSide, Time.deltaTime * shoulderSwitchSpeed);
    }

    // Triggered automatically via Event whenever the player taps your registered Shoulder Switch action button.
    private void OnSwitchShoulder(InputAction.CallbackContext context)
    {
        // Evaluation check: If camera is currently on the left side (<0.5), snap goal to 1 (Right). Otherwise, snap to 0 (Left).
        targetCameraSide = aimCam.CameraSide < 0.5f ? 1f : 0f;
    }

    // Public utility method allowing external scripts to automatically align your aim logic to face a specific camera's forward path.
    public void SetYawPitchFromCameraForward(Transform cameraTransform)
    {
        // Obtains the forward facing directional vector of your chosen reference camera.
        Vector3 flatForward = cameraTransform.forward;

        // Flattens the vector out by removing vertical elevation, leaving an absolute flat horizon path.
        flatForward.y = 0;

        // Check calculation to see if the vector is functionally dead (0 length); skips rotation tracking if it is empty.
        if (flatForward == Vector3.zero)
        {
            return;
        }

        // Calculates a clean look angle looking down your flattened vector path and sets the yaw value to match.
        yaw = Quaternion.LookRotation(flatForward).eulerAngles.y;

        // Directly forces the horizontal target object to snap to this newly calculated camera rotation angle.
        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Directly flattens the vertical target object's tilt layout back out to zero level.
        pitchTarget.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Resets the internal pitch tracking number to 0 to keep variables perfectly in sync with the line above.
        pitch = 0f;
    }
}
