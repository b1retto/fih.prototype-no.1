using UnityEngine;

public class DamageScript : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private int damage = 50;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController.TakeDamage(damage);
        }
    }
}
