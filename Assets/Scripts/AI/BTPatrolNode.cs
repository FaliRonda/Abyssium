using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BT Patrol Node", menuName = "AI/BT Nodes/Patrol Node")]
public class BTPatrolNode : BTNode
{
    private int currentWaypointIndex = 0;

    public override BTNodeState Execute()
    {
        // Move towards the current waypoint
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        Vector3 direction = targetPosition - enemyTransform.position;
        float distance = direction.magnitude;

        if (distance < 0.1f)
        {
            // Reached the waypoint, move to the next one
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            return BTNodeState.Success;
        }

        // Move towards the waypoint
        enemyTransform.Translate(direction.normalized * Time.deltaTime * patrolSpeed);
        enemySprite.flipX = direction.x > 0;
        enemyAnimator.Play("Stilt_walk");
        
        return BTNodeState.Running;
    }
}