using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

[CreateAssetMenu(fileName = "New BT Attack Node", menuName = "AI/BT Nodes/Attack Node")]
public class BTAttackNode : BTNode
{
    [FormerlySerializedAs("attackDistance")] public float attackVisibilityDistance;
    public float attackCD = 1f;
    public float anticipationDistance = 0.5f;
    public float anticipacionDuration = 0.3f;
    public float attackMovementDistance = 2f;
    public float attackMovementDuration = 0.3f;
    
    private bool waitForNextAttack;
    private bool attackPlaying;

    public override BTNodeState Execute()
    {
        // Check if the player is within attack distance
        Vector3 direction = playerTransform.position - enemyTransform.position;
        float distance = direction.magnitude;

        if (distance <= attackVisibilityDistance)
        {
            if (!waitForNextAttack)
            {
                Attack(direction);
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

    private void Attack(Vector3 direction)
    {
        enemySprite.flipX = direction.x > 0;

        enemyAnimator.Play("Stilt_attack");
        attackPlaying = true;

        float animLenght = Core.AnimatorHelper.GetAnimLenght(enemyAnimator, "Stalker_attack");
        Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () => { attackPlaying = false; });

        waitForNextAttack = true;

        Sequence attackCDSequence = DOTween.Sequence();
        attackCDSequence.AppendInterval(attackCD).AppendCallback(() => { waitForNextAttack = false; });
        
        Sequence attackSequence = DOTween.Sequence();

        float duration = 1f;

        Vector3 targetPosition = playerTransform.position;
        Vector3 enemyPosition = enemyTransform.position;

        Vector3 startPosition = enemyPosition;
        Vector3 anticipationDirection = (startPosition - targetPosition).normalized * anticipationDistance;

        attackSequence.Append(enemyTransform.DOMove(enemyPosition + anticipationDirection, anticipacionDuration));
        
        Vector3 attackDirection = (targetPosition - startPosition).normalized * attackMovementDistance;
        
        attackSequence.Append(enemyTransform.DOMove(enemyPosition + attackDirection, attackMovementDuration));

        // Reproducir la secuencia
        attackSequence.Play();
    }

    public override void ResetNode()
    {
        waitForNextAttack = false;
        attackPlaying = false;
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, attackVisibilityDistance);
    }
}