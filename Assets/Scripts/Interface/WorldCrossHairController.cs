using UnityEngine;

public class WorldCrosshairController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private RectTransform crosshairUI;

    [Header("Camera & Raycasting")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask raycastMask = ~0; // Fixed: was -0 (invalid), corrected to ~0 (all layers)

    public RaycastHit publicHit;

    [Header("Visual Tuning")]
    [SerializeField] private float crossHairOffsetMultiplier = 0.01f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SetCrosshairVisibility(false);
    }

    void Update()
    {
        if (!crosshairUI.gameObject.activeSelf) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = aimCamera.ScreenPointToRay(screenCenter);
        Vector3 targetPos;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask))
        {
            publicHit = hit;
            targetPos = hit.point + hit.normal * crossHairOffsetMultiplier;

            // Aligns the crosshair flat on the surface while keeping its rotation upright relative to the camera
            crosshairUI.rotation = Quaternion.LookRotation(hit.normal, aimCamera.transform.up);
        }
        else
        {
            targetPos = ray.GetPoint(maxDistance);
            crosshairUI.rotation = Quaternion.LookRotation(aimCamera.transform.forward, aimCamera.transform.up);
        }

        crosshairUI.position = targetPos;
    }

    public void SetCrosshairVisibility(bool visible)
    {
        if (crosshairUI != null)
        {
            crosshairUI.gameObject.SetActive(visible);
        }
    }
}