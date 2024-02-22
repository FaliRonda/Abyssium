using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

[CreateAssetMenu(fileName = "New BT Attack Node", menuName = "AI/BT Nodes/Attack Node")]
public class BTAttackNode : BTNode
{
    [FormerlySerializedAs("attackDistance")] public float attackVisibilityDistance;
    public float standAfterAttackCD = 1;
    public float anticipationDistance = 0.5f;
    public float anticipacionDuration = 0.3f;
    public float attackMovementDistance = 2f;
    public float attackMovementDuration = 0.3f;
    public float enemyRayMaxDistance = .75f;
    
    private bool attackPlaying;
    private Ray ray;
    private RaycastHit[] hits;
    private Sequence attackSequence;
    private Vector3 lastPlayerDirectionBeforeAttack;

    public override BTNodeState Execute()
    {
        ray = new Ray(enemyTransform.position, lastPlayerDirectionBeforeAttack);
        EnemyRaycastHit(Color.green);
        
        if (attackPlaying)
        {
            if (EnemyRayHitLayer(Layers.WALL_LAYER) || EnemyRayHitLayer(Layers.DOOR_LAYER) || EnemyRayHitLayer(Layers.PJ_LAYER))
            {
                attackSequence.Kill();
            }
            
            return BTNodeState.Success;
        }
        
        // Check if the player is within attack distance
        Vector3 playerDirection = playerTransform.position - enemyTransform.position;
        float playerDistance = playerDirection.magnitude;
        
        ray = new Ray(enemyTransform.position, lastPlayerDirectionBeforeAttack);
        EnemyRaycastHit(Color.green);
        
        if (playerDistance <= attackVisibilityDistance)
        {
            if (!attackPlaying)
            {
                lastPlayerDirectionBeforeAttack = playerDirection;
                Attack(playerDirection);
            }

            return BTNodeState.Success;
        }
        
        return BTNodeState.Failure;
    }

    private void Attack(Vector3 direction)
    {
        attackPlaying = true;
        
        // Animation
        enemySprite.flipX = direction.x > 0;
        enemyAnimator.Play("Enemy_attack");

        // Sequence
        attackSequence = DOTween.Sequence();
        
        Vector3 targetPosition = playerTransform.position;
        Vector3 enemyPosition = enemyTransform.position;

        Vector3 startPosition = enemyPosition;
        Vector3 anticipationDirection = (startPosition - targetPosition).normalized * anticipationDistance;

        attackSequence.Append(enemyTransform.DOMove(enemyPosition + anticipationDirection, anticipacionDuration));
        
        Vector3 attackDirection = (targetPosition - startPosition).normalized * attackMovementDistance;
        
        attackSequence.Append(enemyTransform.DOMove(enemyPosition + attackDirection, attackMovementDuration));
        attackSequence.AppendCallback(AttackEndCD);
        attackSequence.OnKill(() =>
        {
            AttackEndCD();
        });
    }

    private void AttackEndCD()
    {
        var attackingCooldownSequence = DOTween.Sequence();
        attackingCooldownSequence.AppendInterval(standAfterAttackCD);
        attackingCooldownSequence.AppendCallback(() =>
        {
            attackPlaying = false;
        });
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
    
    public override void InitializeNode(Dictionary<string, object> parameters)
    {
        base.InitializeNode(parameters);
        attackPlaying = false;
    }

    public override void ResetNode()
    {
        attackSequence.Kill();
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, attackVisibilityDistance);
    }
}