using UnityEngine;

[CreateAssetMenu(fileName = "New BT Chase Node", menuName = "AI/BT Nodes/Chase Node")]
public class BTChaseNode : BTNode
{
    public float visibilityRadius = 10f;
    public float minimumDistanceToPlayer;
    public float lightRadius = 10f;
    
    public float chaseSpeed;
    public float chaseInLightSpeed;

    public bool isShadow;
    private bool currentlyChasing;

    public override BTNodeState Execute()
    {
        // Check if the player is within the visibility radius
        Vector3 distanceRootPosition =
            chasePivotTransform != null ? chasePivotTransform.position : enemyTransform.position;
        float distanceFromPivot = Vector3.Distance(distanceRootPosition, playerTransform.position);
        float distanceFromEnemy = Vector3.Distance(enemyTransform.position, playerTransform.position);
        if (distanceFromPivot <= visibilityRadius)
        {
            if (minimumDistanceToPlayer <= distanceFromEnemy)
            {
                if (!currentlyChasing)
                {
                    Core.Audio.Play(SOUND_TYPE.EnemyChasing, 1, 0.1f, 0.01f);
                }
                currentlyChasing = true;
                // Move towards the player
                Vector3 direction = playerTransform.position - enemyTransform.position;

                float movementSpeed = chaseSpeed;
                
                if (isShadow && distanceFromEnemy <= lightRadius && playerTransform.GetComponent<PJ>().inventory.HasLantern)
                {
                    movementSpeed = chaseInLightSpeed;
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
        Gizmos.DrawWireSphere(enemyTransform.position, visibilityRadius);
    }
}