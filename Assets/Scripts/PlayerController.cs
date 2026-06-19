using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ==================== MOVEMENT SETTINGS ====================
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;        // Normal walking speed
    [SerializeField] private float sprintSpeed = 10f;    // Sprinting speed
    [SerializeField] private float rotationSpeed = 10f; // How fast player rotates to face movement

    // ==================== JUMP SETTINGS ====================
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;      // Max jump height
    [SerializeField] private float gravity = -9.8f;     // Gravity force (negative = down)

    // ==================== REFERENCES ====================
    [Header("References")]
    [SerializeField] private Transform cameraTransform; // Camera reference for directional movement

    // ==================== PRIVATE VARIABLES ====================
    private CharacterController controller;   // Handles movement and collision
    private Vector2 moveInput;               // WASD input storage
    private Vector3 velocity;               // Current velocity (used for gravity/jump)
    private bool isSprinting;              // Sprint state tracker

    // ==================== INITIALIZATION ====================
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // ==================== MAIN UPDATE LOOP ====================
    void Update()
    {
        // Get current speed based on sprint state
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Get camera directions for movement
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Flatten directions (ignore camera vertical tilt)
        forward.y = 0f;
        right.y = 0f;

        // Normalize for consistent speed in all directions
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction based on input and camera
        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;

        // Apply horizontal movement
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Rotate player to face movement direction (if moving)
        if (moveInput != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply vertical movement (jump/fall)
        controller.Move(velocity * Time.deltaTime);
    }

    // ==================== INPUT CALLBACKS ====================
    // Called when WASD/joystick input changes
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Called when sprint button is pressed/released
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    // Called when jump button is pressed
    public void OnJump(InputAction.CallbackContext context)
    {
        // Only jump if on ground
        if (context.performed && controller.isGrounded)
        {
            // Calculate jump velocity using physics formula
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}