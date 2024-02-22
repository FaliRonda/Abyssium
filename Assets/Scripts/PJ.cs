using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

public class PJ : MonoBehaviour
{
    public Inventory inventory;
    
    [Header("MOVEMENT")]
    public float playerSpeed = 1;
    public float playerRotationSpeed = 1f;
    public float playerRollFactor = 2f;
    public float rollCooldown;
    
    [Header("ATTACK")]
    public float attackImpulseFactor = 1.5f;
    public float combo1AttackCooldown;
    public float combo2AttackCooldown;
    public float combo3AttackCooldown;
    public float moveAfterAttackCooldown = 0.5f;
    public float playerRayMaxDistance = 0.5f;
    public float playerDustParticlesDelay = 0.5f;
    public float comboTimeWindow = 1.5f; // Ventana de tiempo para realizar el siguiente golpe del combo
    public int comboCount = 0; // Contador de golpes en el combo

    [Header("PLAYER DAMAGED")]
    public float deathFrameDuration = 0.5f;
    public float knockbackMovementFactor = 2f;
    public float damagedCamShakeIntensity = 2f;
    public float damagedCamShakeDuration = 0.3f;
    public float damageBlinkingDuration = 1f;
    public float spriteBlinkingFrecuency = 0.15f;
    private float damagedBlinkingCounter;
    private bool spriteBlinking;
    
    [Header("DEBUG")]
    public bool debugAttack;
    public bool canRoll;
    
    private ParticleSystem pjStepDust;
    private Animator pjAnimator;
    private SpriteRenderer pjSprite;
    
    private Quaternion initialPlayerRotation;
    private Vector3 lastDirection;

    private bool dustParticlesPlaying;
    private bool gameIn3D;
    private bool pjDoingAction;
    private bool pjIsRolling;
    private bool pjIsImpulsing;
    private bool rollReady = true;
    private bool attackReady = true;
    private float lastAttackInputTime;
    private bool bufferedAttack;
    private float initialPlayerRayMaxDistance;
    
    private Ray ray;
    private RaycastHit[] hits;
    private TweenerCore<Vector3, Vector3, VectorOptions> rollingTween;
    private Interactable interactableInContact;
    private bool pjInvulnerable;
    private bool beingDamaged;
    private bool stepReady = true;
    private Sequence damagedSequence;
    private Sequence impulseSequence;
    private float damagedGamepadVibrationIntensity = 1f;
    private float damagedGamepadVibrationDuration = 0.2f;

    #region Unity events
    
    private void Awake()
    {
        pjStepDust = GetComponentInChildren<ParticleSystem>();
        pjSprite = GetComponentInChildren<SpriteRenderer>();
        pjAnimator = GetComponentInChildren<Animator>();

        initialPlayerRotation = transform.rotation;
        initialPlayerRayMaxDistance = playerRayMaxDistance;

        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
    }

    private void Start()
    {
        //Set initial direction and rayhit
        lastDirection = transform.right;
        PjRaycastHit(Color.cyan);
    }

