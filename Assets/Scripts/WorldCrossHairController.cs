using UnityEngine;

public class WorldCrossHairController : MonoBehaviour
{
    [SerializeField] private RectTransform crosshairUI;
    [SerializeField] private Camera aimCamera;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float crossHairOffsetMultiplier = 0.01f;
    [SerializeField] private LayerMask raycastMask = -0;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = aimCamera.ScreenPointToRay(screenCenter);

        Vector3 targetPos;
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask))
        {
            targetPos = hit.point + hit.normal * crossHairOffsetMultiplier;
            crosshairUI.rotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            targetPos = ray.GetPoint(maxDistance);
            crosshairUI.forward = aimCamera.transform.transform.forward;
        }

        crosshairUI.position = targetPos;
    }
}
