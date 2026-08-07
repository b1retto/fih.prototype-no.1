using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float bulletRotationSpeed = 3600f;

    [SerializeField] private float overshootDistance = 5f;

    private Vector3 finalDestroyPoint;

    void Start()
    {
        var crosshair = GameObject.Find("crosshair");

        if (crosshair != null)
        {
            Vector3 crosshairPos = crosshair.transform.position;

            Vector3 directionToCrosshair = (crosshairPos - transform.position).normalized;

            float distanceToCrosshair = Vector3.Distance(transform.position, crosshairPos);
            float totalDistance = distanceToCrosshair + overshootDistance;

            finalDestroyPoint = transform.position + (directionToCrosshair * totalDistance);
        }
        else
        {
            Vector3 direction = transform.forward;
            float totalDistance = 100f + overshootDistance;
            finalDestroyPoint = transform.position + (direction * totalDistance);
        }
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, finalDestroyPoint, speed * Time.deltaTime);

        transform.Rotate(0f, bulletRotationSpeed * Time.deltaTime, 0f);

        if (Vector3.Distance(transform.position, finalDestroyPoint) < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}