using System.Collections.Generic;
using UnityEngine;

public class PatrolBehaviour
{
    // General parameters
    public Transform enemyTransform;
    public Transform playerTransform;
    public Transform chasePivotTransform;
    public Transform[] waypoints;
    public Animator enemyAnimator;
    public SpriteRenderer enemySprite;
    
    // Patrol parameters
    private int currentWaypointIndex = 0;
    private PatrolNodeParametersSO patrolNodeParameters;
    
    public void InitializeBehaviour(Dictionary<string, object> parameters, Enemies.CODE_NAMES enemyCode)
    {
        object parameterObj;
        parameters.TryGetValue("EnemyTransform", out parameterObj);
        enemyTransform = parameterObj as Transform;
        parameters.TryGetValue("PlayerTransform", out parameterObj);
        playerTransform = parameterObj as Transform;
        parameters.TryGetValue("ChasePivotTransform", out parameterObj);
        chasePivotTransform = parameterObj as Transform;
        parameters.TryGetValue("Waypoints", out parameterObj);
        waypoints = parameterObj as Transform[];
        parameters.TryGetValue("EnemyAnimator", out parameterObj);
        enemyAnimator = parameterObj as Animator;
        parameters.TryGetValue("EnemySprite", out parameterObj);
        enemySprite = parameterObj as SpriteRenderer;
        
        patrolNodeParameters =
            Resources.Load<PatrolNodeParametersSO>(Enemies.EnemiesParametersPathDictionary(enemyCode, "Patrol"));

        // Check if the ScriptableObject was loaded successfully.
        if (patrolNodeParameters == null)
        {
            Debug.LogError("EnemyParemeters not found in Resources folder.");
        }
    }
    
    public void ResetBehaviour()
    {
        currentWaypointIndex = 0;
    }
    
    public BTNodeState ExecuteBehaviour()
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
        enemyTransform.Translate(direction.normalized * (Time.deltaTime * patrolNodeParameters.patrolSpeed));
        enemySprite.flipX = direction.x > 0;
        enemyAnimator.Play("Enemy_walk");
        
        return BTNodeState.Running;
    }
}
