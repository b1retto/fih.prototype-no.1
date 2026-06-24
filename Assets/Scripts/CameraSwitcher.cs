using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera freelookCam;
    [SerializeField] private CinemachineCamera aimCam;
    [SerializeField] private CinemachineInputAxisController inputAxisController;

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private WorldCrossHairController crosshairController;

    private PlayerActions input;
    private InputAction aimAction;
    private AimCameraController aimCameraController;
    private CinemachineOrbitalFollow orbitalFollow;
    private bool isAiming;

    void Start()
    {
        aimCameraController = aimCam.GetComponent<AimCameraController>();
        inputAxisController = freelookCam.GetComponent<CinemachineInputAxisController>();
        orbitalFollow = freelookCam.GetComponent<CinemachineOrbitalFollow>();

        input = new PlayerActions();
        input.Enable();
        aimAction = input.Gameplay.Aim;
    }

    void Update()
    {
        bool aimPressed = aimAction.IsPressed();
        player.isAiming = aimPressed;

        if (aimPressed && !isAiming) EnterAimMode();
        else if (!aimPressed && isAiming) ExitAimMode();
    }

    private void EnterAimMode()
    {
        isAiming = true;
        aimCameraController.SetYawPitchFromCameraForward(freelookCam.transform);

        ToggleCameraState(aimPriority: 20, freelookPriority: 10, crosshair: true, enableAxis: false);
    }

    private void ExitAimMode()
    {
        isAiming = false;
        SnapFreeLookBehindPlayer();

        ToggleCameraState(aimPriority: 10, freelookPriority: 20, crosshair: false, enableAxis: true);
    }

    private void ToggleCameraState(int aimPriority, int freelookPriority, bool crosshair, bool enableAxis)
    {
        aimCam.Priority = aimPriority;
        freelookCam.Priority = freelookPriority;
        inputAxisController.enabled = enableAxis;

        if (crosshairController) crosshairController.SetCrosshairVisibility(crosshair);
    }

    private void SnapFreeLookBehindPlayer()
    {
        Vector3 forward = aimCam.transform.forward;
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        if (orbitalFollow) orbitalFollow.HorizontalAxis.Value = angle;
    }

    public void CleanUpInputBeforeReload()
    {
        input?.Disable();
    }
}
