using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera freelookCam;
    [SerializeField] private CinemachineCamera aimCam;
    [SerializeField] private CinemachineInputAxisController inputAxisController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private PlayerActions input;

    private InputAction aimAction;
    private bool isAiming = false;
    private Transform yawTarget;
    private Transform pitchTarget;

    private AimCameraController aimCameraController;

    void Start()
    {
        aimCameraController = aimCam.GetComponent<AimCameraController>();

        inputAxisController = freelookCam.GetComponent<CinemachineInputAxisController>();

        input = new PlayerActions();
        input.Enable();
        aimAction = input.Gameplay.Aim;
    }

    void Update()
    {
        bool aimPressed = aimAction.IsPressed();
        player.isAiming = aimPressed;

        if (aimPressed && !isAiming)
        {
            EnterAimMode();
        }

        else if (!aimPressed && isAiming)
        {
            ExitAimMode();
        }
    }

    private void ExitAimMode()
    {
        isAiming = false;

        SnapFreeLookToBehindPlayer();

        aimCam.Priority = 10;
        freelookCam.Priority = 20;

        inputAxisController.enabled = true;
    }

    private void SnapFreeLookToBehindPlayer()
    {
        CinemachineOrbitalFollow orbitalFollow = freelookCam.GetComponent<CinemachineOrbitalFollow>();
        Vector3 forward = aimCam.transform.forward;
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        orbitalFollow.HorizontalAxis.Value = angle;
    }

    private void SnapAimCameraToPlayerForward()
    {
        aimCameraController.SetYawPitchFromCameraForward(freelookCam.transform);
    }

    private void EnterAimMode()
    {
        isAiming = true;

        SnapAimCameraToPlayerForward();

        aimCam.Priority = 20;
        freelookCam.Priority = 10;

        inputAxisController.enabled = false;
    }
}