    public void DoUpdate(GameDirector.ControlInputData controlInputData)
    {
        UpdateLastDirection(controlInputData.movementDirection);
        UpdatePjRay();
        
        if (!pjDoingAction)
        {
            DoPjMovement(controlInputData);
            inventory.UpdatePosition(transform);
            
            PjDoRotation(controlInputData);
        } else
        {
            StopRollAndImpulseWhenHitWall();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (beingDamaged && (other.gameObject.layer == Layers.WALL_LAYER || other.gameObject.layer == Layers.DOOR_LAYER))
        {
            damagedSequence.Kill();
        }

        if (other.gameObject.layer == Layers.BOSS_COMBAT_LAYER)
        {
            Core.Event.Fire(new GameEvents.BossCombatReached(){});
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == Layers.INTERACTABLE_LAYER)
        {
            if (interactableInContact == null || !interactableInContact.IsInteracting())
            {
                if (interactableInContact == null)
                {
                    interactableInContact = other.GetComponent<Interactable>();
                    interactableInContact.SetOutlineVisibility(true);
                }
            }
        }
        
        if (beingDamaged && (other.gameObject.layer == Layers.WALL_LAYER || other.gameObject.layer == Layers.DOOR_LAYER))
        {
            damagedSequence.Kill();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == Layers.INTERACTABLE_LAYER)
        {
            if (interactableInContact != null && other.GetComponent<Interactable>() == interactableInContact)
            {
                interactableInContact.SetOutlineVisibility(false);
            }
            interactableInContact = null;
        }
    }

    #endregion

    #region Roll

    public void DoRoll()
    {
        if (!pjDoingAction && rollReady && canRoll)
        {
            pjDoingAction = true;
            pjIsRolling = true;
            pjInvulnerable = true;

            pjAnimator.Play("PJ_roll");
            Core.Audio.Play(SOUND_TYPE.PjDash, 1f, 0.1f, 0.01f);
            float animLength = Core.AnimatorHelper.GetAnimLength(pjAnimator, "PJ_roll");
            
            PjActionFalseWhenAnimFinish(animLength);

            Vector3 endPosition = GetRollEndPosition();
            
            Debug.DrawRay(transform.position, lastDirection, Color.red);
            if (!PjRaycastHit(Color.red) || !PjRayHitLayer(Layers.WALL_LAYER))
            {
                rollingTween = transform.DOMove(endPosition, animLength)
                    //.OnComplete(StopRolling)
                    .OnKill(StopRolling);

                var emissionModule = pjStepDust.emission;
                emissionModule.rateOverTime = 100;
                var mainModule = pjStepDust.main;
                mainModule.loop = true;
                pjStepDust.Play();
                dustParticlesPlaying = true;
            }
        }
    }
    
    private void StopRollAndImpulseWhenHitWall()
    {
        if (PjRaycastHit(Color.yellow) && (PjRayHitLayer(Layers.WALL_LAYER) || PjRayHitLayer(Layers.DOOR_LAYER)))
        {
            if (pjIsRolling)
            {
                rollingTween.Kill();
            }

            if (pjIsImpulsing)
            {
                impulseSequence.Kill();
            }
        }
    }

    private void StopRolling()
    {
        pjIsRolling = false;
        rollReady = false;

        Sequence invulnerableAfterDashSequence = DOTween.Sequence();

        invulnerableAfterDashSequence
            .AppendInterval(0.3f)
            .AppendCallback(() => pjInvulnerable = false);
        
        StartRollCoolown();
        var emissionModule = pjStepDust.emission;
        emissionModule.rateOverTime = 40;
        var mainModule = pjStepDust.main;
        mainModule.loop = false;
        pjStepDust.Stop();
        dustParticlesPlaying = false;
    }

    private Vector3 GetRollEndPosition()
    {
        Vector3 endDirection = lastDirection != Vector3.zero ? lastDirection.normalized : transform.right;
        endDirection *= playerRollFactor;
        var position = transform.position;
        Vector3 endPosition =
            new Vector3(position.x + endDirection.x, position.y + endDirection.y, position.z + endDirection.z);
        return endPosition;
    }

    private void StartRollCoolown()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(rollCooldown).AppendCallback(() => rollReady = true);
    }
    

    #endregion

    #region Movement, rotation and orientation

