using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class BTShootNode : BTNode
{
    private Enemies.ENEMY_TYPE enemyCode;
    private bool waitForNextShoot;
    private bool isShooting;
    private ShootNodeParametersSO shootNodeParameters;
    private Sequence shootSequence;
    private MaterialPropertyBlock propertyBlock;
    private int bulletsCreated;
    private bool shootCDReady;

    public BTShootNode(Enemies.ENEMY_TYPE enemyCode)
    {
        this.enemyCode = enemyCode;
    }

    public override BTNodeState Execute()
    {
        // Check if the player is within attack distance
        Vector3 direction = playerTransform.position - enemyTransform.position;
        float distance = direction.magnitude;

        if (distance <= shootNodeParameters.attackVisibilityDistance)
        {
            if (shootCDReady)
            {
                waitForNextShoot = false;
                shootCDReady = false;
                return BTNodeState.NextTree;
            }
            
            if (!waitForNextShoot)
            {
                Shoot(direction);
                return BTNodeState.Running;
            }
            
            if (isShooting)
            {
                return BTNodeState.Running;
            }
        }
        
        return BTNodeState.NextTree;
    }

    private void Shoot(Vector3 direction)
    {
        isShooting = true;
        waitForNextShoot = true;
        enemySprite.flipX = direction.x > 0;

        // Animation
        enemyAnimator.Play("Enemy_attack");
        float animLength = Core.AnimatorHelper.GetAnimLength(enemyAnimator, "Enemy_attack");

        float whiteHitTargetValue = 1 - shootNodeParameters.whiteHitPercentage;

        Color castingColor = shootNodeParameters.castingColor;
        
        shootSequence = DOTween.Sequence();
        shootSequence
            .Append(DOTween.To(() => 1, x => {
                whiteHitTargetValue = x;
                UpdateCastingGrading(x, castingColor);
            }, whiteHitTargetValue, shootNodeParameters.anticipacionDuration))
            .AppendCallback(() =>
            {
                UpdateCastingGrading(1, castingColor);
                ShootBullets();
            })
            .OnKill(() =>
            {
                UpdateCastingGrading(1, castingColor);
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

    private void ShootBullets()
    {
        bulletsCreated = 0;
        CreateBullet();
    }

    private void CreateBullet()
    {
        if (isShooting)
        {
            if (!shootNodeParameters.isRadialShoot)
            {
                Vector3 direction = playerTransform.position - enemyTransform.position;
                var bullet = InstanceBullet(direction);
                bullet.StartShoot(direction);
            }
            else
            {
                Vector3[] radialDirections = CalculateRadialDirections();
                
                foreach (Vector3 direction in radialDirections)
                {
                    var bullet = InstanceBullet(direction);
                    bullet.StartShoot(direction);
                }
            }

            bulletsCreated++;
            
            if (bulletsCreated < shootNodeParameters.bulletsPerShoot)
            {
                Sequence bulletsCDSequence = DOTween.Sequence();
                bulletsCDSequence
                    .AppendInterval(shootNodeParameters.timeBetweenBullets)
                    .AppendCallback(() => CreateBullet());
            }
            else
            {
                isShooting = false;
                bulletsCreated = 0;
                Sequence shootCDSequence = DOTween.Sequence();
                shootCDSequence.AppendInterval(shootNodeParameters.shootCD).AppendCallback(() => { shootCDReady = true; });
            }
        }
    }

    private Bullet InstanceBullet(Vector3 direction)
    {
        GameObject bulletGO = Object.Instantiate(shootNodeParameters.bulletPrefab);
        bulletGO.transform.position = enemyTransform.position + direction.normalized * 0.5f;
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.Initialize(shootNodeParameters.bulletSpeed, shootNodeParameters.bulletLifeTime);
        return bullet;
    }

    public Vector3[] CalculateRadialDirections()
    {
        Vector3[] directions = new Vector3[shootNodeParameters.numberOfRadialBullets];
        float angleIncrement = 360f / shootNodeParameters.numberOfRadialBullets;

        for (int i = 0; i < shootNodeParameters.numberOfRadialBullets; i++)
        {
            float angle = i * angleIncrement;
            directions[i] = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        }

        return directions;
    }
    
    public override void ResetNode(bool force, bool enemyDead)
    {
        if (enemyDead)
        {
            shootSequence.Kill();
            isShooting = false;
            bulletsCreated = 0;
        }
    }

    public override void InitializeNode(Dictionary<string, object> parameters)
    {
        base.InitializeNode(parameters);
        AssignNodeParameters();
    }
    
    private void AssignNodeParameters()
    {
        shootNodeParameters =
            Resources.Load<ShootNodeParametersSO>(Enemies.EnemiesParametersPathDictionary(enemyCode, "Shoot"));

        // Check if the ScriptableObject was loaded successfully.
        if (shootNodeParameters == null)
        {
            Debug.LogError("EnemyParemeters not found in Resources folder.");
        }
    }

    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, shootNodeParameters.attackVisibilityDistance);
    }
}