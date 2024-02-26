using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BT Patrol Node", menuName = "AI/BT Nodes/Patrol Node")]
public class BTPatrolNode : BTNode
{
    public Enemies.CODE_NAMES enemyCode;
    
    private float patrolSpeed;
    private int currentWaypointIndex = 0;
    private PatrolNodeParametersSO patrolNodeParameters;

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
        enemyTransform.Translate(direction.normalized * (Time.deltaTime * patrolSpeed));
        enemySprite.flipX = direction.x > 0;
        enemyAnimator.Play("Enemy_walk");
        
        return BTNodeState.Running;
    }
    
    public override void InitializeNode(Dictionary<string, object> parameters)
    {
        base.InitializeNode(parameters);
        AssignNodeParameters();
    }
    
    private void AssignNodeParameters()
    {
        patrolNodeParameters =
            Resources.Load<PatrolNodeParametersSO>(Enemies.EnemiesParametersPathDictionary(enemyCode, "Patrol"));

        // Check if the ScriptableObject was loaded successfully.
        if (patrolNodeParameters == null)
        {
            Debug.LogError("EnemyParemeters not found in Resources folder.");
        }

        patrolSpeed = patrolNodeParameters.patrolSpeed;
    }

    public override void ResetNode()
    {
        currentWaypointIndex = 0;
    }
}