using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimCameraController : MonoBehaviour
{
    [Header("Camera Targets")]
    [SerializeField] private Transform yawTarget;
    [SerializeField] private Transform pitchTarget;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference lookInput;
    [SerializeField] private InputActionReference switchShoulderInput;

    [Header("Sensitivity Settings")]
    [SerializeField] private float mouseSensitivity = 0.05f;
    [SerializeField] private float sensitivity = 1.5f;

    [Header("Rotation Constraints")]
    [SerializeField] private float pitchMin = -40f;
    [SerializeField] private float pitchMax = 80f;

    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineThirdPersonFollow aimCam;
    [SerializeField] private float shoulderSwitchSpeed = 5f;

    private float yaw;
    private float pitch;
    private float targetCameraSide;

    void Awake()
    {
        if (!aimCam) aimCam = GetComponent<CinemachineThirdPersonFollow>();
        if (aimCam) targetCameraSide = aimCam.CameraSide;
    }

    void Start()
    {
        Vector3 angles = yawTarget.rotation.eulerAngles;
        yaw = angles.y;
        pitch = angles.x > 180 ? angles.x - 360 : angles.x;

        lookInput.asset.Enable();
    }

    void OnEnable()
    {
        switchShoulderInput.action.Enable();
        switchShoulderInput.action.performed += OnSwitchShoulder;
    }

    void OnDisable()
    {
        switchShoulderInput.action.Disable();
        switchShoulderInput.action.performed -= OnSwitchShoulder;
    }

    void Update()
    {
        Vector2 look = lookInput.action.ReadValue<Vector2>();

        if (Mouse.current != null && Mouse.current.delta.IsActuated())
        {
            look *= mouseSensitivity;
        }

        yaw += look.x * sensitivity;
        pitch = Mathf.Clamp(pitch - (look.y * sensitivity), pitchMin, pitchMax);

        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (aimCam)
        {
            aimCam.CameraSide = Mathf.MoveTowards(aimCam.CameraSide, targetCameraSide, shoulderSwitchSpeed * Time.deltaTime);
        }
    }

    private void OnSwitchShoulder(InputAction.CallbackContext context)
    {
        if (aimCam) targetCameraSide = targetCameraSide < 0.5f ? 1f : 0f;
    }

    public void SetYawPitchFromCameraForward(Transform cameraTransform)
    {
        Vector3 flatForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
        if (flatForward == Vector3.zero) return;

        yaw = Quaternion.LookRotation(flatForward).eulerAngles.y;
        pitch = 0f;

        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchTarget.localRotation = Quaternion.identity;
    }
}
