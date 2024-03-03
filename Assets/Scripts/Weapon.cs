using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

public class Weapon : MonoBehaviour
{
    public int weaponDamage = 1;
    public float weaponHeight = 1.5f;
    public float weaponCenter = 0.25f;
    public float weaponCd = 0.1f;

    private float initialWeaponHeight;
    private float initialWeaponCenter;
    
    private CapsuleCollider weaponCollider;
    
    private bool currentlyAttacking = false;

    private int initialWeaponDamage;
    private float initialWeaponRangeValue;
    private float initialWeaponCd;
    [HideInInspector]
    public Sequence attackingSequence;

    private void Start()
    {
        weaponCollider = GetComponent<CapsuleCollider>();

        initialWeaponHeight = weaponHeight;
        initialWeaponCenter = weaponCenter;

        UpdateWeaponRange();
        
        weaponCollider.enabled = false;

        initialWeaponDamage = weaponDamage;
        initialWeaponRangeValue = weaponHeight;
        initialWeaponCd = weaponCd;
    }

    private void UpdateWeaponRange()
    {
        weaponCollider.height = weaponHeight;
        weaponCollider.center = new Vector3(0, weaponCenter, 0);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.ENEMY_LAYER)
        {
            Core.Audio.PlayFMODAudio("event:/Characters/Player/Combat/Weapons/Sword1_Hit1", transform);
            other.gameObject.GetComponent<EnemyAI>().GetDamage(weaponDamage);
        }
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
        weaponHeight = item.weaponHeight;
        weaponCenter = item.weaponCenter;
        weaponCd = item.weaponCd;

        UpdateWeaponRange();
    }

    public void ResetValues()
    {
        weaponDamage = initialWeaponDamage;
        weaponHeight = initialWeaponRangeValue;
        weaponCd = initialWeaponCd;
    }
}
