// Patrol node
using UnityEngine;

public class BTPatrolNode : BTNode
{
    private Transform enemyTransform;
    private Transform[] waypoints;
    private readonly Animator enemyAnimator;
    private readonly SpriteRenderer enemySprite;
    private int currentWaypointIndex;
    private float patrolSpeed;

    public BTPatrolNode(Transform enemyTransform, Transform[] waypoints, Animator enemyAnimator,
        SpriteRenderer enemySprite, float patrolSpeed)
    {
        this.enemyTransform = enemyTransform;
        this.waypoints = waypoints;
        this.enemyAnimator = enemyAnimator;
        this.enemySprite = enemySprite;
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
        enemySprite.flipX = direction.x > 0;
        enemyAnimator.Play("Stilt_walk");
        
        return BTNodeState.Running;
    }
}