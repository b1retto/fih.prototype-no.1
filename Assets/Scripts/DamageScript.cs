using UnityEngine;

public class DamageScript : MonoBehaviour
{
    [SerializeField] private int damage = 50;

    public void ExecuteDamage(PlayerController player)
    {
        player.TakeDamage(damage);
    }
}
