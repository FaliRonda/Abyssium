// Base class for behavior tree nodes

using System.Collections.Generic;
using UnityEngine;

public abstract class BTNode : ScriptableObject
{
    [HideInInspector]
    public Transform enemyTransform;
    [HideInInspector]
    public Transform playerTransform;
    [HideInInspector]
    public Transform chasePivotTransform;
    [HideInInspector]
    public Transform[] waypoints;
    [HideInInspector]
    public Animator enemyAnimator;
    [HideInInspector]
    public SpriteRenderer enemySprite;
    
    public abstract BTNodeState Execute();
    public virtual void ResetNode(){}
    public virtual void DrawGizmos(){}
    
    public virtual void InitializeNode(Dictionary<string, object> parameters)
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
    }
}

// Enum to represent the state of a behavior tree node
public enum BTNodeState
{
    Success,
    Failure,
    Running
}