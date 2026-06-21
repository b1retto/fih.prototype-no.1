using System;
using Mono.Cecil;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimCameraController : MonoBehaviour
{
    [Header("Camera Targets")]
    [SerializeField] private Transform yawTarget; // Object used to look left/right
    [SerializeField] private Transform pitchTarget; // Object used to look up/down

    [Header("Input Action References")]
    [SerializeField] private InputActionReference lookInput; // Reads mouse or joystick movement data
    [SerializeField] private InputActionReference switchShoulderInput; // Reads the button press to swap shoulders

    [Header("Sensitivity Settings")]
    [SerializeField] private float mouseSensitivity = 0.05f; // Slows down raw mouse speed so it isn't twitchy
    [SerializeField] private float sensitivity = 1.5f; // General speed multiplier for looking around

    [Header("Rotation Constraints")]
    [SerializeField] private float pitchMin = -40f; // Limit for looking down (stops camera from hitting floor)
    [SerializeField] private float pitchMax = 80f; // Limit for looking up (stops camera from flipping upside down)

    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineThirdPersonFollow aimCam; // Cinemachine component that positions the camera
    [SerializeField] private float shoulderSwitchSpeed = 5f; // How fast the camera slides from left to right side

    private float yaw; // Stores current horizontal angle (left/right)
    private float pitch; // Stores current vertical angle (up/down)
    private float targetCameraSide; // Target shoulder position: 0 is fully left, 1 is fully right

    // Runs once when the game object spawns
    void Awake()
    {
        // Grabs the Cinemachine component automatically from this object
        aimCam = GetComponent<CinemachineThirdPersonFollow>();

        // Sets initial shoulder target to match what you set in the Inspector
        targetCameraSide = aimCam.CameraSide;
    }

    // Runs on the first frame of the game
    void Start()
    {
        // Gets the current starting rotation of the horizontal object
        Vector3 angles = yawTarget.rotation.eulerAngles;

        // Saves those starting angles into our tracker variables
        yaw = angles.y;
        pitch = angles.x;

        // Converts Unity angles (0 to 360) into standard angles (-180 to 180) to fix math glitches
        if (pitch > 180) pitch -= 360;

        // Tells the look input system to start listening for mouse/joystick movement
        lookInput.asset.Enable();
    }

    // Runs whenever this script becomes active
    void OnEnable()
    {
        // Activates the shoulder switch button input
        switchShoulderInput.action.Enable();

        // Subscribes to an event: "When this button is pressed, run the OnSwitchShoulder function"
        switchShoulderInput.action.performed += OnSwitchShoulder;
    }

    // Runs whenever this script is disabled or turned off
    void OnDisable()
    {
        // Deactivates the shoulder switch button input
        switchShoulderInput.action.Disable();

        // Unsubscribes from the event to keep code clean and prevent memory leaks
        switchShoulderInput.action.performed -= OnSwitchShoulder;
    }

    // Runs every single frame of the game
    void Update()
    {
        // Reads input data (X = horizontal movement, Y = vertical movement)
        Vector2 look = lookInput.action.ReadValue<Vector2>();

        // Checks if a mouse is connected and actively moving
        if (Mouse.current != null && Mouse.current.delta.IsActuated())
        {
            // Multiplies mouse input by sensitivity so looking isn't too fast
            look *= mouseSensitivity;
        }

        // Adds horizontal movement to the total left/right rotation value
        yaw += look.x * sensitivity;

        // Subtracts vertical movement to look up/down (subtracting stops controls from being inverted)
        pitch -= look.y * sensitivity;

        // Locks the pitch value between our Min and Max limits so the player can't look too high or low
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // Converts our flat yaw angle into a 3D rotation and applies it to turn left/right
        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Converts our flat pitch angle into a 3D rotation and applies it to tilt up/down
        pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Smoothly slides the camera position value closer to the target shoulder value frame by frame
        aimCam.CameraSide = Mathf.Lerp(aimCam.CameraSide, targetCameraSide, Time.deltaTime * shoulderSwitchSpeed);
    }

    // Automatically runs when the shoulder switch button is pressed
    private void OnSwitchShoulder(InputAction.CallbackContext context)
    {
        // Inline IF statement: If currently on the left side (< 0.5), switch to 1 (Right). Otherwise, switch to 0 (Left).
        targetCameraSide = aimCam.CameraSide < 0.5f ? 1f : 0f;
    }

    // Public method that external scripts can call to instantly align this script with a camera's view
    public void SetYawPitchFromCameraForward(Transform cameraTransform)
    {
        // Gets the direction the reference camera is facing
        Vector3 flatForward = cameraTransform.forward;

        // Clears the vertical value (Y = 0) so we only calculate flat ground direction
        flatForward.y = 0;

        // Safety check: If the vector has no length (player isn't moving/looking anywhere), stop here
        if (flatForward == Vector3.zero)
        {
            return;
        }

        // Converts the 3D direction vector into a 0-360 angle and extracts the horizontal (Y) component
        yaw = Quaternion.LookRotation(flatForward).eulerAngles.y;

        // Instantly turns our horizontal object to match that camera angle
        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Resets the vertical object to look completely straight ahead
        pitchTarget.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Resets our internal vertical tracking variable back to zero to match the line above
        pitch = 0f;
    }
}
