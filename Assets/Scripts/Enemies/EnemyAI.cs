using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Ju.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;
using Sequence = DG.Tweening.Sequence;

public class EnemyAI : MonoBehaviour
{
    public bool aIActive;
    public bool isBoss = false;
    public Material dissolveMaterial;
    // Reference to the player
    public Transform playerTransform;
    public Transform chasePivotTransform;

    public int lifeAmount = 3;
    public GameObject itemToDrop;

    public float damagedCamShakeIntensity = 2f;
    public float damagedCamShakeFrequency = 0.3f;
    public float damagedCamShakeDuration = 0.5f;
    public float knockbackMovementFactor = 1f;
    public float spriteBlinkingFrecuency = 0.15f;
    public float damageBlinkingDuration = 1f;
    public float stunnedCD = 2f;
    public float invulnerableCD = 0.1f;
    
    public float damagedGamepadVibrationIntensity = 0.5f;
    public float damagedGamepadVibrationDuration = 0.2f;
    
    // Behavior tree root node
    public List<EnemyBTNodesSO> behaviorNodesContainer;
    private List<BTSelector> nodeTrees;
    private Dictionary<string, object> parameters;
    
    [HideInInspector]
    public bool attackInCD;
    [HideInInspector]
    public bool enemyStunned;
    private bool spriteBlinking = false;
    private float damagedBlinkingCounter;
    private bool enemyInvulnerable;
    [HideInInspector]
    public bool isDead = false;
    
    private SpriteRenderer enemySprite;
    private SpriteRenderer shadowSprite;
    private Animator enemyAnimator;
    private ParticleSystem enemyDamagedParticles;

    private Quaternion defaultEnemySpriteRotation;
    [HideInInspector]
    public Collider attackCollider;

    private Ray ray;
    private RaycastHit[] hits;
    private Sequence knockbackSequence;
    private int currentTreeIndex = -1;
    private BTSelector currentTree;
    [HideInInspector]
    public bool playerDamaged;
    private Slider lifeSlider;
    private Sequence lifeSliderCDSequence;
    private bool thirdLifeReached;
    private Material originalMaterial;
    private int minionsCounter;
    private bool bossDissolved;


    public void Initialize(Transform pjTransform)
    {
        playerTransform = pjTransform;
        
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
        this.EventSubscribe<GameEvents.BossDied>(e => Die());
        this.EventSubscribe<GameEvents.EnemyDied>(e => EnemyDied());

        if (isBoss)
        {
            Core.Event.Fire(new GameEvents.BossSpawned(){bossLife = lifeAmount});
        }
        
        SpriteRenderer[] enemySprites = GetComponentsInChildren<SpriteRenderer>();
        enemySprite = enemySprites[0];
        shadowSprite = enemySprites[1];
        enemyAnimator = GetComponentInChildren<Animator>();
        enemyDamagedParticles = GetComponentInChildren<ParticleSystem>();
        attackCollider = gameObject.GetComponentsInChildren<Collider>()[0];
        
        lifeSlider = gameObject.GetComponentInChildren<Slider>();
        lifeSlider.maxValue = lifeAmount;
        lifeSlider.value = lifeAmount;
        lifeSlider.gameObject.SetActive(false);

        if (isBoss)
        {
            Image[] allSliderImages = lifeSlider.GetComponentsInChildren<Image>();
            Image[] lifeThirdsIndicators = new Image[] {allSliderImages[allSliderImages.Length-1], allSliderImages[allSliderImages.Length-2]};

            lifeThirdsIndicators[0].enabled = false;
            lifeThirdsIndicators[1].enabled = false;
            
            Transform lifeSliderTransform = lifeSlider.transform;
            Vector3 initialScale = new Vector3(0f, lifeSliderTransform.localScale.y, lifeSliderTransform.localScale.z);
            lifeSliderTransform.localScale = initialScale;

            Vector3 finalScale = new Vector3(5f, lifeSliderTransform.localScale.y, lifeSliderTransform.localScale.z);

            lifeSliderTransform.gameObject.SetActive(true);
            lifeSliderTransform.transform.DOScale(finalScale, 1f).SetEase(Ease.OutQuad)
                .OnKill(() =>
                {
                    lifeThirdsIndicators[0].enabled = true;
                    lifeThirdsIndicators[1].enabled = true;
                });
        }

        defaultEnemySpriteRotation = enemySprite.transform.rotation;

        parameters = new Dictionary<string, object>
        {
            { "EnemyAI", this },
            { "EnemyTransform", transform },
            { "PlayerTransform", playerTransform },
            { "ChasePivotTransform", chasePivotTransform },
            { "EnemyAnimator", enemyAnimator },
            { "EnemySprite", enemySprite },
            // Agrega otros parámetros según sea necesario
        };

        nodeTrees = new List<BTSelector>();

        foreach (EnemyBTNodesSO nodeTree in behaviorNodesContainer)
        {
            var newNodeTree = new BTSelector(nodeTree.behaviorNodes.ToArray(), nodeTree.enemyCode);
            newNodeTree.InitializeNode(parameters);
            nodeTrees.Add(newNodeTree);
        }
    }

