using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

[CreateAssetMenu(fileName = "New BT Shoot Node", menuName = "AI/BT Nodes/Shoot Node")]
public class BTShootNode : BTNode
{
    public GameObject bulletPrefab;
    public float bulletLifeTime = 2f;
    public float bulletSpeed = 1f;
    
    public float attackVisibilityDistance;
    public float shootCD = 1f;
    
    private bool waitForNextShoot;
    private bool shootPlaying;

    public override BTNodeState Execute()
    {
        // Check if the player is within attack distance
        Vector3 direction = playerTransform.position - enemyTransform.position;
        float distance = direction.magnitude;

        if (distance <= attackVisibilityDistance)
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
        shootPlaying = true;
        float animLenght = Core.AnimatorHelper.GetAnimLength(enemyAnimator, "Enemy_attack");
        Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () => { shootPlaying = false; });

        // CD
        waitForNextShoot = true;
        Sequence shootCDSequence = DOTween.Sequence();
        shootCDSequence.AppendInterval(shootCD).AppendCallback(() => { waitForNextShoot = false; });
        
        // Shoot
        GameObject bulletGO = Object.Instantiate(bulletPrefab, enemyTransform);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.Initialize(bulletSpeed, bulletLifeTime, playerTransform);
        bullet.StartShoot(direction);
    }

    public override void ResetNode()
    {
        waitForNextShoot = false;
        shootPlaying = false;
    }
    
    public override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, attackVisibilityDistance);
    }
}