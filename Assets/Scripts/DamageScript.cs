using UnityEngine;

public class DamageScript : MonoBehaviour
{
    [SerializeField] private int damage = 50;
    private bool canDamage = true;

    public int ExecuteDamage(PlayerController player)
    {
        if (canDamage)
        {
            player.TakeDamage(damage);
            canDamage = false;
            Invoke("ResetCooldown", 0.1f);
            return damage;
        }
        return 0;
    }

    private void ResetCooldown()
    {
        canDamage = true;
    }
}
