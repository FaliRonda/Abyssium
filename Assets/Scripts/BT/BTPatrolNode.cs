// Patrol node
using UnityEngine;

public class BTPatrolNode : BTNode
{
    private Transform enemyTransform;
    private Transform[] waypoints;
    private int currentWaypointIndex;
    private float patrolSpeed;

    public BTPatrolNode( Transform enemyTransform, Transform[] waypoints, float patrolSpeed)
    {
        this.enemyTransform = enemyTransform;
        this.waypoints = waypoints;
        this.patrolSpeed = patrolSpeed;
        currentWaypointIndex = 0;
    }

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
        return BTNodeState.Running;
    }
}