using System;
using System.Collections;
using DG.Tweening;
using Ju.Extensions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

public class EnemyAI : MonoBehaviour
{
    // Reference to the player
    public Transform playerTransform;

    public int lifeAmount = 3;
    public float patrolSpeed = 1f;
    public float chaseSpeed = 1f;
    public float chaseInLightSpeed = 0.5f;
    public GameObject itemToDrop;
    
    // Visibility radius
    public float visibilityRadius = 10f;
    public float lightRadius = 10f;

    // Attack distance
    public float attackDistance = 2f;
    public float spriteBlinkingFrecuency = 0.15f;
    public Transform[] waypoints;
    

    // Behavior tree root node
    public bool isShadow = false;
    private BTSelector rootNode;
    private AudioSource detectedAudio;
    
    private bool beingDamaged = false;
    private bool spriteBlinking = false;
    private bool isDead = false;
    private SpriteRenderer enemySprite;
    private Animator enemyAnimator;

    private BTAttackNode attackNode;
    private BTChaseNode chaseNode;
    private BTPatrolNode patrolNode;

    private Quaternion defaultEnemySpriteRotation;
    private SphereCollider attackCollider;

    private void Awake()
    {
        PJ player = (PJ)FindObjectOfType(typeof(PJ));
        playerTransform = player.transform;
        
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
    }

    private void Start()
    {
        detectedAudio = GetComponentInChildren<AudioSource>();
        enemySprite = GetComponentInChildren<SpriteRenderer>();
        enemyAnimator = GetComponentInChildren<Animator>();
        attackCollider = gameObject.GetComponentsInChildren<SphereCollider>()[1];

        defaultEnemySpriteRotation = enemySprite.transform.rotation;

        attackNode = new BTAttackNode(transform, playerTransform, enemyAnimator, enemySprite, attackDistance);
        chaseNode = new BTChaseNode(transform, playerTransform, enemyAnimator, enemySprite, chaseSpeed, chaseInLightSpeed, isShadow, detectedAudio);
        patrolNode = new BTPatrolNode(transform, waypoints, enemyAnimator, enemySprite, patrolSpeed);
        
        // Create the behavior tree
        rootNode = new BTSelector(
            attackNode,
            chaseNode,
            patrolNode
        );
    }

    private void Update()
    {
        if (!beingDamaged)
        {
            // Update the behavior tree
            rootNode.Execute();
        }
        
        
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw the visibility radius gizmo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visibilityRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    public void GetDamage(int damageAmount)
    {
        if (!isDead)
        {
            lifeAmount -= damageAmount;
            if (lifeAmount <= 0)
            {
                Die();
            }
            else
            {
                PlayDamageAnimation();
            }
        }
    }

    private void Die()
    {
        isDead = true;
        
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null)
        {
            dropper.Drop(itemToDrop);
        }

        rootNode.AIActive = false;
        enemyAnimator.Play("Stilt_die");
        float animLenght = Core.AnimatorHelper.GetAnimLenght(enemyAnimator, "Stilt_die_anim");
        Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () =>
        {
            enemySprite.GetComponent<LookCameraOn3D>().rotateCameraOn3DActive = false;
            Core.Event.Fire(new GameEvents.EnemyDied());
        });
    }

    private void PlayDamageAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => beingDamaged = true)
            .AppendCallback(() => StartCoroutine(SpriteBlinking()))
            .AppendInterval(2f)
            .AppendCallback(() => beingDamaged = false);
    }

    IEnumerator SpriteBlinking()
    {
        enemySprite.color = Color.red;
        
        while (beingDamaged)
        {
            if (!spriteBlinking)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.AppendCallback(() => spriteBlinking = true)
                    .AppendCallback(() => enemySprite.enabled = false)
                    .AppendInterval(spriteBlinkingFrecuency)
                    .AppendCallback(() => enemySprite.enabled = true)
                    .AppendInterval(spriteBlinkingFrecuency)
                    .AppendCallback(() => spriteBlinking = false);
            }
            
            yield return null;
        }

        chaseNode.ResetChasing();
        enemySprite.color = Color.white;
        yield return null;
    }

    public void Switch2D3D(bool gameIn3D)
    {
        Transform enemySpriteTransform = enemySprite.transform;
        
        if (gameIn3D)
        {
            enemySpriteTransform.Rotate(new Vector3(-90, 0, 0));
        }
        else
        {
            enemySpriteTransform.rotation = defaultEnemySpriteRotation;
        }
    }

    public void ActiveAttackTrigger()
    {
        if (enemySprite.flipX)
        {
            attackCollider.center = new Vector3(0.75f, 0, 0.15f);
        }
        else
        {
            attackCollider.center = new Vector3(-0.75f, 0, 0.15f);
        }
        
        attackCollider.enabled = true;
    }

    public void DeactiveAttackTrigger()
    {
        attackCollider.enabled = false;
    }
    
    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == Layers.PJ_LAYER)
        {
            Debug.Log("Attacking player");
            attackCollider.enabled = false;
        }
    }
}