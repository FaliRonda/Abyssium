using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Extensions;
using UnityEngine;

public class PJ : MonoBehaviour
{
    public Inventory inventory;
    public float playerSpeed = 1;
    public float playerRotationSpeed = 1f;
    public float playerRollFactor = 2f;
    public float rollCooldown;
    public bool debugAttack;
    public bool canRoll;
    public float attackCooldown;
    public float playerRayMaxDistance = 0.5f;
    public float playerDustParticlesDelay = 0.5f;
    public float damageBlinkingDuration = 1f;
    public float spriteBlinkingFrecuency = 0.15f;
    
    private float damagedBlinkingCounter;
    private bool spriteBlinking;
    
    private ParticleSystem pjStepDust;
    private Animator pjAnimator;
    private SpriteRenderer pjSprite;
    
    private Quaternion initialPlayerRotation;
    private Vector3 lastDirection;

    private bool dustParticlesPlaying;
    private bool gameIn3D;
    private bool pjDoingAction;
    private bool pjIsRolling;
    private bool rollReady = true;
    private bool attackReady = true;
    private bool bufferedAttack;
    private float initialPlayerRayMaxDistance;
    
    private Ray ray;
    private RaycastHit hit;
    private TweenerCore<Vector3, Vector3, VectorOptions> rollingTween;
    private Interactable interactableInContact;
    private bool pjInvulnerable;
    private bool beingDamaged;
    private bool stepReady = true;
    private Sequence damagedSequence;

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
            
            PjDoRotation(controlInputData.cameraRotation);
        } else
        {
            StopRollingWhenHitWall();
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

    public void DoRoll(Vector3 direction)
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
    
    private void StopRollingWhenHitWall()
    {
        if (pjIsRolling && PjRaycastHit(Color.yellow) && (PjRayHitLayer(Layers.WALL_LAYER) || PjRayHitLayer(Layers.DOOR_LAYER)))
        {
            rollingTween.Kill();
        }
    }

    private void StopRolling()
    {
        pjIsRolling = false;
        rollReady = false;
        pjInvulnerable = false;
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

    private void PjDoRotation(Vector2 cameraRotation)
    {
        float mouseX = 0;
        float mouseY = 0;
        //Core.Input.Mouse.GetPositionDelta(out mouseX, out mouseY);

        if (cameraRotation.x != 0)
        {
            mouseX = cameraRotation.x;
        }

        if (cameraRotation.y != 0)
        {
            mouseY = cameraRotation.y;
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
            pjAnimator.Play("PJ_walk");
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
        return hit.transform.gameObject.layer == layer;
    }

    private bool PjRaycastHit(Color color)
    {
        Debug.DrawRay(ray.origin, ray.direction);
        return Physics.Raycast(ray, out hit, playerRayMaxDistance);
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
        
        pjAnimator.Play("PJ_attack");
        Core.Audio.Play(SOUND_TYPE.SwordAttack, 1, 0.1f, 0.01f);
        
        float animLength = Core.AnimatorHelper.GetAnimLength(pjAnimator, "PJ_attack");

        Weapon activeWeapon = inventory.GetActiveWeapon() != null ? inventory.GetActiveWeapon() : null;
        if (activeWeapon != null)
        {
            activeWeapon.DoAttack();
        }

        PjActionFalseWhenAnimFinish(animLength);
        attackReady = false;
        StartAttackCooldown();
        
    }

    private void StartAttackCooldown()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(attackCooldown).AppendCallback(() => attackReady = true);
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
            float deathFrameDuration = 1f;
            
            Core.Event.Fire(new GameEvents.PlayerDamaged(){deathFrameDuration = deathFrameDuration});
            PlayDamagedAnimation(damager);
            Core.CameraEffects.ShakeCamera(2, 0.3f);
            
            Core.Audio.Play(SOUND_TYPE.PjDamaged, 1, 0.1f, 0.03f);
        }

        return damaged;
    }

    private void PlayDamagedAnimation(Transform damager)
    {
        beingDamaged = true;
        damagedSequence = DOTween.Sequence();
        
        Vector3 position = transform.position;
        Vector3 enemyPosition = damager.position;
        Vector3 damagedDirection = (position - enemyPosition).normalized * 2f;
        
        damagedSequence
            .Append(transform.DOMove(position + new Vector3(damagedDirection.x, position.y, damagedDirection.z), 0.2f))
            .OnComplete(() => { beingDamaged = false; });
        damagedSequence.Play();
        
        damagedBlinkingCounter = damageBlinkingDuration;
        StartCoroutine(SpriteBlinking());
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

    public void Rotate180()
    {
        transform.eulerAngles += new Vector3(0, 180, 0);
    }
}