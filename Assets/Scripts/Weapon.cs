using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class Weapon : MonoBehaviour
{
    public int weaponDamage = 1;
    public float weaponRange = 1f;
    public float weaponCd = 0.5f;

    private float initialWeaponRange;
    private Vector3 initialWeaponPivot;
    
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

        initialWeaponRange = weaponCollider.height;
        initialWeaponPivot = weaponCollider.center;
        
        UpdateWeaponRange();
        
        weaponSprite.enabled = false;
        weaponCollider.enabled = false;

        initialWeaponDamage = weaponDamage;
        initialWeaponRangeValue = weaponRange;
        initialWeaponCd = weaponCd;
    }

    private void UpdateWeaponRange()
    {
        weaponCollider.height = weaponRange;
        weaponCollider.center = new Vector3(initialWeaponPivot.x, weaponRange / 2, initialWeaponPivot.z);
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
        weaponRange = item.weaponRange;
        weaponCd = item.weaponCd;

        UpdateWeaponRange();
    }

    public void ResetValues()
    {
        weaponDamage = initialWeaponDamage;
        weaponRange = initialWeaponRangeValue;
        weaponCd = initialWeaponCd;
    }
}
