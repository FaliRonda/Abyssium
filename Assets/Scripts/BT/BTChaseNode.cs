// Chase node
using UnityEngine;

public class BTChaseNode : BTNode
{
    private Transform enemyTransform;
    private Transform playerTransform;
    private float chaseSpeed;

    public BTChaseNode(Transform enemyTransform, Transform playerTransform, float chaseSpeed)
    {
        this.enemyTransform = enemyTransform;
        this.playerTransform = playerTransform;
        this.chaseSpeed = chaseSpeed;
    }

    public override BTNodeState Execute()
    {
        // Check if the player is within the visibility radius
        float distance = Vector3.Distance(enemyTransform.position, playerTransform.position);
        if (distance <= enemyTransform.GetComponent<EnemyAI>().visibilityRadius)
        {
            // Move towards the player
            Vector3 direction = playerTransform.position - enemyTransform.position;
            enemyTransform.Translate(direction.normalized * (Time.deltaTime * chaseSpeed));
            return BTNodeState.Running;
        }

        return BTNodeState.Failure;
    }
}