    private void EnemyDied()
    {
        if (isBoss && bossDissolved)
        {
            minionsCounter--;

            if (minionsCounter == 0)
            {
                BossReappear();
            }
        }
    }

    private void BossReappear()
    {
        Renderer renderer = enemySprite.GetComponent<Renderer>();
        float nextValue = 0;

        Sequence bossReappearingSequence = DOTween.Sequence();
        
        bossReappearingSequence
            .AppendInterval(1f)
            .Append(DOTween.To(() => renderer.material.GetFloat("_DissolveAmount"), x =>
            {
                nextValue = x;
                renderer.material.SetFloat("_DissolveAmount", x);
            }, 1.8f, 1f))
            .AppendCallback(() =>
            {
                renderer.material = originalMaterial;
                shadowSprite.enabled = true;
                transform.GetComponentInChildren<MeshCollider>().enabled = true;
                bossDissolved = false;
            });

    }

    public void DoUpdate()
    {
        if (!isDead && aIActive)
        {
            if (isBoss && thirdLifeReached)
            {
                ResetAINodes(true, false);
                
                thirdLifeReached = false;
                bossDissolved = true;
                Renderer renderer = enemySprite.GetComponent<Renderer>();
                originalMaterial = renderer.material;
                float targetValue = 0;
                
                Sequence dissolveSequence = DOTween.Sequence();
                dissolveSequence
                    .AppendCallback(() =>
                    {
                        transform.GetComponentInChildren<MeshCollider>().enabled = false;
                        enemySprite.color = new Color(enemySprite.color.r, enemySprite.color.g, enemySprite.color.b,
                            0.25f);
                    })
                    .Append(transform.DOMove(new Vector3(0, 0, 0), 1f))
                    .AppendCallback(() =>
                    {
                        currentTree = nodeTrees[2];
                        currentTree.Execute();
                        minionsCounter = 3;
                    })
                    .AppendInterval(2)
                    .AppendCallback(() =>
                    {
                        renderer.material = dissolveMaterial;
                        renderer.material.SetFloat("_DissolveAmount", 1.8f);

                        DOTween.To(() => renderer.material.GetFloat("_DissolveAmount"), x =>
                        {
                            targetValue = x;
                            renderer.material.SetFloat("_DissolveAmount", x);
                        }, 0, 1f)
                            .OnKill(() =>
                            {
                                shadowSprite.enabled = false;
                            });
                    });
            }
            else if (!bossDissolved)
            {
                Random random = new Random();
                
                while (currentTreeIndex == -1 || currentTreeIndex == 2)
                {
                    currentTreeIndex = random.Next(0, nodeTrees.Count);
                }

                currentTree = nodeTrees[currentTreeIndex];
                
                var state = currentTree.Execute();

                if (state == BTNodeState.NextTree)
                {
                    currentTreeIndex = -1;
                }
            }
        }
    }

#if UNITY_EDITOR
    