    private void PjDoRotation(GameDirector.ControlInputData controlInputData)
    {
        float mouseX = 0;
        float mouseY = 0;
        //Core.Input.Mouse.GetPositionDelta(out mouseX, out mouseY);

        if (controlInputData.cameraRotation.x != 0)
        {
            mouseX = controlInputData.cameraRotation.x * 2f;
        }
        else if (controlInputData.cameraMouseRotation.x != 0)
        {
            mouseX = controlInputData.cameraMouseRotation.x;
        }

        if (controlInputData.cameraRotation.y != 0)
        {
            mouseY = controlInputData.cameraRotation.y * 2f;
        }
        else if (controlInputData.cameraMouseRotation.y != 0)
        {
            mouseY = controlInputData.cameraMouseRotation.y;
        }
        
        mouseY = Mathf.Clamp(mouseY, -1f, 1f);
        
        Vector3 anglesIncrement = playerRotationSpeed * new Vector3(0, mouseX, 0);
        
        bool playerIsNotAttacking = inventory.GetActiveWeapon() == null || !inventory.GetActiveWeapon().IsCurrentlyAttacking();
        if (gameIn3D && playerIsNotAttacking)
        {
            transform.eulerAngles += anglesIncrement;
        }

        inventory.RotateItems(gameIn3D, lastDirection);
    }

    private void DoPjMovement(GameDirector.ControlInputData controlInputData)
    {
        Vector3 direction = controlInputData.movementDirection;
        
        bool directionIsZero = direction.x == 0 && direction.z == 0;
        if (!directionIsZero)
        {
            SetSpriteXOrientation(controlInputData.inputDirection.x);
            PlayDirectionalWalkAnimation(controlInputData.inputDirection);
            CreatePlayerDustParticles();
        }
        else
        {
            pjAnimator.Play("PJ_idle");
        }

        direction = FixDiagonalSpeedMovement(direction);

        if (PjRaycastHit(Color.blue))
        {
            if (PjRayHitLayer(Layers.WALL_LAYER) || PjRayHitLayer(Layers.DOOR_LAYER))
            {
                direction = Vector3.zero;
            }
        }

        if (stepReady && controlInputData.inputDirection != Vector3.zero)
        {
            Core.Audio.Play(SOUND_TYPE.PjStep, 1, 0.1f, 0.03f);

            stepReady = false;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.3f).AppendCallback(() => stepReady = true);
        }
        
