using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float bulletCoolDown = 0.5f;
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBarScript healthBar;
    private bool canShoot = true;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.8f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform yawTarget;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject bulletpoint;
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

    [HideInInspector] public bool isAiming;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (runParticle.isPlaying) runParticle.Stop();
            if (audioSource.isPlaying && (audioSource.clip == runningSound || audioSource.clip == walkingSound))
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.loop = false;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.B)) TakeDamage(20);

        if (controller.isGrounded && isSprinting && moveInput != Vector2.zero)
        {
            if (!runParticle.isPlaying || audioSource.clip != runningSound)
            {
                runParticle.Play();
                audioSource.clip = runningSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (runParticle.isPlaying) runParticle.Stop();
            if (audioSource.clip == runningSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.loop = false;
            }
        }

        if (controller.isGrounded && !isSprinting && moveInput != Vector2.zero)
        {
            if (!audioSource.isPlaying || audioSource.clip != walkingSound)
            {
                audioSource.clip = walkingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.clip == walkingSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.loop = false;
            }
        }

        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Transform reference = isAiming ? transform : cameraTransform;
        Vector3 forward = reference.forward;
        Vector3 right = reference.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;

        if (moveDirection != Vector3.zero) runParticle.transform.forward = -moveDirection.normalized;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (isAiming)
        {
            Vector3 lookDirection = yawTarget.forward;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            if (Camera.main != null && shootParticle != null)
                shootParticle.transform.forward = Camera.main.transform.forward;
        }
        else if (moveInput != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;
        if (context.performed && controller.isGrounded)
        {
            jumpParticle.Play();
            AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;
        if (context.performed && isAiming && canShoot)
        {
            canShoot = false;
            Instantiate(bullet, bulletpoint.transform.position, transform.rotation);
            shootParticle.Play();
            audioSource.PlayOneShot(pewpewSound);
            Invoke(nameof(ResetShoot), bulletCoolDown);
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }

    void ResetShoot() => canShoot = true;
}