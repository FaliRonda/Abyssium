// Attack node
using UnityEngine;

public class BTAttackNode : BTNode
{
    private Transform enemyTransform;
    private Transform playerTransform;
    private float attackDistance;

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
            // Perform the attack behavior
            Debug.Log("Attacking the player!");
            return BTNodeState.Success;
        }

        return BTNodeState.Failure;
    }
}