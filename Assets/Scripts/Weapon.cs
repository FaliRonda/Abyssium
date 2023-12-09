using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

public class Weapon : MonoBehaviour
{
    public int weaponDamage = 1;
    public float weaponHeight = 1.5f;
    public float weaponCenter = 0.25f;
    public float weaponCd = 0.5f;

    private float initialWeaponHeight;
    private float initialWeaponCenter;
    
    private SpriteRenderer weaponSprite;
    private CapsuleCollider weaponCollider;
    
    private bool currentlyAttacking = false;

    private int initialWeaponDamage;
    private float initialWeaponRangeValue;
    private float initialWeaponCd;

    private void Start()
    {
        weaponSprite = GetComponent<SpriteRenderer>();
        weaponCollider = GetComponent<CapsuleCollider>();

        initialWeaponHeight = weaponHeight;
        initialWeaponCenter = weaponCenter;

        UpdateWeaponRange();
        
        weaponSprite.enabled = false;
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
            other.gameObject.GetComponent<EnemyAI>().GetDamage(weaponDamage);
        }
    }

    public void DoAttack()
    {
        currentlyAttacking = true;
        
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
            {
                weaponSprite.enabled = true;
                weaponCollider.enabled = true;
            })
            .AppendInterval(weaponCd)
            .AppendCallback(() =>
            {
                weaponSprite.enabled = false;
                weaponCollider.enabled = false;
            })
            .AppendCallback(() => currentlyAttacking = false);
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
