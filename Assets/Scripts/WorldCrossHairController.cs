using UnityEngine;

public class WorldCrossHairController : MonoBehaviour
{
    [Header("References")]
    public RectTransform crosshairUI;

    [Header("Camera & Raycasting")]
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask raycastMask = -0;
    public RaycastHit publicHit;

    [Header("Visual Tuning")]
    [SerializeField] private float crossHairOffsetMultiplier = 0.01f;


    void Awake()
    {
        SetCrosshairVisibility(false);
    }

    void Start()
    {
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }
    }

    void Update()
    {
        if (crosshairUI == null || aimCamera == null) return;
        if (!crosshairUI.gameObject.activeSelf) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        Ray ray = aimCamera.ScreenPointToRay(screenCenter);

        Vector3 targetPos;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask))
        {
            publicHit = hit;
            targetPos = hit.point + hit.normal * crossHairOffsetMultiplier;
            crosshairUI.rotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            targetPos = ray.GetPoint(maxDistance);
            crosshairUI.forward = aimCamera.transform.forward;
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
