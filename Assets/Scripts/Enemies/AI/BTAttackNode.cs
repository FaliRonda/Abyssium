using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

[CreateAssetMenu(fileName = "New BT Attack Node", menuName = "AI/BT Nodes/Attack Node")]
public class BTAttackNode : BTNode
{
    [FormerlySerializedAs("attackDistance")] public float attackVisibilityDistance;
    public float attackCD = 1f;
    public float anticipationDistance = 0.5f;
    public float anticipacionDuration = 0.3f;
    public float attackMovementDistance = 2f;
    public float attackMovementDuration = 0.3f;
    public float enemyRayMaxDistance = .75f;
    
    private bool waitForNextAttack;
    private bool attackPlaying;
    private Ray ray;
    private RaycastHit hit;
    private Sequence attackSequence;
    private Vector3 lastDirectionBeforeAttack;

    public override BTNodeState Execute()
    {
        // Check if the player is within attack distance
        Vector3 direction = playerTransform.position - enemyTransform.position;
        float distance = direction.magnitude;
        
        ray = new Ray(enemyTransform.position, lastDirectionBeforeAttack);
        EnemyRaycastHit(Color.green);

        if (distance <= attackVisibilityDistance)
        {
            if (!waitForNextAttack)
            {
                lastDirectionBeforeAttack = direction;
                Attack(direction);
            }
            else
            {
                if (!attackPlaying)
                {
                    return BTNodeState.Failure;
                }
                else
                {
                    if (EnemyRayHitLayer(Layers.WALL_LAYER) || EnemyRayHitLayer(Layers.DOOR_LAYER))
                    {
                        attackPlaying = false;
                        attackSequence.Kill();
                    }
                }
            } 
            return BTNodeState.Success;
        }
        
        if (attackPlaying)
        {
            return BTNodeState.Success;
        }

        return BTNodeState.Failure;
    }

    private void Attack(Vector3 direction)
    {
        enemySprite.flipX = direction.x > 0;

        // Animation
        enemyAnimator.Play("Enemy_attack");
        attackPlaying = true;
        float animLenght = Core.AnimatorHelper.GetAnimLenght(enemyAnimator, "Enemy_attack");
        Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () => { attackPlaying = false; });

        // CD
        waitForNextAttack = true;
        Sequence attackCDSequence = DOTween.Sequence();
        attackCDSequence.AppendInterval(attackCD).AppendCallback(() => { waitForNextAttack = false; });
        
        // Attack
        attackSequence = DOTween.Sequence();
        
        Vector3 targetPosition = playerTransform.position;
        Vector3 enemyPosition = enemyTransform.position;

        Vector3 startPosition = enemyPosition;
        Vector3 anticipationDirection = (startPosition - targetPosition).normalized * anticipationDistance;

        attackSequence.Append(enemyTransform.DOMove(enemyPosition + anticipationDirection, anticipacionDuration));
        
        Vector3 attackDirection = (targetPosition - startPosition).normalized * attackMovementDistance;
        
        attackSequence.Append(enemyTransform.DOMove(enemyPosition + attackDirection, attackMovementDuration));
        attackSequence.Play();
    }

    private bool EnemyRaycastHit(Color color)
    {
        Debug.DrawRay(ray.origin, ray.direction, color);
        return Physics.Raycast(ray, out hit, enemyRayMaxDistance);
    }
    
    private bool EnemyRayHitLayer(int layer)
    {
        bool hitLayer = false;
        
        if (hit.transform != null)
        {
            hitLayer = hit.transform.gameObject.layer == layer;
        }

        return hitLayer;
    }
    
    
    public override void ResetNode()
    {
        waitForNextAttack = false;
        attackPlaying = false;
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, attackVisibilityDistance);
    }
}