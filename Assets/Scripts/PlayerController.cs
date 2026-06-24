using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f; // Controls how smoothly the player turns - higher = snappier turns
    [SerializeField] private float bulletCoolDown = 0.5f;
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBarScript healthBar;
    private bool canShoot = true; // Acts like a gate - true means the player is allowed to fire

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    // How strongly gravity pulls the player down each second (-9.8 mimics real world gravity)
    [SerializeField] private float gravity = -9.8f;

    [Header("References")]
    // Drag your camera here so movement knows which direction is "forward"
    [SerializeField] private Transform cameraTransform;
    // The anchor point that tracks the camera's left/right rotation for aiming
    [SerializeField] private Transform yawTarget;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject bulletpoint; // Bullets creation point
    [SerializeField] private ParticleSystem jumpParticle;
    [SerializeField] private ParticleSystem runParticle;
    [SerializeField] private ParticleSystem shootParticle;
    [SerializeField] private AudioClip pewpewSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private AudioClip walkingSound;

    public AudioSource audioSource;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isSprinting;

    [HideInInspector]
    public bool isAiming;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        // 1. When time is paused, explicitly stop looping movement sounds & run particles
        if (Time.timeScale == 0f)
        {
            if (runParticle != null && runParticle.isPlaying)
            {
                runParticle.Stop();
            }

            if (audioSource != null && audioSource.isPlaying && (audioSource.clip == runningSound || audioSource.clip == walkingSound))
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.loop = false;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            TakeDamage(20);
        }

        if (controller.isGrounded && isSprinting == true && moveInput != Vector2.zero)
        {
            if (!runParticle.isPlaying || !audioSource.isPlaying || audioSource.clip != runningSound)
            {
                runParticle.Play();
                audioSource.clip = runningSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (!controller.isGrounded || isSprinting == false || moveInput == Vector2.zero)
        {
            if (runParticle.isPlaying)
            {
                runParticle.Stop();
            }

            if (audioSource.clip == runningSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.loop = false;
            }
        }

        if (controller.isGrounded && isSprinting == false && moveInput != Vector2.zero)
        {
            if (!audioSource.isPlaying || audioSource.clip != walkingSound)
            {
                audioSource.clip = walkingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (!controller.isGrounded || isSprinting == true || moveInput == Vector2.zero)
        {
            if (audioSource.clip == walkingSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.loop = false;
            }
        }

        // If the player is on the ground and being pushed down
        if (controller.isGrounded && velocity.y < 0)
        {
            // A small negative value keeps the player firmly pressed against the ground
            velocity.y = -2f;
        }

        // If(isSprinting) use sprintSpeed, else use walkSpeed
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // This will hold our final movement direction once we calculate it below
        Vector3 moveDirection = Vector3.zero;

        if (isAiming)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Remove any vertical tilt so the player only moves horizontally
            forward.y = 0f;
            right.y = 0f;

            // Make sure the vectors are exactly length 1 so diagonal movement isn't faster
            forward.Normalize();
            right.Normalize();

            // Combine the input axes with the body directions to get final movement
            moveDirection = forward * moveInput.y + right * moveInput.x;
        }
        else
        {
            // Pressing W always moves you into the screen, regardless of body rotation
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            // Flatten the vectors so camera tilt doesn't affect ground movement
            forward.y = 0f;
            right.y = 0f;

            // Normalize to prevent diagonal speed boost
            forward.Normalize();
            right.Normalize();

            // Build the movement direction from camera orientation and player input
            moveDirection = forward * moveInput.y + right * moveInput.x;
        }

        if (moveDirection != Vector3.zero)
        {
            runParticle.transform.forward = -moveDirection.normalized;
        }

        // Actually move the player horizontally
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (isAiming)
        {
            // While aiming, rotate the player body to match where the camera is looking
            Vector3 lookDirection = yawTarget.forward;

            // Zero out Y to stop the player mesh from tilting up or down
            lookDirection.y = 0;

            // Only rotate if there's a valid direction (avoids math errors with zero vectors)
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            if (Camera.main != null && shootParticle != null)
            {
                shootParticle.transform.forward = Camera.main.transform.forward;
            }
        }
        else if (moveInput != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Apply gravity every frame - velocity.y gets more negative the longer we're airborne
        velocity.y += gravity * Time.deltaTime;

        // Move the player vertically based on our accumulated velocity (gravity + jump force)
        controller.Move(velocity * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // 2. Removed Time.timeScale check so releasing keys while paused updates moveInput to zero!
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        // 2. Removed Time.timeScale check so releasing Shift while paused updates isSprinting to false!
        if (context.started) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f)
            return;

        if (context.performed && controller.isGrounded)
        {
            jumpParticle.Play();
            AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f)
            return;

        if (context.performed && isAiming && canShoot)
        {
            canShoot = false;
            GameObject bulletShot = Instantiate(bullet, bulletpoint.transform.position, transform.rotation);

            shootParticle.Play();
            AudioSource.PlayClipAtPoint(pewpewSound, transform.position);

            Invoke("ResetShoot", bulletCoolDown);
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }

    void ResetShoot()
    {
        canShoot = true;
    }
}