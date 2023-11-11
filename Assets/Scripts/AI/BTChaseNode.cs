// Chase node
using UnityEngine;

public class BTChaseNode : BTNode
{
    private Transform enemyTransform;
    private Transform playerTransform;
    private readonly Animator enemyAnimator;
    private readonly SpriteRenderer enemySprite;
    private float chaseSpeed;
    private float chaseInLightSpeed;
    private bool isShadow;
    private AudioSource detectedAudio;
    private bool currentlyChasing = false;

    public BTChaseNode(Transform enemyTransform, Transform playerTransform, Animator enemyAnimator,
        SpriteRenderer enemySprite, float chaseSpeed, float chaseInLightSpeed, bool isShadow, AudioSource detectedAudio)
    {
        this.enemyTransform = enemyTransform;
        this.playerTransform = playerTransform;
        this.enemyAnimator = enemyAnimator;
        this.enemySprite = enemySprite;
        this.chaseSpeed = chaseSpeed;
        this.chaseInLightSpeed = chaseInLightSpeed;
        this.isShadow = isShadow;
        this.detectedAudio = detectedAudio;
    }

    public override BTNodeState Execute()
    {
        // Check if the player is within the visibility radius
        float distance = Vector3.Distance(enemyTransform.position, playerTransform.position);
        if (distance <= enemyTransform.GetComponent<EnemyAI>().visibilityRadius)
        {
            if (!currentlyChasing)
            {
                detectedAudio.Play();
            }
            currentlyChasing = true;
            // Move towards the player
            Vector3 direction = playerTransform.position - enemyTransform.position;

            float movementSpeed = chaseSpeed;
            
            if (isShadow && distance <= enemyTransform.GetComponent<EnemyAI>().lightRadius && playerTransform.GetComponent<PJ>().inventory.HasLantern)
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
}