    private void OnDrawGizmosSelected()
    {
        if (EditorApplication.isPlaying)
        {
            foreach (BTSelector nodeTree in nodeTrees)
            {
                nodeTree.DrawGizmos();
            }
        }
    }
    
#endif

    public void GetDamage(int damageAmount)
    {
        if (!enemyInvulnerable)
        {
            lifeAmount -= damageAmount;
            isDead = lifeAmount <= 0;

            thirdLifeReached = isBoss && lifeAmount % (lifeSlider.maxValue / 3) == 0;

            UpdateLifeUI();
            
            enemyInvulnerable = true;

            if (isBoss)
            {
                Core.Event.Fire(new GameEvents.BossDamaged(){});
            }
            
            Sequence invulnerableSequence = DOTween.Sequence();
            invulnerableSequence
                .AppendInterval(invulnerableCD)
                .AppendCallback(() => enemyInvulnerable = false);
            
            //Core.Audio.Play(SOUND_TYPE.PjImpact, 1, 0.1f, 0.05f);
            PlayDamagedAnimation();
            PlayDamagedKnockbackAnimation();

            if (!enemyStunned)
            {
                enemyStunned = true;
                
                Sequence stunnedSequence = DOTween.Sequence();
                stunnedSequence
                    .AppendInterval(stunnedCD)
                    .AppendCallback(() => enemyStunned = false);
            }
            
            foreach (BTSelector nodeTree in nodeTrees)
            {
                if (isDead)
                {
                    nodeTree.ResetNodes(false, true);
                }
                else
                {
                    nodeTree.ResetNodes(false, false);
                }
            }

            
            Core.GamepadVibrationService.SetControllerVibration(damagedGamepadVibrationIntensity, damagedGamepadVibrationDuration);
            Core.CameraEffects.StartShakingEffect(damagedCamShakeIntensity, damagedCamShakeFrequency, damagedCamShakeDuration);

            if (isDead)
            {
                Die();
            }
            else
            {
                if (isBoss)
                {
                    Core.Audio.PlayFMODAudio("event:/Characters/Enemies/Boss/GetDamage", transform);
                }
                else
                {
                    Core.Audio.PlayFMODAudio("event:/Characters/Enemies/Stalker/GetDamage", transform);
                }
            }
        }
    }
    
    private void UpdateLifeUI()
    {
        float currentLife = lifeSlider.value;
        float targetLife = lifeAmount;
        
        Sequence lifeUISequence = DOTween.Sequence();

        if (!isBoss)
        {
            lifeUISequence
                .AppendCallback(() => { lifeSlider.gameObject.SetActive(true); });
        }
        
        lifeUISequence
            .AppendInterval(0.1f)
            .Append(DOTween.To(() => currentLife, x =>
            {
                targetLife = x;
                lifeSlider.value = x;
            }, targetLife, 0.1f));

        if (currentLife == 1)
        {
            lifeUISequence
                .AppendCallback(() =>
                {
                    lifeSlider.GetComponentsInChildren<Image>()[1].enabled = false;
                });
        }
        
        if (!isBoss)
        {
            if (lifeSliderCDSequence != null)
            {
                lifeSliderCDSequence.Kill();
            }

            lifeSliderCDSequence = DOTween.Sequence();
            lifeSliderCDSequence
                .AppendInterval(1f)
                .AppendCallback(() => { lifeSlider.gameObject.SetActive(false); });
        }
    }
    
