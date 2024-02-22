using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

[CreateAssetMenu(fileName = "New BT Attack Node", menuName = "AI/BT Nodes/Attack Node")]
public class BTAttackNode : BTNode
{
    [FormerlySerializedAs("attackDistance")] public float attackVisibilityDistance;
    public float attackCD = 1f;
    public float standAfterAttackCD = 0;
    public float anticipationDistance = 0.5f;
    public float anticipacionDuration = 0.3f;
    public float attackMovementDistance = 2f;
    public float attackMovementDuration = 0.3f;
    public float enemyRayMaxDistance = .75f;
    
    private bool standAfterAttack;
    private bool waitForNextAttack;
    private bool attackPlaying;
    private Ray ray;
    private RaycastHit[] hits;
    private Sequence attackSequence;
    private Vector3 lastDirectionBeforeAttack;

    public override BTNodeState Execute()
    {
        if (standAfterAttack)
        {
            return BTNodeState.Success;
        }
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
                    if (EnemyRayHitLayer(Layers.WALL_LAYER) || EnemyRayHitLayer(Layers.DOOR_LAYER) || EnemyRayHitLayer(Layers.PJ_LAYER))
                    {
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
        float animLength = Core.AnimatorHelper.GetAnimLength(enemyAnimator, "Enemy_attack");
        //Core.AnimatorHelper.DoOnAnimationFinish(animLength, () => { attackPlaying = false; });

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

        attackSequence.AppendCallback(StandAfterAttack);
        
        attackSequence.OnKill(() =>
        {
            var attackingCooldownSequence = DOTween.Sequence();
            attackingCooldownSequence.AppendInterval(1f);
            attackingCooldownSequence.AppendCallback(() =>
            {
                attackPlaying = false;
            });
        });
        
        attackSequence.Play();
    }

    private void StandAfterAttack()
    {
        standAfterAttack = true;
        Sequence standAfterAttackCDSequence = DOTween.Sequence();
        standAfterAttackCDSequence.AppendInterval(standAfterAttackCD).AppendCallback(() => { standAfterAttack = false; });
    }

    private void EnemyRaycastHit(Color color)
    {
        Debug.DrawRay(ray.origin, ray.direction, color);
        hits = Physics.RaycastAll(ray.origin, ray.direction, enemyRayMaxDistance);
    }
    
    private bool EnemyRayHitLayer(int layer)
    {
        bool layerHit = false;
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.layer == layer)
            {
                layerHit = true;
            }
        }

        return layerHit;
    }

    public override void ResetNode()
    {
        attackSequence.Kill();
        
        var attackingCooldownSequence = DOTween.Sequence();
        attackingCooldownSequence.AppendInterval(1f);
        attackingCooldownSequence.AppendCallback(() =>
        {
            standAfterAttack = false;
            waitForNextAttack = false;
            attackPlaying = false;
        });
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, attackVisibilityDistance);
    }
}