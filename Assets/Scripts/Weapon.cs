using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

public class Weapon : MonoBehaviour
{
    public int weaponDamage = 1;
    public float hitBoxCenterY = 0.25f;
    public float hitBoxRadius = 0.25f;
    public float hitBoxHeight = 1.5f;
    public float weaponCd = 0.1f;

    private CapsuleCollider weaponCollider;
    
    private bool currentlyAttacking = false;

    private int initialWeaponDamage;
    private float initialWeaponRangeValue;
    private float initialWeaponCd;
    public Sequence attackingSequence;

    private void Start()
    {
        weaponCollider = GetComponent<CapsuleCollider>();

        UpdateWeaponRange();
        
        weaponCollider.enabled = false;

        initialWeaponDamage = weaponDamage;
        initialWeaponRangeValue = hitBoxHeight;
        initialWeaponCd = weaponCd;
    }

    private void UpdateWeaponRange()
    {
        weaponCollider.center = new Vector3(0, hitBoxCenterY, 0);
        weaponCollider.radius = hitBoxRadius;
        weaponCollider.height = hitBoxHeight;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.ENEMY_LAYER)
        {
            Core.Audio.PlayFMODAudio("event:/Characters/Player/Combat/Weapons/Sword1_Hit1", transform);
            other.gameObject.GetComponent<EnemyAI>().GetDamage(weaponDamage);
        }
        
        // Descomentar para destruir las balas a espadazos
        /*if (other.gameObject.layer == Layers.BULLET_LAYER)
        {
            Core.Audio.PlayFMODAudio("event:/Characters/Player/Combat/Weapons/Sword1_Hit1", transform);
            Destroy(other.gameObject);
        }*/
    }

    public void DoAttack()
    {
        currentlyAttacking = true;
        weaponCollider.enabled = true;
        
        attackingSequence = DOTween.Sequence();
        attackingSequence
            .AppendInterval(0.2f)
            .AppendCallback(() =>
            {
                weaponCollider.enabled = false;
                currentlyAttacking = false;
            })
            .OnKill(() =>
            {
                weaponCollider.enabled = false;
                currentlyAttacking = false;
            });
    }

    public bool IsCurrentlyAttacking()
    {
        return currentlyAttacking;
    }

    public void UptadeWeaponStats(Item item)
    {
        weaponDamage = item.weaponDamage;
        hitBoxHeight = item.weaponHeight;
        hitBoxCenterY = item.weaponCenter;
        weaponCd = item.weaponCd;

        UpdateWeaponRange();
    }

    public void ResetValues()
    {
        weaponDamage = initialWeaponDamage;
        hitBoxHeight = initialWeaponRangeValue;
        weaponCd = initialWeaponCd;
    }
}
