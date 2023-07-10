using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    private int ENEMY_LAYER = 9;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == ENEMY_LAYER)
        {
            other.gameObject.GetComponent<EnemyAI>().GetDamage();
        }
    }
}
