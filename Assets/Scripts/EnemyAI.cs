using System.Collections;
using DG.Tweening;
using Ju.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyAI : MonoBehaviour
{
    // Reference to the player
    [FormerlySerializedAs("player")] public Transform playerTransform;

    public int lifeAmount = 3;
    public float patrolSpeed = 1f;
    public float chaseSpeed = 1f;
    public GameObject itemToDrop;
    
    // Visibility radius
    public float visibilityRadius = 10f;

    // Attack distance
    public float attackDistance = 2f;
    public float spriteBlinkingFrecuency = 0.15f;
    public Transform[] waypoints;

    // Behavior tree root node
    private BTNode rootNode;
    private AudioSource detectedAudio;
    
    private bool beingDamaged = false;
    private SpriteRenderer enemySprite;
    private bool spriteBlinking = false;

    private BTAttackNode attackNode;
    private BTChaseNode chaseNode;
    private BTPatrolNode patrolNode;

    private Quaternion defaultEnemySpriteRotation;

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

        defaultEnemySpriteRotation = enemySprite.transform.rotation;

        attackNode = new BTAttackNode(transform, playerTransform, attackDistance);
        chaseNode = new BTChaseNode(transform, playerTransform, chaseSpeed, detectedAudio);
        patrolNode = new BTPatrolNode(transform, waypoints, patrolSpeed);
        
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

    private void Die()
    {
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null)
        {
            dropper.Drop(itemToDrop);
        }
        
        Core.Event.Fire(new GameEvents.EnemyDied());
        Destroy(gameObject);
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
}