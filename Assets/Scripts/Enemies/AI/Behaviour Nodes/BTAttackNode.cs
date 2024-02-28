using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

[CreateAssetMenu(fileName = "New BT Attack Node", menuName = "AI/BT Nodes/Attack Node")]
public class BTAttackNode : BTNode
{
    private Enemies.CODE_NAMES enemyCode;
    private bool attackPlaying;
    private Ray ray;
    private RaycastHit[] hits;
    private Sequence attackSequence;
    private Vector3 lastPlayerDirectionBeforeAttack;
    private AttackNodeParametersSO attackNodeParameters;

    public BTAttackNode(Enemies.CODE_NAMES enemyCode)
    {
        this.enemyCode = enemyCode;
    }

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
        
        if (playerDistance <= attackNodeParameters.attackVisibilityDistance)
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

        // Sequence
        attackSequence = DOTween.Sequence();
        
        Vector3 targetPosition = playerTransform.position;
        Vector3 enemyPosition = enemyTransform.position;

        Vector3 startPosition = enemyPosition;
        Vector3 anticipationDirection = (startPosition - targetPosition).normalized * attackNodeParameters.anticipationDistance;

        attackSequence.Append(enemyTransform.DOMove(enemyPosition + anticipationDirection, attackNodeParameters.anticipacionDuration));
        
        Vector3 attackDirection = (targetPosition - startPosition).normalized * attackNodeParameters.attackMovementDistance;

        attackSequence.AppendCallback(() => enemyAnimator.Play("Enemy_attack"));
        
        attackSequence.Append(enemyTransform.DOMove(enemyPosition + attackDirection, attackNodeParameters.attackMovementDuration));
        attackSequence.AppendCallback(AttackEndCD);
        attackSequence.OnKill(() =>
        {
            AttackEndCD();
        });
    }

    private void AttackEndCD()
    {
        var attackingCooldownSequence = DOTween.Sequence();
        attackingCooldownSequence.AppendInterval(attackNodeParameters.standAfterAttackCD);
        attackingCooldownSequence.AppendCallback(() =>
        {
            attackPlaying = false;
        });
    }

    private void EnemyRaycastHit(Color color)
    {
        Debug.DrawRay(ray.origin, ray.direction, color);
        hits = Physics.RaycastAll(ray.origin, ray.direction, attackNodeParameters.enemyRayMaxDistance);
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

        AssignNodeParameters();
    }

    private void AssignNodeParameters()
    {
        attackNodeParameters =
            Resources.Load<AttackNodeParametersSO>(Enemies.EnemiesParametersPathDictionary(enemyCode, "Attack"));

        // Check if the ScriptableObject was loaded successfully.
        if (attackNodeParameters == null)
        {
            Debug.LogError("EnemyParemeters not found in Resources folder.");
        }
    }

    public override void ResetNode()
    {
        attackSequence.Kill();
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, attackNodeParameters.attackVisibilityDistance);
    }
}