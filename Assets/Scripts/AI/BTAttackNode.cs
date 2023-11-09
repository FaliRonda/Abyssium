// Attack node

using DG.Tweening;
using UnityEngine;

public class BTAttackNode : BTNode
{
    public float attackCD = 1f;
    
    private Transform enemyTransform;
    private Transform playerTransform;
    private float attackDistance;
    private bool waitForNextAttack = false;
    private readonly Animator enemyAnimator;
    private readonly SpriteRenderer enemySprite;
    private bool attackPlaying = false;

    public BTAttackNode(Transform enemyTransform, Transform playerTransform, Animator enemyAnimator,
        SpriteRenderer enemySprite,
        float attackDistance)
    {
        this.enemyTransform = enemyTransform;
        this.playerTransform = playerTransform;
        this.enemyAnimator = enemyAnimator;
        this.enemySprite = enemySprite;
        this.attackDistance = attackDistance;
    }

    public override BTNodeState Execute()
    {
        // Check if the player is within attack distance
        Vector3 direction = playerTransform.position - enemyTransform.position;
        float distance = direction.magnitude;

        if (distance <= attackDistance)
        {
            if (!waitForNextAttack)
            {
                enemySprite.flipX = direction.x > 0;
                // Perform the attack behavior
                enemyAnimator.Play("Stilt_attack");
                attackPlaying = true;
                
                float animLenght = Core.AnimatorHelper.GetAnimLenght(enemyAnimator, "Stilt_attack_anim");
                Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () => { attackPlaying = false; });
                
                waitForNextAttack = true;
                
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(attackCD).
                    AppendCallback(() => waitForNextAttack = false);
                
            }
            else
            {
                if (!attackPlaying)
                {
                    enemyAnimator.Play("Stilt_idle");
                }
            } 
            return BTNodeState.Success;
        }
        
        if (attackPlaying)
        {
            return BTNodeState.Success;
        }

        return BTNodeState.Failure;
    }
}