using UnityEngine;
public class WorldCrossHairController : MonoBehaviour
{

    [Header("UI Reference")]
    [SerializeField] private RectTransform crosshairUI; // The UI element. **REQUIRED: Canvas MUST be set to World Space mode!**

    [Header("Camera & Raycasting")]
    [SerializeField] private Camera aimCamera; // The main camera rendering your game view.
    [SerializeField] private float maxDistance = 20f; // Maximum distance the crosshair can look out into the world.
    [SerializeField] private LayerMask raycastMask = -0; // Filters which layers/objects the crosshair raycast will hit.
    public RaycastHit publicHit;

    [Header("Visual Tuning")]
    [SerializeField] private float crossHairOffsetMultiplier = 0.01f; // Tiny distance to push the UI off surfaces so it doesn't clip through walls.

    // Runs once when the scene begins playing.
    void Start()
    {
        // Hides the standard Windows/OS mouse pointer from the player's screen.
        Cursor.visible = false;

        // Locks the hidden cursor to the exact center of the game screen so it cannot drift off-screen.
        Cursor.lockState = CursorLockMode.Locked;

        // Start with the crosshair hidden by default
        SetCrosshairVisibility(false);

    }

    // Runs automatically every frame to recalculate the 3D crosshair position.
    void Update()
    {
        //If the crosshair is turned off, skip all the heavy raycast math entirely
        if (!crosshairUI.gameObject.activeSelf) return;

        // 1. Find the literal absolute center coordinate of your game screen in pixels.
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        // 2. Creates a mathematical laser "Ray" passing from the camera lens out through that center screen pixel.
        Ray ray = aimCamera.ScreenPointToRay(screenCenter);

        // This variable will hold our final target 3D world coordinate for the crosshair.
        Vector3 targetPos;

        // 3. SHOOT THE RAY: Fire the laser into the scene using our configured distance and layer mask limits.
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask))
        {
            publicHit = hit;
            // CASE A: The laser struck an object (a wall, floor, enemy, etc.).

            // Calculate a point slightly hovering off the hit surface using the surface normal (outward facing vector).
            targetPos = hit.point + hit.normal * crossHairOffsetMultiplier;

            // Forces the UI crosshair flat against the surface angle so it conforms perfectly to the geometry.
            crosshairUI.rotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            // CASE B: The laser hit nothing (looking directly up at the open sky or a far horizon void).

            // Extends the ray straight forward out into empty space to its maximum allowed configuration distance.
            targetPos = ray.GetPoint(maxDistance);

            // Forces the crosshair to stay perfectly flat relative to the camera lens' forward viewpoint.
            crosshairUI.forward = aimCamera.transform.forward;
        }

        // 4. Update the actual 3D transformation location of the world canvas crosshair UI element.
        crosshairUI.position = targetPos;
    }

    //Added public method so the CameraSwitcher script can turn the crosshair on and off
    public void SetCrosshairVisibility(bool visible)
    {
        if (crosshairUI != null)
        {
            crosshairUI.gameObject.SetActive(visible);
        }
    }
}
