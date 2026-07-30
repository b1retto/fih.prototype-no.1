using UnityEngine;
using UnityEngine.InputSystem;
using System;

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
    private bool jumped = false;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.8f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform, yawTarget;
    [SerializeField] private Transform crosshairTransform;
    [SerializeField] private GameObject bullet, bulletpoint;
    [SerializeField] private ParticleSystem jumpParticle, runParticle, shootParticle;
    [SerializeField] private AudioClip pewpewSound, jumpSound, runningSound, walkingSound, hurtSound;

    public GameObject cucumber;
    public AudioSource audioSource, audioSource2;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isSprinting;

    [HideInInspector] public bool isAiming;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        if (crosshairTransform == null && Camera.main != null)
            crosshairTransform = Camera.main.transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (cucumber != null && crosshairTransform != null)
            {
                Vector3 spawnPos = crosshairTransform.position;
                Instantiate(cucumber, spawnPos, cucumber.transform.rotation);
            }
        }

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Transform reference = isAiming ? transform : cameraTransform;
        Vector3 forward = reference.forward;
        Vector3 right = reference.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;

        if (moveDirection != Vector3.zero && !isAiming)
            runParticle.transform.forward = -moveDirection.normalized;

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

        if (Time.timeScale == 0f)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                if (audioSource.clip == walkingSound || audioSource.clip == runningSound)
                    audioSource.clip = null;
            }
            if (runParticle != null && runParticle.isPlaying)
                runParticle.Stop();
        }
        else if (controller.isGrounded && moveInput != Vector2.zero && audioSource != null)
        {
            AudioClip targetClip = isSprinting ? runningSound : walkingSound;
            if (audioSource.clip != targetClip)
            {
                audioSource.clip = targetClip;
                audioSource.loop = true;
                audioSource.Play();
            }
            if (isSprinting && runParticle != null && !runParticle.isPlaying)
                runParticle.Play();
            if (!isSprinting && runParticle != null && runParticle.isPlaying)
                runParticle.Stop();
        }
        else if ((!controller.isGrounded || moveInput == Vector2.zero) && audioSource != null)
        {
            if (audioSource.clip == walkingSound || audioSource.clip == runningSound)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }

            if (jumped && !controller.isGrounded)
            {
                jumped = false;
                audioSource.PlayOneShot(jumpSound);
            }

            if (runParticle != null && runParticle.isPlaying)
                runParticle.Stop();
        }
    }

    void OnDisable()
    {
        moveInput = Vector2.zero;
        if (runParticle != null && runParticle.isPlaying)
            runParticle.Stop();

        if (audioSource != null && audioSource.isPlaying &&
            (audioSource.clip == runningSound || audioSource.clip == walkingSound))
        {
            audioSource.loop = false;
            audioSource.clip = null;
            audioSource.Stop();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
            isSprinting = true;
        else if (context.canceled)
            isSprinting = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f)
            return;

        if (context.performed && controller.isGrounded)
        {
            jumped = true;
            if (jumpParticle != null)
                jumpParticle.Play();
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f)
            return;

        if (context.performed && isAiming && canShoot && bullet != null && bulletpoint != null)
        {
            canShoot = false;
            Instantiate(bullet, bulletpoint.transform.position, transform.rotation);

            if (pewpewSound != null && audioSource != null)
                audioSource.PlayOneShot(pewpewSound);

            if (bulletCoolDown > 0f)
                Invoke(nameof(ResetShoot), bulletCoolDown);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Time.timeScale == 0)
        {
            if (audioSource2 != null)
            {
                audioSource2.Stop();
                audioSource2.clip = null;
            }
            return;
        }

        DamageScript damagePart = hit.gameObject.GetComponent<DamageScript>();

        if (damagePart != null)
        {
            int damageAmount = damagePart.ExecuteDamage(this);

            if (damageAmount > 0 && hurtSound != null && audioSource2 != null)
            {
                audioSource2.clip = hurtSound;
                audioSource2.Play();
            }
        }
    }
    public int TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
        return damage;
    }

    void ResetShoot() => canShoot = true;
}