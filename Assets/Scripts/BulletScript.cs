using Unity.VisualScripting;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private WorldCrossHairController crossHairScript;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float bulletRotationSpeed = 60f;

    private Vector3 lockedTargetLocation;

    void Start()
    {
        crossHairScript = GameObject.Find("crosshair").GetComponent<WorldCrossHairController>();

        if (crossHairScript != null)
        {
            lockedTargetLocation = crossHairScript.transform.position;
        }
    }

    void Update()
    {
        if (gameObject != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, lockedTargetLocation, speed * Time.deltaTime);
            transform.Rotate(0f, bulletRotationSpeed * Time.deltaTime, 0f);
        }

        if (transform.position == lockedTargetLocation)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }

        Destroy(gameObject);
    }
}
