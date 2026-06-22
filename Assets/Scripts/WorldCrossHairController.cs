using UnityEngine;

public class WorldCrossHairController : MonoBehaviour
{
    [Header("References")]
    // The actual crosshair image component (Requires a Canvas set to World Space)
    [SerializeField] private RectTransform crosshairUI;
    [SerializeField] private UIManager uiManager;

    [Header("Camera & Raycasting")]
    [SerializeField] private Camera aimCamera; // The game camera looking at the world
    [SerializeField] private float maxDistance = 20f; // How far away the crosshair can look/reach
    [SerializeField] private LayerMask raycastMask = -0; // Filter to choose what objects the crosshair can hit
    // Stores the hit data publicly so other scripts can see what you are aiming at
    public RaycastHit publicHit;

    [Header("Visual Tuning")]
    // Small buffer distance to stop the crosshair from glitching or sinking inside walls
    [SerializeField] private float crossHairOffsetMultiplier = 0.01f;


    void Awake()
    {
        // Turns the crosshair off when the game starts
        SetCrosshairVisibility(false);
    }

    // Runs once when the game starts
    void Start()
    {
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }
    }

    // Runs every single frame
    void Update()
    {
        // Safe check to prevent errors if references are completely missing
        if (crosshairUI == null || aimCamera == null) return;

        // If the crosshair is hidden, stop running the heavy math below
        if (!crosshairUI.gameObject.activeSelf) return;

        // Find the exact center pixel of your screen
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        // Shoots an invisible mathematical line (Ray) straight out from the center of the camera view
        Ray ray = aimCamera.ScreenPointToRay(screenCenter);

        // This will store where the crosshair should physically sit in 3D space
        Vector3 targetPos;

        // Physics.Raycast shoots the line out. Returns TRUE if it hits something.
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask))
        {
            // Saves the hit data so other scripts can access it
            publicHit = hit;

            // Math: Hit point is the exact spot on the wall. Normal is the direction pushing out of the wall surface.
            // This line calculates a position slightly hovering off the surface so it stays visible.
            targetPos = hit.point + hit.normal * crossHairOffsetMultiplier;

            // Rotates the crosshair image flat against whatever angle it hit (slopes, walls, etc.)
            crosshairUI.rotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            // If the ray hits nothing (looking at the sky), place it at the maximum distance in mid-air
            targetPos = ray.GetPoint(maxDistance);

            // Rotates the crosshair to stay perfectly flat against your computer screen
            crosshairUI.forward = aimCamera.transform.forward;
        }

        // Moves the crosshair image to the newly calculated 3D world position
        crosshairUI.position = targetPos;
    }

    // A public function that other scripts can call to turn the crosshair on or off
    public void SetCrosshairVisibility(bool visible)
    {
        if (crosshairUI != null)
        {
            // Activates or deactivates the UI object based on the True/False value passed in
            crosshairUI.gameObject.SetActive(visible);
        }
    }
}
