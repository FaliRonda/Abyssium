using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class WeaponDamage : MonoBehaviour
{
    public float attackCD = 0.5f;
    
    private SpriteRenderer weaponSprite;
    private BoxCollider weaponCollider;
    
    private bool currentlyAttacking = false;

    private void Start()
    {
        weaponSprite = GetComponent<SpriteRenderer>();
        weaponCollider = GetComponent<BoxCollider>();

        weaponSprite.enabled = false;
        weaponCollider.enabled = false;
    }
    
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.ENEMY_LAYER)
        {
            other.gameObject.GetComponent<EnemyAI>().GetDamage();
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
            .AppendInterval(attackCD)
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
}
