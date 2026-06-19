using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ==================== VALUES & SETTINGS ====================
    #region Values
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;      // How fast the player walks (units per second)
    [SerializeField] private float sprintSpeed = 10f;   // How fast the player sprints (units per second)
    [SerializeField] private float jumpHeight = 2f;     // How high the player can jump (in units)
    [SerializeField] private float gravity = -9.8f;     // Gravity force pulling player down (negative = downward)

    // -------- PRIVATE VARIABLES --------
    private CharacterController controller; // Unity component that handles player movement and collision
    private Vector2 moveInput;              // Stores WASD input as 2D vector (x = left/right, y = forward/back)
    private Vector3 velocity;               // Stores the player's current velocity (speed and direction)
    private bool isSprinting = false;       // Tracks whether the player is currently holding sprint button
    #endregion


    // ==================== INITIALIZATION ====================
    #region Start
    void Start()
    {
        // Get the CharacterController component attached to this GameObject
        controller = GetComponent<CharacterController>();
    }
    #endregion


    // ==================== MAIN UPDATE LOOP ====================
    #region Update
    void Update()
    {
        // -------- DETERMINE CURRENT SPEED --------
        // Choose the correct speed based on sprint state
        // Ternary operator: condition ? valueIfTrue : valueIfFalse
        // Only ONE speed is used at a time (never added together)
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        controller.Move(move * currentSpeed * Time.deltaTime);

        // -------- APPLY GRAVITY --------
        // Gradually increase downward velocity over time (simulates falling)
        // velocity.y gets more negative each frame = falls faster
        velocity.y += gravity * Time.deltaTime;

        // -------- APPLY VERTICAL MOVEMENT --------
        // Move the player based on current velocity (mainly affects jumping/falling)
        controller.Move(velocity * Time.deltaTime);
    }
    #endregion


    // ==================== INPUT SYSTEM CALLBACKS ====================
    // These functions are called automatically by Unity's New Input System
    // They're triggered when the player presses buttons defined in the Input Actions asset

    #region OnMove
    // Called automatically when player uses WASD or joystick
    public void OnMove(InputAction.CallbackContext context)
    {
        // Read the 2D input value from the input device
        // Returns Vector2 where:
        // x = horizontal input (-1 = left, 0 = neutral, 1 = right)
        // y = vertical input (-1 = back, 0 = neutral, 1 = forward)
        moveInput = context.ReadValue<Vector2>();
    }
    #endregion

    #region OnSprint
    // Called automatically when player presses/releases sprint button (usually Left Shift)
    public void OnSprint(InputAction.CallbackContext context)
    {
        // -------- START SPRINTING --------
        // context.started = triggered the moment button is first pressed down
        if (context.started)
        {
            isSprinting = true; // Enable sprint mode
        }

        // -------- STOP SPRINTING --------
        // context.canceled = triggered the moment button is released
        else if (context.canceled)
        {
            isSprinting = false; // Disable sprint mode
        }
    }
    #endregion

    #region OnJump
    // Called automatically when player presses jump button (usually Spacebar)
    public void OnJump(InputAction.CallbackContext context)
    {
        // Only allow jumping if:
        // 1. context.performed = button was fully pressed (not just touched)
        // 2. controller.isGrounded = player is touching the ground (no double jump)
        if (context.performed && controller.isGrounded)
        {
            // -------- CALCULATE JUMP VELOCITY --------
            // Physics formula: v = √(height × -2 × gravity)
            // This calculates the exact upward velocity needed to reach the desired jumpHeight
            // Mathf.Sqrt = square root function
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    #endregion
}