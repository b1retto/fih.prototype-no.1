using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float bulletRotationSpeed = 60f;

    private Vector3 lockedTargetLocation;

    void Start()
    {
        var crosshair = GameObject.Find("crosshair");
        if (crosshair && crosshair.TryGetComponent(out WorldCrossHairController crossHairScript))
        {
            lockedTargetLocation = crossHairScript.transform.position;
        }
        else
        {
            lockedTargetLocation = transform.position + transform.forward * 100f;
        }
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, lockedTargetLocation, speed * Time.deltaTime);
        transform.Rotate(0f, bulletRotationSpeed * Time.deltaTime, 0f);

        if (transform.position == lockedTargetLocation)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
