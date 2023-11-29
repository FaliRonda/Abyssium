using UnityEngine;

[CreateAssetMenu(fileName = "New BT Chase Node", menuName = "AI/BT Nodes/Chase Node")]
public class BTChaseNode : BTNode
{
    public float visibilityRadius = 10f;
    public float lightRadius = 10f;
    
    public float chaseSpeed;
    public float chaseInLightSpeed;

    public bool isShadow;
    private bool currentlyChasing;

    public override BTNodeState Execute()
    {
        // Check if the player is within the visibility radius
        float distance = Vector3.Distance(enemyTransform.position, playerTransform.position);
        if (distance <= visibilityRadius)
        {
            if (!currentlyChasing)
            {
                detectedAudioSource.Play();
            }
            currentlyChasing = true;
            // Move towards the player
            Vector3 direction = playerTransform.position - enemyTransform.position;

            float movementSpeed = chaseSpeed;
            
            if (isShadow && distance <= lightRadius && playerTransform.GetComponent<PJ>().inventory.HasLantern)
            {
                movementSpeed = chaseInLightSpeed;
            }
            
            enemyTransform.Translate(direction.normalized * (Time.deltaTime * movementSpeed));
            enemySprite.flipX = direction.x > 0;
            enemyAnimator.Play("Stilt_walk");
            
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