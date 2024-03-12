using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class BTAttackNode : BTNode
{
    private Enemies.CODE_NAMES enemyCode;
    private bool attackPlaying;
    private Ray ray;
    private RaycastHit[] hits;
    private Sequence attackSequence;
    private Vector3 lastPlayerDirectionBeforeAttack;
    private AttackNodeParametersSO attackNodeParameters;
    private MaterialPropertyBlock propertyBlock;
    private Sequence standAfterAttackSequence;
    private Sequence waitForNextAttackSequence;
    private bool waitAfterAttackFinished;

    public BTAttackNode(Enemies.CODE_NAMES enemyCode)
    {
        this.enemyCode = enemyCode;
    }

    public override BTNodeState Execute()
    {
        ray = new Ray(enemyTransform.position, lastPlayerDirectionBeforeAttack);
        EnemyRaycastHit(Color.green);

        if (waitAfterAttackFinished)
        {
            waitAfterAttackFinished = false;
            attackPlaying = false;
            return BTNodeState.NextTree;
        }
        
        if (attackPlaying)
        {
            if (EnemyRayHitLayer(Layers.WALL_LAYER) || EnemyRayHitLayer(Layers.DOOR_LAYER) || EnemyRayHitLayer(Layers.PJ_LAYER))
            {
                attackSequence.Kill();
            }
            
            return BTNodeState.Running;
        }

        if (!waitAfterAttackFinished)
        {
            Vector3 playerDirection = playerTransform.position - enemyTransform.position;
            float playerDistance = playerDirection.magnitude;
            
            ray = new Ray(enemyTransform.position, lastPlayerDirectionBeforeAttack);
            EnemyRaycastHit(Color.green);
            
            if (playerDistance <= attackNodeParameters.attackVisibilityDistance)
            {
                if (!attackPlaying && !enemyAI.attackInCD && (!enemyAI.enemyStunned || !attackNodeParameters.stoppedByStun))
                {
                    lastPlayerDirectionBeforeAttack = playerDirection;
                    Attack(playerDirection);

                    WaitForNextAttack();
                    return BTNodeState.Running;
                }
            }
        }
        
        return BTNodeState.Failure;
    }

    private void WaitForNextAttack()
    {
        enemyAI.attackInCD = true;

        waitForNextAttackSequence = DOTween.Sequence();
        waitForNextAttackSequence
            .AppendInterval(attackNodeParameters.waitForNextAttackCD)
            .AppendCallback(() => { enemyAI.attackInCD = false; });
    }

    private void Attack(Vector3 direction)
    {
        attackPlaying = true;
        
        // Animation
        enemySprite.flipX = direction.x > 0;

        // Sequence
        attackSequence = DOTween.Sequence();
        
        Sequence attackSoundSequence = DOTween.Sequence();
        attackSoundSequence
            .AppendInterval(0.3f)
            .AppendCallback(() =>
            {
                Core.Audio.PlayFMODAudio("event:/Characters/Enemies/Stalker/JumpToAttack", enemyTransform);
            });

        if (!attackNodeParameters.jumpAttack)
        {
            DoLinealAttack();
        }
        else
        {
            DoJumpAttack();
        }
    }

    private void DoLinealAttack()
    {
        Vector3 enemyPosition = enemyTransform.position;
        Vector3 targetPosition = playerTransform.position;
        
        Vector3 anticipationDirection = (enemyPosition - targetPosition).normalized * attackNodeParameters.anticipationDistance;
        float whiteHitTargetValue = 1 - attackNodeParameters.whiteHitPercentage;
        
        attackSequence
            .AppendCallback(() => { enemyAnimator.Play("Enemy_attack"); })
            .Append(enemyTransform.DOMove(enemyPosition + anticipationDirection, attackNodeParameters.anticipacionDuration))
            .Join(DOTween.To(() => 1, x =>
            {
                whiteHitTargetValue = x;
                UpdateWhiteHitValue(x);
            }, whiteHitTargetValue, attackNodeParameters.anticipacionDuration));

        Vector3 attackDirection = (targetPosition - enemyPosition).normalized * attackNodeParameters.attackMovementDistance;

        attackSequence
            .AppendCallback(() =>
            {
                UpdateWhiteHitValue(1);
                enemyAI.attackCollider.isTrigger = false;
            })
            .Append(enemyTransform.DOMove(enemyPosition + attackDirection, attackNodeParameters.attackMovementDuration))
            .AppendCallback(StandAfterAttack)
            .AppendInterval(0.1f)
            .AppendCallback(() => { enemyAI.attackCollider.isTrigger = true; })
            .OnKill(() =>
            {
                UpdateWhiteHitValue(1);
                StandAfterAttack();

                Sequence waitAndDisableColliderSequence = DOTween.Sequence();
                waitAndDisableColliderSequence
                    .AppendInterval(0.1f)
                    .AppendCallback(() => { enemyAI.attackCollider.isTrigger = true; });
            });
    }
    
    private void DoJumpAttack()
    {
        Vector3 targetPosition = playerTransform.position;
        var position = enemyTransform.position;

        attackSequence
            .AppendCallback(() =>
            {
                enemyAI.attackCollider.isTrigger = false;
            })
            .Append(enemyTransform.DOJump(targetPosition, attackNodeParameters.jumpHeight, 1, attackNodeParameters.jumpDuration))
            .SetEase(Ease.Linear)
            .AppendCallback(() =>
            {
                ShowJumpParticles();
            })
            .AppendCallback(StandAfterAttack)
            .AppendInterval(0.2f)
            .AppendCallback(() => { enemyAI.attackCollider.isTrigger = true; })
            .OnKill(() =>
            {
                attackPlaying = false;
                StandAfterAttack();

                Sequence waitAndDisableColliderSequence = DOTween.Sequence();
                waitAndDisableColliderSequence
                    .AppendInterval(0.1f)
                    .AppendCallback(() => { enemyAI.attackCollider.isTrigger = true; });
            });
    }

    private void ShowJumpParticles()
    {
        ParticleSystem jumpParticles = enemyTransform.GetComponentsInChildren<ParticleSystem>()[1];
        jumpParticles.Play();
        jumpParticles.transform.DOScale(new Vector3(5, 5, 5), 1)
            .OnComplete(() =>
            {
                jumpParticles.transform.localScale = new Vector3(1, 1, 1);
            });
    }

    private void UpdateWhiteHitValue(float value)
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
        
        Renderer renderer = enemySprite.GetComponent<Renderer>();
        
        Material mat = renderer.material;
        mat.SetFloat("_AlphaHit", value);

        renderer.material = mat;
    }

    private void StandAfterAttack()
    {
        standAfterAttackSequence = DOTween.Sequence();
        standAfterAttackSequence.AppendInterval(attackNodeParameters.standAfterAttackCD);
        standAfterAttackSequence.AppendCallback(() =>
        {
            waitAfterAttackFinished = true;
        });
    }

    private void EnemyRaycastHit(Color color)
    {
        Debug.DrawRay(ray.origin, ray.direction.normalized * attackNodeParameters.enemyRayMaxDistance, color);
        hits = Physics.RaycastAll(ray.origin, ray.direction, attackNodeParameters.enemyRayMaxDistance);
        Debug.Log("Hits: " + hits.Length);
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
        if (!attackNodeParameters.jumpAttack)
        {
            attackSequence.Kill();
        }
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, attackNodeParameters.attackVisibilityDistance);
    }
}