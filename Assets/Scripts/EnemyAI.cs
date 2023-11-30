using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Ju.Extensions;
using UnityEditor;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class EnemyAI : MonoBehaviour
{
    // Reference to the player
    public Transform playerTransform;

    public int lifeAmount = 3;
    public GameObject itemToDrop;

    // Attack distance
    public float spriteBlinkingFrecuency = 0.15f;
    public float damageBlinkingDuration = 1f;
    public Transform[] waypoints;
    
    // Behavior tree root node
    public EnemyBTNodesSO behaviorNodeContainer;
    private BTSelector rootNode;
    private BTAttackNode attackNode;
    private BTChaseNode chaseNode;
    private BTPatrolNode patrolNode;
    private Dictionary<string, object> parameters;
    
    private bool spriteBlinking = false;
    private float damagedBlinkingCounter;
    private bool isDead = false;
    
    private SpriteRenderer enemySprite;
    private Animator enemyAnimator;
    private AudioSource detectedAudioSource;

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
        detectedAudioSource = GetComponentInChildren<AudioSource>();
        enemySprite = GetComponentInChildren<SpriteRenderer>();
        enemyAnimator = GetComponentInChildren<Animator>();
        attackCollider = gameObject.GetComponentsInChildren<SphereCollider>()[1];

        defaultEnemySpriteRotation = enemySprite.transform.rotation;

        // Create the behavior tree
        rootNode = new BTSelector(behaviorNodeContainer.behaviorNodes.ToArray());
        
        parameters = new Dictionary<string, object>
        {
            { "EnemyTransform", transform },
            { "PlayerTransform", playerTransform },
            { "Waypoints", waypoints },
            { "EnemyAnimator", enemyAnimator },
            { "EnemySprite", enemySprite },
            { "DetectedAudioSource", detectedAudioSource },
            // Agrega otros parámetros según sea necesario
        };

        rootNode.InitializeNode(parameters);
    }

    private void Update()
    {
        // Update the behavior tree
        rootNode.Execute();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (EditorApplication.isPlaying)
        {
            rootNode.DrawGizmos();
        }
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
                PlayDamagedAnimation();
            }
        }
    }

    private void Die()
    {
        isDead = true;
        
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null && itemToDrop != null)
        if (dropper != null && itemToDrop != null)
        {
            dropper.Drop(itemToDrop);
        }

        rootNode.AIActive = false;
        enemyAnimator.Play("Enemy_die");
        float animLenght = Core.AnimatorHelper.GetAnimLenght(enemyAnimator, "Enemy_die");
        Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () =>
        {
            enemySprite.GetComponent<LookCameraOn3D>().rotateCameraOn3DActive = false;
            Core.Event.Fire(new GameEvents.EnemyDied());
        });
    }

    private void PlayDamagedAnimation()
    {
        damagedBlinkingCounter = damageBlinkingDuration;
        StartCoroutine(SpriteBlinking());
    }

    IEnumerator SpriteBlinking()
    {
        while (damagedBlinkingCounter > 0)
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

            damagedBlinkingCounter -= Time.deltaTime;
            
            yield return null;
        }

        //TODO debería hacerse esto en otro lugar?
        //chaseNode.ResetChasing();
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
        attackCollider.enabled = true;
    }

    public void DeactiveAttackTrigger()
    {
        attackCollider.enabled = false;
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.PJ_LAYER)
        {
            other.GetComponent<PJ>().GetDamage();
            attackCollider.enabled = false;
        }
    }
}