        transform.position += direction * (Time.deltaTime * playerSpeed);
    }

    private void PlayDirectionalWalkAnimation(Vector3 direction)
    {
        // Top direction
        if (direction.x == 0 && direction.y == 1)
        {
            pjAnimator.Play("PJ_walk");
        }
        // Diagonal top direction
        else if (direction.x > 0 && direction.y > 0 ||
                 direction.x < 0 && direction.y > 0)
        {
            pjAnimator.Play("PJ_walk");
        }
        // Diagonal bottom direction
        else if (direction.x > 0 && direction.y < 0 ||
                 direction.x < 0 && direction.y < 0)
        {
            pjAnimator.Play("PJ_walk");
        }
        // Forward direction
        else if (direction.x > 0 && direction.y == 0 ||
                 direction.x < 0 && direction.y == 0)
        {
            pjAnimator.Play("PJ_walk");
        }
        // Bottom direction
        else if (direction.x == 0 && direction.y < 0)
        {
            pjAnimator.Play("PJ_walk");
        }
    }

    private static Vector3 FixDiagonalSpeedMovement(Vector3 direction)
    {
        bool directionIsDiagonal = direction.x != 0 && direction.z != 0;
        if (directionIsDiagonal)
        {
            direction = direction.normalized;
        }

        return direction;
    }
    
    private void UpdateLastDirection(Vector3 direction)
    {
        lastDirection = !pjDoingAction ? (direction != Vector3.zero ? direction : lastDirection) : lastDirection;
    }

    private void SetSpriteXOrientation(float xInputDirection)
    {
        bool flipX = false; // xInputDirection > 0
        
        if (xInputDirection == 0)
        {
            flipX = pjSprite.flipX;
        } else if (xInputDirection < 0)
        {
            flipX = true;
        }
        
        pjSprite.flipX = flipX;
    }

    #endregion

    #region Raycast
    
    private void UpdatePjRay()
    {
        ray = new Ray(transform.position, lastDirection);
    }

    private bool PjRayHitLayer(int layer)
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

    private bool PjRaycastHit(Color color)
    {
        Debug.DrawRay(ray.origin, ray.direction, color);
        hits = Physics.RaycastAll(ray.origin, ray.direction, playerRayMaxDistance);
        return hits.Length > 0;
    }

    #endregion

    #region Utils

    private void CreatePlayerDustParticles()
    {
        if (!dustParticlesPlaying)
        {
            dustParticlesPlaying = true;
            pjStepDust.Play();
            Sequence dustResetSequence = DOTween.Sequence();
            dustResetSequence.AppendInterval(playerDustParticlesDelay)
                .AppendCallback(() => dustParticlesPlaying = false);
        }
    }

    public void Switch2D3D(bool gameIn3D)
    {
        this.gameIn3D = gameIn3D;
        
        inventory.RestoreItemsRotation();

        Vector3 currentSpriteEulerAngles = pjSprite.transform.eulerAngles;
        
        if (gameIn3D)
        {
            pjSprite.transform.eulerAngles = new Vector3(0, currentSpriteEulerAngles.y, currentSpriteEulerAngles.z);
            playerRayMaxDistance = initialPlayerRayMaxDistance - 0.25f;
        }
        else
        {
            // Since in 3D the player rotates, it sets the initial rotation of the player
            transform.rotation = initialPlayerRotation;
            pjSprite.transform.eulerAngles = new Vector3(45, 0, 0);
            playerRayMaxDistance = initialPlayerRayMaxDistance + 0.25f;
        }
    }
    
    private void PjActionFalseWhenAnimFinish(float animLength)
    {
        Core.AnimatorHelper.DoOnAnimationFinish(animLength, () => { pjDoingAction = false; });
    }

    #endregion

    #region Main Action
    
    public void DoMainAction()
    {
        if (gameIn3D)
        {
            Interact();
        }
        else
        {
            if (!pjDoingAction && attackReady && (inventory.HasWeapon || debugAttack)) // Basic attack
            {
                if (Time.time - lastAttackInputTime > comboTimeWindow)
                {
                    comboCount = 0;
                }
                
                Attack();
            }
            else if (pjIsRolling) // Attack on dash Input Buffer
            {
                bufferedAttack = true;
                float animLength = Core.AnimatorHelper.GetAnimLength(pjAnimator, "PJ_roll");
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(animLength).AppendCallback((() => 
                {
                    if (bufferedAttack && !pjIsRolling && inventory.HasWeapon)
                    {
                        Attack();
                    }
                    bufferedAttack = false;
                }));
            }
            
            // Actualiza el tiempo de la última pulsación
            lastAttackInputTime = Time.time;
        }
    }

    private void Interact()
    {
        pjDoingAction = true;
        
        if (interactableInContact != null)
        { 
            PlayIdle();
            interactableInContact.Interact(this);
           
            if (!interactableInContact.IsInteracting())
            {
                pjDoingAction = false;
            }
        }
        else
        {
            pjDoingAction = false;
        }
    }

    private void Attack()
    {
        pjDoingAction = true;
        attackReady = false;

        if (Time.time - lastAttackInputTime > comboTimeWindow)
        {
            comboCount = 1;
        }
        else // Si está dentro del margen de tiempo, incrementa el contador del combo
        {
            comboCount++;
        }

        // Ejecuta la acción correspondiente al golpe del combo según el contador actual
        PerformComboAction(comboCount);
    }
    
    void PerformComboAction(int count)
    {
        switch (count)
        {
            case 1:
                Debug.Log("Golpe 1 del combo");
                ShowAttackFeedback(combo1AttackCooldown);
                break;
            case 2:
                Debug.Log("Golpe 2 del combo");
                ShowAttackFeedback(combo2AttackCooldown);
                break;
            case 3:
                Debug.Log("Golpe 3 del combo");
                ShowAttackFeedback(combo3AttackCooldown);
                // Aquí podrías ejecutar una acción especial o el golpe final del combo
                // Luego resetea el combo
                comboCount = 0;
                break;
            default:
                Debug.Log("Combo reseteado");
                break;
        }
    }

    private void ShowAttackFeedback(float comboAttackCooldown)
    {
        pjAnimator.Play("PJ_attack");
        Core.Audio.Play(SOUND_TYPE.SwordAttack, 1, 0.1f, 0.01f);

        PlayAttackImpulseAnimation();
        
        Weapon activeWeapon = inventory.GetActiveWeapon() != null ? inventory.GetActiveWeapon() : null;
        if (activeWeapon != null)
        {
            activeWeapon.DoAttack();
        }

        StartAttackCooldown(comboAttackCooldown);
    }

    private void StartAttackCooldown(float comboAttackCooldown)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(comboAttackCooldown).AppendCallback(() =>
        {
            attackReady = true;
            pjDoingAction = false;
        });
    }

    public void CollectItem(Item item)
    {
        inventory.AddItem(item);
    }
    
    #endregion

    public bool GetDamage(Transform damager)
    {
        bool damaged = false;
        
        if (!pjInvulnerable)
        {
            damaged = true;
            
            Core.Audio.Play(SOUND_TYPE.PjHitted, 1, 0, 0.5f);
            Core.Event.Fire(new GameEvents.PlayerDamaged(){deathFrameDuration = deathFrameDuration});
            
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(deathFrameDuration).AppendCallback(() =>
            {
                PlayDamagedKnockbackAnimation(damager);
                Core.GamepadVibrationService.SetControllerVibration(damagedGamepadVibrationIntensity, damagedGamepadVibrationDuration);
                Core.CameraEffects.ShakeCamera(damagedCamShakeIntensity, damagedCamShakeDuration);
                
            });
            
        }

        return damaged;
    }

    private void PlayDamagedKnockbackAnimation(Transform damager)
    {
        beingDamaged = true;
        damagedSequence = DOTween.Sequence();
        
        Vector3 position = transform.position;
        Vector3 enemyPosition = damager.position;
        Vector3 damagedDirection = (position - enemyPosition).normalized * knockbackMovementFactor;
        
        damagedSequence
            .Append(transform.DOMove(position + new Vector3(damagedDirection.x, position.y, damagedDirection.z), 0.2f))
            .OnComplete(() => { beingDamaged = false; });
        damagedSequence.Play();
        
        damagedBlinkingCounter = damageBlinkingDuration;
        StartCoroutine(SpriteBlinking());
    }
    
    private void PlayAttackImpulseAnimation()
    {
        Debug.DrawRay(transform.position, lastDirection, Color.green);
        if (!PjRaycastHit(Color.green) || !PjRayHitLayer(Layers.WALL_LAYER))
        {
            pjIsImpulsing = true;
            
            impulseSequence = DOTween.Sequence();
            
            Vector3 impulseDirection = lastDirection * attackImpulseFactor;

            impulseSequence
                .Append(transform.DOMove(transform.position + impulseDirection, 0.2f));
            impulseSequence.OnKill(() => { pjIsImpulsing = false; });
            impulseSequence.Play();
        }
    }

    IEnumerator SpriteBlinking()
    {
        pjInvulnerable = true;
        while (damagedBlinkingCounter > 0)
        {
            if (!spriteBlinking)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.AppendCallback(() => spriteBlinking = true)
                    .AppendCallback(() => pjSprite.enabled = false)
                    .AppendInterval(spriteBlinkingFrecuency)
                    .AppendCallback(() => pjSprite.enabled = true)
                    .AppendInterval(spriteBlinkingFrecuency)
                    .AppendCallback(() => spriteBlinking = false);
            }

            damagedBlinkingCounter -= Time.deltaTime;
            
            yield return null;
        }

        pjInvulnerable = false;
        pjSprite.color = Color.white;
        yield return null;
    }

    public void ResetItems()
    {
        inventory.ResetItems();
    }

    public void PlayIdle()
    {
        pjAnimator.Play("PJ_idle");
    }

    public void PlayWalk()
    {
        pjAnimator.Play("PJ_walk");
    }

    public void Rotate180()
    {
        transform.eulerAngles += new Vector3(0, 180, 0);
    }
}