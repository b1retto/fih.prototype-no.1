using Unity.VisualScripting;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    // References the crosshair controller script in the scene
    private WorldCrossHairController crossHairScript;

    // Movement and spinning speeds for the bullet
    [SerializeField] private float speed = 10f;
    [SerializeField] private float bulletRotationSpeed = 60f;

    // Stores the static destination position captured at spawn
    private Vector3 lockedTargetLocation;

    void Start()
    {
        // Finds the crosshair object and gets its controller component
        crossHairScript = GameObject.Find("crosshair").GetComponent<WorldCrossHairController>();

        // Safely saves the crosshair's current position as the target
        if (crossHairScript != null)
        {
            lockedTargetLocation = crossHairScript.transform.position;
        }
    }

    void Update()
    {
        // Safety check to ensure the bullet exists before moving it
        if (gameObject != null)
        {
            // Moves the bullet steadily toward the locked destination coordinates
            transform.position = Vector3.MoveTowards(transform.position, lockedTargetLocation, speed * Time.deltaTime);

            // Spins the bullet around its Y-axis as it flies
            transform.Rotate(0f, bulletRotationSpeed * Time.deltaTime, 0f);
        }

        // Deletes the bullet once it successfully reaches the exact target location
        if (transform.position == lockedTargetLocation)
        {
            Destroy(gameObject);
            return;
        }
    }

    // Triggers automatically when the bullet passes through another collider
    void OnTriggerEnter(Collider other)
    {
        // Ignores the player completely so the bullet doesn't hit its shooter
        if (other.CompareTag("Player"))
        {
            return;
        }

        // Destroys the bullet upon impacting any other valid object or wall
        Destroy(gameObject);
    }
}
