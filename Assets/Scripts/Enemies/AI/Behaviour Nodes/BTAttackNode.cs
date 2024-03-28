using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class BTAttackNode : BTNode
{
    private Enemies.ENEMY_TYPE enemyCode;
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

    public BTAttackNode(Enemies.ENEMY_TYPE enemyCode)
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
            if (EnemyRayHitLayer(Layers.WALL_LAYER) || EnemyRayHitLayer(Layers.DOOR_LAYER))
            {
                attackSequence.Kill();
            }
            if (EnemyRayHitLayer(Layers.PJ_LAYER) && !playerTransform.GetComponent<PJ>().pjInvulnerable && !attackNodeParameters.jumpAttack)
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
                    if (Attack(playerDirection))
                    {
                        WaitForNextAttack();
                        return BTNodeState.Running;
                    }
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

    private bool Attack(Vector3 direction)
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
            return DoLinealAttack();
        }
        else
        {
            return DoJumpAttack();
        }
    }

    private bool DoLinealAttack()
    {
        Vector3 enemyPosition = enemyTransform.position;
        Vector3 targetPosition = playerTransform.position;
        
        Vector3 anticipationDirection = (enemyPosition - targetPosition).normalized * attackNodeParameters.anticipationDistance;
        float whiteHitTargetValue = 1 - attackNodeParameters.whiteHitPercentage;

        Color castingColor = attackNodeParameters.castingColor;

        Vector3 attackDirection = (playerTransform.position - enemyPosition).normalized * attackNodeParameters.attackMovementDistance;
        
        attackSequence
            .AppendCallback(() => { enemyAnimator.Play("Enemy_attack"); })
            .Append(enemyTransform.DOMove(enemyPosition + anticipationDirection, attackNodeParameters.anticipacionDuration))
            .Join(DOTween.To(() => 1, x =>
            {
                whiteHitTargetValue = x;
                UpdateCastingGrading(x, castingColor);
            }, whiteHitTargetValue, attackNodeParameters.anticipacionDuration));


        attackSequence
            .AppendCallback(() =>
            {
                UpdateCastingGrading(1, castingColor);
                enemyAI.attackCollider.isTrigger = false;
            })
            .Append(enemyTransform.DOMove(enemyPosition + attackDirection, attackNodeParameters.attackMovementDuration))
            .AppendCallback(StandAfterAttack)
            .AppendInterval(0.1f)
            .AppendCallback(() => { enemyAI.attackCollider.isTrigger = true; })
            .OnKill(() =>
            {
                UpdateCastingGrading(1, castingColor);
                StandAfterAttack();

                Sequence waitAndDisableColliderSequence = DOTween.Sequence();
                waitAndDisableColliderSequence
                    .AppendInterval(0.1f)
                    .AppendCallback(() => { enemyAI.attackCollider.isTrigger = true; });
            });
        return true;
    }
    
    private bool DoJumpAttack()
    {
        Vector3 targetPosition = playerTransform.position;

        EnemyRaycastHit(Color.green, attackNodeParameters.attackMovementDistance);
        if (!EnemyRayHitLayer(Layers.WALL_LAYER) & !EnemyRayHitLayer(Layers.DOOR_LAYER))
        {
            SpriteRenderer shadowSprite = enemyAI.GetComponentsInChildren<SpriteRenderer>()[1];
            SphereCollider attackCollider = (SphereCollider)enemyAI.attackCollider;
            MeshCollider damageCollider = enemyAI.GetComponentInChildren<MeshCollider>();
            
            attackSequence
                .AppendCallback(() =>
                {
                    damageCollider.enabled = false;
                    attackCollider.isTrigger = false;
                    attackCollider.radius *= 2;
                    shadowSprite.enabled = false;
                })
                .Append(enemyTransform.DOJump(targetPosition, attackNodeParameters.jumpHeight, 1, attackNodeParameters.jumpDuration))
                .SetEase(Ease.Linear)
                .AppendCallback(() =>
                {
                    ShowJumpParticles();
                })
                .AppendCallback(StandAfterAttack)
                .AppendInterval(0.1f)
                .OnKill(() =>
                {
                    attackCollider.radius /= 2;
                    attackPlaying = false;
                    shadowSprite.enabled = true;
                    StandAfterAttack();

                    Sequence waitAndDisableColliderSequence = DOTween.Sequence();
                    waitAndDisableColliderSequence
                        .AppendInterval(0.1f)
                        .AppendCallback(() =>
                        {
                            damageCollider.enabled = true;
                            attackCollider.isTrigger = true;
                        });
                });
            return true;
        }
        
        attackPlaying = false;
        return false;
    }

    private void ShowJumpParticles()
    {
        ParticleSystem originalJumpParticles = enemyTransform.GetComponentsInChildren<ParticleSystem>()[1];
        GameObject jumpParticlesCopy = GameObject.Instantiate(originalJumpParticles.gameObject, enemyTransform.parent);
        jumpParticlesCopy.transform.position = enemyTransform.position;

        ParticleSystem jumpParticles = jumpParticlesCopy.GetComponent<ParticleSystem>();
        
        jumpParticles.Play();
        jumpParticles.transform.DOScale(new Vector3(5, 5, 5), 1)
            .OnComplete(() =>
            {
                jumpParticles.transform.localScale = new Vector3(1, 1, 1);
                GameObject.Destroy(jumpParticlesCopy);
            });
    }

    private void UpdateCastingGrading(float value, Color color)
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
        
        Renderer renderer = enemySprite.GetComponent<Renderer>();
        
        Material mat = renderer.material;
        mat.SetFloat("_AlphaCasting", value);
        mat.SetColor("_ColorCasting", color);

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

    private void EnemyRaycastHit(Color color, float attackMovementDistance)
    {
        Debug.DrawRay(ray.origin, ray.direction.normalized * attackMovementDistance, color);
        hits = Physics.RaycastAll(ray.origin, ray.direction, attackMovementDistance);
    }
    
    private void EnemyRaycastHit(Color color)
    {
        EnemyRaycastHit(color, attackNodeParameters.enemyRayMaxDistance);
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
        if (attackNodeParameters.jumpAttack)
        {
            return;
        }

        if (attackNodeParameters.stoppedByStun && enemyAI.enemyStunned)
        {
            attackSequence.Kill();
        }

        if (enemyAI.playerDamaged)
        {
            attackSequence.Kill();
        }
    }
    
    public override void ResetNode(bool force, bool enemyDead)
    {
        if (force || enemyDead)
        {
            attackSequence.Kill();
        }
        else
        {
            this.ResetNode();
        }
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, attackNodeParameters.attackVisibilityDistance);
    }
}