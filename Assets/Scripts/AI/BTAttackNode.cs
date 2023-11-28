using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

[CreateAssetMenu(fileName = "New BT Attack Node", menuName = "AI/BT Nodes/Attack Node")]
public class BTAttackNode : BTNode
{
    public float attackCD = 1f;
    private bool waitForNextAttack = false;
    private bool attackPlaying = false;
    private bool isInitialized;

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
                
                float animLenght = Core.AnimatorHelper.GetAnimLenght(enemyAnimator, "Enemigo1_Attack");
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