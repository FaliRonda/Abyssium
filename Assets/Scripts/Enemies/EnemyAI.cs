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
    public Transform chasePivotTransform;

    public int lifeAmount = 3;
    public GameObject itemToDrop;

    // Attack distance
    public float spriteBlinkingFrecuency = 0.15f;
    public float damageBlinkingDuration = 1f;
    public Transform[] waypoints;
    
    // Behavior tree root node
    public EnemyBTNodesSO behaviorNodeContainer;
    private BTSelector rootNode;
    private Dictionary<string, object> parameters;
    
    private bool spriteBlinking = false;
    private float damagedBlinkingCounter;
    private bool isDead = false;
    
    private SpriteRenderer enemySprite;
    private SpriteRenderer shadowSprite;
    private Animator enemyAnimator;

    private Quaternion defaultEnemySpriteRotation;
    private SphereCollider attackCollider;

    public void Initialize(Transform pjTransform)
    {
        playerTransform = pjTransform;
        
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
        
        SpriteRenderer[] enemySprites = GetComponentsInChildren<SpriteRenderer>();
        enemySprite = enemySprites[0];
        shadowSprite = enemySprites[1];
        enemyAnimator = GetComponentInChildren<Animator>();
        attackCollider = gameObject.GetComponentsInChildren<SphereCollider>()[1];

        defaultEnemySpriteRotation = enemySprite.transform.rotation;

        // Create the behavior tree
        rootNode = new BTSelector(behaviorNodeContainer.behaviorNodes.ToArray());
        
        parameters = new Dictionary<string, object>
        {
            { "EnemyTransform", transform },
            { "PlayerTransform", playerTransform },
            { "ChasePivotTransform", chasePivotTransform },
            { "Waypoints", waypoints },
            { "EnemyAnimator", enemyAnimator },
            { "EnemySprite", enemySprite },
            // Agrega otros parámetros según sea necesario
        };

        rootNode.InitializeNode(parameters);
    }

    public void DoUpdate()
    {
        // Update the behavior tree
        rootNode.Execute();
    }

#if UNITY_EDITOR
    
    private void OnDrawGizmosSelected()
    {
        if (EditorApplication.isPlaying)
        {
            rootNode.DrawGizmos();
        }
    }
    
#endif

    public void GetDamage(int damageAmount)
    {
        Core.Audio.Play(SOUND_TYPE.PjImpact, 1, 0.1f, 0.05f);
        PlayDamagedAnimation();
        
        if (!isDead)
        {
            lifeAmount -= damageAmount;
            if (lifeAmount <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        isDead = true;
        
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null && itemToDrop != null)
        {
            dropper.Drop(itemToDrop);
        }

        rootNode.AIActive = false;
        enemyAnimator.Play("Enemy_die");
        Core.Audio.Play(SOUND_TYPE.EnemyDied, 1, 0.05f, 0.01f);
        shadowSprite.enabled = false;
        float animLenght = Core.AnimatorHelper.GetAnimLength(enemyAnimator, "Enemy_die");
        Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () =>
        {
            enemySprite.GetComponent<LookCameraOn3D>().rotateCameraOn3DActive = false;
            Core.Event.Fire(new GameEvents.EnemyDied(){ enemy = this});
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
            other.GetComponent<PJ>().GetDamage(transform);
            attackCollider.enabled = false;
        }
    }
}