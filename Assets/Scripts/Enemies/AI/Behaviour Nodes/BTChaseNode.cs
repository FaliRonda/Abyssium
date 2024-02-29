using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BT Chase Node", menuName = "AI/BT Nodes/Chase Node")]
public class BTChaseNode : BTNode
{
    public bool isShadow;

    private Enemies.CODE_NAMES enemyCode;
    private bool currentlyChasing;
    private ChaseNodeParametersSO chaseNodeParameters;

    public BTChaseNode(Enemies.CODE_NAMES enemyCode)
    {
        this.enemyCode = enemyCode;
    }


    public override BTNodeState Execute()
    {
        if (enemyAI.waitForNextAttack)
        {
            return BTNodeState.Failure;
        }
        // Check if the player is within the visibility radius
        Vector3 distanceRootPosition =
            chasePivotTransform != null ? chasePivotTransform.position : enemyTransform.position;
        float distanceFromPivot = Vector3.Distance(distanceRootPosition, playerTransform.position);
        float distanceFromEnemy = Vector3.Distance(enemyTransform.position, playerTransform.position);
        if (distanceFromPivot <= chaseNodeParameters.visibilityRadius)
        {
            if (chaseNodeParameters.minimumDistanceToPlayer <= distanceFromEnemy)
            {
                if (!currentlyChasing)
                {
                    // Core.Audio.Play(SOUND_TYPE.EnemyChasing, 1, 0.1f, 0.01f);
                }
                currentlyChasing = true;
                // Move towards the player
                Vector3 direction = playerTransform.position - enemyTransform.position;

                float movementSpeed = chaseNodeParameters.chaseSpeed;
                
                if (isShadow && distanceFromEnemy <= chaseNodeParameters.lightRadius && playerTransform.GetComponent<PJ>().inventory.HasLantern)
                {
                    movementSpeed = chaseNodeParameters.chaseInLightSpeed;
                }
                
                enemyTransform.Translate(direction.normalized * (Time.deltaTime * movementSpeed));
                enemySprite.flipX = direction.x > 0;
                enemyAnimator.Play("Enemy_walk");
            }
            else
            {
                enemyAnimator.Play("Enemy_idle");
            }
            
            return BTNodeState.Running;
        }

        currentlyChasing = false;
        return BTNodeState.Failure;
    }
    
    public override void InitializeNode(Dictionary<string, object> parameters)
    {
        base.InitializeNode(parameters);
        AssignNodeParameters();
    }

    private void AssignNodeParameters()
    {
        chaseNodeParameters =
            Resources.Load<ChaseNodeParametersSO>(Enemies.EnemiesParametersPathDictionary(enemyCode, "Chase"));

        // Check if the ScriptableObject was loaded successfully.
        if (chaseNodeParameters == null)
        {
            Debug.LogError("EnemyParemeters not found in Resources folder.");
        }
    }

    public void ResetChasing()
    {
        currentlyChasing = false;
    }

    public override void ResetNode()
    {
        ResetChasing();
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(enemyTransform.position, chaseNodeParameters.visibilityRadius);
    }
}