    private void PlayDamagedKnockbackAnimation()
    {
        Vector3 position = transform.position;
        Vector3 damagedDirection = (position - playerTransform.position).normalized * knockbackMovementFactor;
        
        ray = new Ray(transform.position, damagedDirection);
        
        if (!EnemyRaycastHit(Color.green, 0.5f) || !EnemyRayHitLayer(Layers.WALL_LAYER))
        {
            knockbackSequence = DOTween.Sequence();

            knockbackSequence
                .Append(transform.DOMove(position + new Vector3(damagedDirection.x, 0, damagedDirection.z), 0.2f));
            knockbackSequence.Play();
        }

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

    private void Die()
    {
        isDead = true;
        aIActive = false;
        lifeSlider.gameObject.SetActive(false);
        attackCollider.enabled = false;
        GetComponentInChildren<MeshCollider>().enabled = false;

        bool enemyDied = true;
        
        if (isBoss)
        {
            Core.Event.Fire(new GameEvents.BossDied(){});
        }
        
        Dropper dropper = GetComponent<Dropper>();
        if (dropper != null && itemToDrop != null)
        {
            dropper.Drop(itemToDrop);
        }

        enemyAnimator.Play("Enemy_die");
        //Core.Audio.Play(SOUND_TYPE.EnemyDied, 1, 0.05f, 0.01f);
        if (isBoss)
        {
            Core.Audio.PlayFMODAudio("event:/Characters/Enemies/Boss/Die", transform);
        }
        else
        {
            Core.Audio.PlayFMODAudio("event:/Characters/Enemies/Stalker/Die", transform);
        }
        shadowSprite.enabled = false;
        float animLength = Core.AnimatorHelper.GetAnimLength(enemyAnimator, "Enemy_die");
        Core.AnimatorHelper.DoOnAnimationFinish(animLength, () =>
        {
            enemySprite.GetComponent<LookCameraOn3D>().rotateCameraOn3DActive = false;
        });
        Sequence delaySequence = DOTween.Sequence();
        delaySequence
            .AppendInterval(0.01f)
            .AppendCallback(() =>
            {
                Core.Event.Fire(new GameEvents.EnemyDied() { enemy = this });
            });
        ResetAINodes(false, true);
    }

    private void PlayDamagedAnimation()
    {
        damagedBlinkingCounter = damageBlinkingDuration;
        enemyDamagedParticles.Play();
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
        // attackCollider.enabled = true;
    }

    public void DeactiveAttackTrigger()
    {
        // attackCollider.enabled = false;
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == Layers.PJ_LAYER)
        {
            HitPlayer(other.gameObject);
        }
        
        if (other.gameObject.layer == Layers.WALL_LAYER || other.gameObject.layer == Layers.DOOR_LAYER)
        {
            ResetAINodes(true, false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.WALL_LAYER || other.gameObject.layer == Layers.DOOR_LAYER)
        {
            if (knockbackSequence != null)
            {
                knockbackSequence.Kill();
            }

            ResetAINodes(true, false);
        }
    }

    private void HitPlayer(GameObject other)
    {
        playerDamaged = other.GetComponent<PJ>().GetDamage(transform);

        if (playerDamaged)
        {
            Core.Audio.PlayFMODAudio("event:/Characters/Enemies/Stalker/AttackHit", transform);
            ResetAINodes();
        }
        playerDamaged = false;
    }

    public void ResetAINodes()
    {
        foreach (BTSelector nodeTree in nodeTrees)
        {
            nodeTree.ResetNodes();
        }
    }
    
    public void ResetAINodes(bool force, bool enemyDead)
    {
        foreach (BTSelector nodeTree in nodeTrees)
        {
            nodeTree.ResetNodes(force, enemyDead);
        }
    }
    
    private bool EnemyRaycastHit(Color color, float enemyRayDistance)
    {
        Debug.DrawRay(ray.origin, ray.direction, color);
        hits = Physics.RaycastAll(ray.origin, ray.direction, enemyRayDistance);
        return hits.Length > 0;
    }
}

