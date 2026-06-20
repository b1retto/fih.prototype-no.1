using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f; // Flat base speed for normal exploration walking.
    [SerializeField] private float sprintSpeed = 10f; // Max velocity used when holding down the sprint modifier button.
    [SerializeField] private float rotationSpeed = 10f; // Slerp modifier determining how quickly the character turns toward their path.
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletCoolDown = 0.5f;
    private bool canShoot = true;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f; // The specific height ceiling (in Unity meters) your jump can reach.
    [SerializeField] private float gravity = -9.8f; // Downward acceleration rate applied every frame while airborne.

    [Header("References")]
    [SerializeField] private Transform cameraTransform; // Points to your camera rig so movement can orient toward your view path.
    [SerializeField] private Transform yawTarget; // Points to your camera's horizontal swivel anchor to align body aim directions.
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject bulletpoint;


    private CharacterController controller; // Cached reference to Unity's built-in collision and movement wrapper.
    private Vector2 moveInput; // Stores current raw WASD or joystick directional coordinates from the input system.
    private Vector3 velocity; // Tracks separate cumulative physics forces (like current gravity velocity and jump bursts).
    private bool isSprinting; // Flag showing whether the player is currently actively sprinting.

    [HideInInspector] // Hidden from Inspector since CameraSwitcher modifies this variable automatically via code.
    public bool isAiming; // Flag showing whether the player is currently aiming down sights.

    // Runs once right when the scene starts playing.
    void Start()
    {
        // Finds and stores the CharacterController component attached to this specific character GameObject.
        controller = GetComponent<CharacterController>();
    }

    // Runs automatically every frame to process movement updates, rotation updates, and gravity physics.
    void Update()
    {
        // CRITICAL GROUNDING PHYSICS FIX:
        // Checks if the character controller is firmly touching the floor geometry.
        if (controller.isGrounded && velocity.y < 0)
        {
            // Lock downward velocity to a small constant negative force. 
            // This resets a massive falling buildup while keeping the player cleanly stuck to slopes/steps.
            velocity.y = -2f;
        }

        // Uses a ternary comparison operator to set movement speed depending on whether sprinting is active.
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Creates a blank baseline vector to calculate our final horizontal movement vector.
        Vector3 moveDirection = Vector3.zero;

        // EVALUATION BRANCH: Check if the player is currently holding down the aim button.
        if (isAiming)
        {
            // CASE A: Player is aiming. Move relative to the player's current body orientation (strafing behavior).
            Vector3 forward = transform.forward; // Get the character's direct forward vector.
            Vector3 right = transform.right; // Get the character's direct right-hand vector.

            // Zero out vertical values to prevent looking down/up from accidentally pulling your player into the floor or sky.
            forward.y = 0f;
            right.y = 0f;

            // Normalizes the vectors to length 1 so moving diagonally doesn't make your player run faster.
            forward.Normalize();
            right.Normalize();

            // Blends input axes with body axes so W/S moves forward/back, and A/D cleanly strafes left/right.
            moveDirection = forward * moveInput.y + right * moveInput.x;
        }
        else
        {
            // CASE B: Player is not aiming. Move relative to where the exploration camera is currently facing.
            Vector3 forward = cameraTransform.forward; // Get the camera's current forward facing directional path.
            Vector3 right = cameraTransform.right; // Get the camera's current right-hand directional path.

            // Zero out vertical values to make sure camera tilts do not disrupt horizontal ground movement speeds.
            forward.y = 0f;
            right.y = 0f;

            // Normalizes vectors to ensure a clean length of 1 regardless of camera angle settings.
            forward.Normalize();
            right.Normalize();

            // Blends input axes with camera directions so pressing W moves you exactly forward into your field of view.
            moveDirection = forward * moveInput.y + right * moveInput.x;
        }

        // Executes horizontal movement calculations inside Unity's collision system, scaled cleanly by frame rates.
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // EVALUATION BRANCH: Handles mesh rotation behaviors based on current combat states.
        if (isAiming)
        {
            // Fetch the direction vector that your camera's horizontal tracking target is pointing.
            Vector3 lookDirection = yawTarget.forward;
            lookDirection.y = 0; // Strip height updates to prevent the whole player mesh from awkwardly tipping over.

            // Checks to ensure the vector has actual magnitude before performing rotation math.
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                // Calculates the exact math rotation looking down the line of your camera tracking rig.
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

                // Smoothly forces the player's body mesh to snap directly toward that viewpoint over time.
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        // If exploring and the user is physically pressing WASD keys (movement input is active).
        else if (moveInput != Vector2.zero)
        {
            // Calculates the mathematical rotation facing the exact direction the player is running.
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            // Smoothly rotates the character mesh to look down their current running path over time.
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Continuously scales down our vertical velocity tracking variable using our gravity force setting.
        velocity.y += gravity * Time.deltaTime;

        // Executes vertical movement updates inside the collision loop to process jump bursts and downward gravity drops.
        controller.Move(velocity * Time.deltaTime);
    }

    // Automatically fired via a Player Input component broadcast when WASD or left joystick movements change.
    public void OnMove(InputAction.CallbackContext context)
    {
        // Extracts the 2D vector coordinate values (-1 to 1) and saves them into our private moveInput tracking variable.
        moveInput = context.ReadValue<Vector2>();
    }

    // Automatically fired via a Player Input component broadcast when the sprint button shifts.
    public void OnSprint(InputAction.CallbackContext context)
    {
        // If the button is pushed down, mark sprinting as active.
        if (context.started) isSprinting = true;
        // If the button is released, mark sprinting as inactive.
        else if (context.canceled) isSprinting = false;
    }

    // Automatically fired via a Player Input component broadcast when the jump button is hit.
    public void OnJump(InputAction.CallbackContext context)
    {
        // Validates that the jump action button was completely pressed AND confirms the character is touching solid ground.
        if (context.performed && controller.isGrounded)
        {
            // Physics Formula (v = sqrt(h * -2 * g)): Calculates the upward explosive vertical velocity burst needed to hit the target height.
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed && isAiming && canShoot)
        {
            canShoot = false;

            GameObject bulletShot = Instantiate(bullet, bulletpoint.transform.position, transform.rotation);

            Invoke("ResetShoot", bulletCoolDown);
        }
    }

    void ResetShoot()
    {
        canShoot = true;
    }
}
