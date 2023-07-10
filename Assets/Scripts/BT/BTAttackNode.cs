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

    public BTAttackNode(Transform enemyTransform, Transform playerTransform, float attackDistance)
    {
        this.enemyTransform = enemyTransform;
        this.playerTransform = playerTransform;
        this.attackDistance = attackDistance;
    }

    public override BTNodeState Execute()
    {
        // Check if the player is within attack distance
        float distance = Vector3.Distance(enemyTransform.position, playerTransform.position);
        if (distance <= attackDistance)
        {
            if (!waitForNextAttack)
            {
                // Perform the attack behavior
                Debug.Log("Attacking the player!");
                waitForNextAttack = true;
                
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(attackCD).
                    AppendCallback(() => waitForNextAttack = false);
                
            }
            return BTNodeState.Success;
        }

        return BTNodeState.Failure;
    }
}