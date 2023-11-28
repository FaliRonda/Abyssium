// Base class for behavior tree nodes

using System.Collections.Generic;
using UnityEngine;

public abstract class BTNode : ScriptableObject
{
    public Transform enemyTransform;
    public Transform playerTransform;
    public Transform[] waypoints;
    public Animator enemyAnimator;
    public SpriteRenderer enemySprite;
    public AudioSource detectedAudioSource;
    public float attackDistance;
    public float patrolSpeed;
    public float chaseSpeed;
    public float chaseInLightSpeed;
    public bool isShadow;
    
    public abstract BTNodeState Execute();
    public virtual void ResetNode(){}
    
    public virtual void InitializeNode(Dictionary<string, object> parameters)
    {
        object parameterObj;
        parameters.TryGetValue("EnemyTransform", out parameterObj);
        enemyTransform = parameterObj as Transform;
        parameters.TryGetValue("PlayerTransform", out parameterObj);
        playerTransform = parameterObj as Transform;
        parameters.TryGetValue("Waypoints", out parameterObj);
        waypoints = parameterObj as Transform[];
        parameters.TryGetValue("EnemyAnimator", out parameterObj);
        enemyAnimator = parameterObj as Animator;
        parameters.TryGetValue("EnemySprite", out parameterObj);
        enemySprite = parameterObj as SpriteRenderer;
        parameters.TryGetValue("DetectedAudioSource", out parameterObj);
        detectedAudioSource = parameterObj as AudioSource;
        parameters.TryGetValue("AttackDistance", out parameterObj);
        attackDistance = (float) parameterObj;
        parameters.TryGetValue("PatrolSpeed", out parameterObj);
        patrolSpeed = (float) parameterObj;
        parameters.TryGetValue("ChaseSpeed", out parameterObj);
        chaseSpeed = (float) parameterObj;
        parameters.TryGetValue("ChaseInLightSpeed", out parameterObj);
        chaseInLightSpeed = (float) parameterObj;
        parameters.TryGetValue("IsShadow", out parameterObj);
        isShadow = (bool) parameterObj;
    }
}

// Enum to represent the state of a behavior tree node
public enum BTNodeState
{
    Success,
    Failure,
    Running
}