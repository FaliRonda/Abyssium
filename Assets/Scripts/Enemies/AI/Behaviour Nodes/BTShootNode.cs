using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

[CreateAssetMenu(fileName = "New BT Shoot Node", menuName = "AI/BT Nodes/Shoot Node")]
public class BTShootNode : BTNode
{
    public Enemies.CODE_NAMES enemyCode;
    public GameObject bulletPrefab;
    
    private bool waitForNextShoot;
    private ShootNodeParametersSO shootNodeParameters;

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
            else
            {
                return BTNodeState.Failure;
            } 
        }
        else
        {
            return BTNodeState.Failure;
        }
    }

    private void Shoot(Vector3 direction)
    {
        enemySprite.flipX = direction.x > 0;

        // Animation
        enemyAnimator.Play("Enemy_attack");
        float animLength = Core.AnimatorHelper.GetAnimLength(enemyAnimator, "Enemy_attack");

        // CD
        waitForNextShoot = true;
        Sequence shootCDSequence = DOTween.Sequence();
        shootCDSequence.AppendInterval(shootNodeParameters.shootCD).AppendCallback(() => { waitForNextShoot = false; });
        
        // Shoot
        GameObject bulletGO = Object.Instantiate(bulletPrefab);
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

    public override void ResetNode()
    {
        waitForNextShoot = false;
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, shootNodeParameters.attackVisibilityDistance);
    }
}