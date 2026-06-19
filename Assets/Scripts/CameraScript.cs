using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    // Zoom configuration
    [SerializeField] private float zoomSpeed = 2f;        // How fast the zoom reacts to scroll input
    [SerializeField] private float zoomLerpSpeed = 10f;   // How smoothly the zoom transitions
    [SerializeField] private float minDistance = 3f;      // Closest the camera can get to the target
    [SerializeField] private float maxDistance = 15f;     // Farthest the camera can get from the target

    // Input
    private PlayerActions actions;    // Generated input actions asset
    private Vector2 scrollDelta;      // Stores the current frame's scroll input

    // Cinemachine references
    private CinemachineCamera cam;             // Reference to the Cinemachine camera component
    private CinemachineOrbitalFollow orbital;  // Controls the orbital radius (zoom distance)

    // Zoom state
    private float targetZoom;   // The desired zoom level we are moving towards
    private float currentZoom;  // The current interpolated zoom level

    void Start()
    {
        // Initialize and enable the input actions
        actions = new PlayerActions();
        actions.Enable();

        // Subscribe to the mouse scroll input event
        actions.CameraControls.MouseZoom.performed += HandleMouseScroll;

        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        // Grab the Cinemachine components from this GameObject
        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();

        // Initialize zoom values to match the current orbital radius
        targetZoom = currentZoom = orbital.Radius;
    }

    // Called when the mouse scroll input is performed
    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        // Read and store the 2D scroll value (y axis is the scroll wheel)
        scrollDelta = context.ReadValue<Vector2>();
    }

    void Update()
    {
        // Only process zoom if there is scroll input and the orbital component exists
        if (scrollDelta.y != 0)
        {
            if (orbital != null)
            {
                // Calculate new target zoom, clamped between min and max distance
                targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * zoomSpeed, minDistance, maxDistance);

                // Reset scroll delta after consuming the input
                scrollDelta = Vector2.zero;
            }
        }

        // Smoothly interpolate current zoom towards the target zoom
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);

        // Apply the interpolated zoom to the orbital follow radius
        orbital.Radius = currentZoom;
    }
}