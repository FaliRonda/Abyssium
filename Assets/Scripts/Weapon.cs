using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class Weapon : MonoBehaviour
{
    public int weaponDamage = 1;
    public float weaponRange = 1f;
    public float weaponCd = 0.5f;

    private Vector3 initialWeaponRange;
    private Vector3 initialWeaponPivot;
    
    private SpriteRenderer weaponSprite;
    private BoxCollider weaponCollider;
    
    private bool currentlyAttacking = false;

    private void Start()
    {
        weaponSprite = GetComponent<SpriteRenderer>();
        weaponCollider = GetComponent<BoxCollider>();

        initialWeaponRange = weaponCollider.size;
        initialWeaponPivot = weaponCollider.center;
        
        UpdateWeaponRange();
        
        weaponSprite.enabled = false;
        weaponCollider.enabled = false;
    }

    private void UpdateWeaponRange()
    {
        weaponCollider.size = new Vector3(initialWeaponRange.x, weaponRange, initialWeaponRange.z);
        weaponCollider.center = new Vector3(initialWeaponPivot.x, weaponRange / 2, initialWeaponPivot.z);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.ENEMY_LAYER)
        {
            other.gameObject.GetComponent<EnemyAI>().GetDamage(weaponDamage);
        }
    }

    public void Attack()
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
}
