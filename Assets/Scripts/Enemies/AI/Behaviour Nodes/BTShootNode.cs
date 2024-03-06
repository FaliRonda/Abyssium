using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class BTShootNode : BTNode
{
    private Enemies.CODE_NAMES enemyCode;
    private bool waitForNextShoot;
    private bool isShooting;
    private ShootNodeParametersSO shootNodeParameters;
    private Sequence shootSequence;
    private MaterialPropertyBlock propertyBlock;

    public BTShootNode(Enemies.CODE_NAMES enemyCode)
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
            if (!waitForNextShoot)
            {
                Shoot(direction);
                return BTNodeState.Success;
            }
            
            if (isShooting)
            {
                return BTNodeState.Running;
            } 
        }
        
        return BTNodeState.Failure;
    }

    private void Shoot(Vector3 direction)
    {
        isShooting = true;
        enemySprite.flipX = direction.x > 0;

        // Animation
        enemyAnimator.Play("Enemy_attack");
        float animLength = Core.AnimatorHelper.GetAnimLength(enemyAnimator, "Enemy_attack");

        // CD
        waitForNextShoot = true;
        Sequence shootCDSequence = DOTween.Sequence();
        shootCDSequence.AppendInterval(shootNodeParameters.shootCD).AppendCallback(() => { waitForNextShoot = false; });

        float whiteHitTargetValue = 1 - shootNodeParameters.whiteHitPercentage;

        shootSequence = DOTween.Sequence();
        shootSequence
            .Append(DOTween.To(() => 1, x => {
                whiteHitTargetValue = x;
                UpdateWhiteHitValue(x);
            }, whiteHitTargetValue, shootNodeParameters.anticipacionDuration))
            .AppendCallback(() =>
            {
                UpdateWhiteHitValue(1);
                CreateBullet(direction);
                isShooting = false;
            })
            .OnKill(() =>
            {
                UpdateWhiteHitValue(1);
                isShooting = false;
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

    private void CreateBullet(Vector3 direction)
    {
        GameObject bulletGO = Object.Instantiate(shootNodeParameters.bulletPrefab);
        bulletGO.transform.position = enemyTransform.position;
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.Initialize(shootNodeParameters.bulletSpeed, shootNodeParameters.bulletLifeTime);
        bullet.StartShoot(direction);
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