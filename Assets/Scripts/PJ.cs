using System.Collections;
using DG.Tweening;
using Ju.Extensions;
using UnityEngine;

public class PJ : MonoBehaviour
{
    public Inventory inventory;
    
    [Header("MOVEMENT")]
    public float playerSpeed = 1;
    public float playerRotationSpeed = 1f;
    public float playerRollFactor = 2f;
    public float rollCooldown;
    public float invulnerableAfterDashCD = 0.3f;
    public float playerRayMaxDistance = 0.5f;
    
    [Header("ATTACK")]
    public float attackImpulseFactor = 1.5f;
    public float combo1AttackCooldown;
    public float combo2AttackCooldown;
    public float combo3AttackCooldown;
    public float moveAfterAttackCooldown = 0.5f;
    public float playerDustParticlesDelay = 0.5f;
    public float comboTimeWindow = 1.5f; // Ventana de tiempo para realizar el siguiente golpe del combo
    public int comboCount = 0; // Contador de golpes en el combo

    [Header("PLAYER DAMAGED")]
    public float deathFrameDuration = 0.5f;
    public float knockbackMovementFactor = 2f;
    public float damagedCamShakeIntensity = 2f;
    public float damageCamShakeFrequency = 0.3f;
    public float damagedCamShakeDuration = 0.3f;
    public float damageBlinkingDuration = 1f;
    public float spriteBlinkingFrecuency = 0.15f;
    private float damagedBlinkingCounter;
    private bool spriteBlinking;
    public float damagedGamepadVibrationIntensity = 1f;
    public float damagedGamepadVibrationDuration = 0.2f;
    
    [Header("DEBUG")]
    public bool debugAttack;
    public bool canRoll;
    
    private ParticleSystem pjStepDust;
    private Animator pjAnimator;
    private Animator pjWeaponAnimator;
    private Animator pjDashAnimator;
    private SpriteRenderer pjSprite;
    private SpriteRenderer pjAttackSprite;
    private SpriteRenderer pjDashSprite;
    private CapsuleCollider pjCollider;
    
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
    private bool pjRollingAttack;
    private float initialPlayerRayMaxDistance;
    
    private Ray ray;
    private RaycastHit[] hits;
    private Sequence rollingSequence;
    private Interactable interactableInContact;
    [HideInInspector] public bool pjInvulnerable;
    private bool pjIsBeingDamaged;
    private bool stepReady = true;
    private Sequence knockbackSequence;
    private Sequence impulseSequence;

    #region Unity events
    
    private void Awake()
    {
        pjStepDust = GetComponentInChildren<ParticleSystem>();
        pjSprite = GetComponentInChildren<SpriteRenderer>();
        pjAttackSprite = GetComponentsInChildren<SpriteRenderer>()[3];
        pjDashSprite = GetComponentsInChildren<SpriteRenderer>()[4];
        pjAnimator = GetComponentInChildren<Animator>();
        pjWeaponAnimator = GetComponentsInChildren<Animator>()[1];
        pjDashAnimator = GetComponentsInChildren<Animator>()[2];
        pjCollider = GetComponent<CapsuleCollider>();

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
        }
        
        StopMovementSequencesWhenHitWall();
    }

    private void OnTriggerEnter(Collider other)
    {
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

    public void DoRoll(GameDirector.ControlInputData controlInputData)
    {
        if (!pjIsRolling && rollReady && canRoll)
        {
            
            pjDoingAction = true;
            pjIsRolling = true;
            pjInvulnerable = true;

            pjCollider.isTrigger = true;
            inventory.GetActiveWeapon().attackingSequence.Kill();
            pjWeaponAnimator.Play("Empty");
            
            //Core.Audio.Play(SOUND_TYPE.PjDash, 1f, 0.1f, 0.01f);
            Core.Audio.PlayFMODAudio("event:/Characters/Player/Exploration/Dash", transform);
            
            float animLength = Core.AnimatorHelper.GetAnimLength(pjAnimator, "PJ_roll");
            
            PjActionFalseWhenAnimFinish(animLength);

            Vector3 endPosition = GetRollEndPosition(controlInputData);
            SetSpriteXOrientation(controlInputData.inputDirection.x);
            pjAnimator.Play("PJ_roll");
            GameObject dashFXInstance = Instantiate(pjDashAnimator.gameObject, transform.parent);
            dashFXInstance.transform.position = transform.position;

            RotateDashSprite(dashFXInstance.transform);

            Animator dashFXAnimator = dashFXInstance.GetComponent<Animator>();
            dashFXAnimator.Play("Roll");
            Destroy(dashFXInstance,  1);
            
            Debug.DrawRay(transform.position, lastDirection, Color.red);
            if (!PjRaycastHit(Color.red) || !PjRayHitLayer(Layers.WALL_LAYER))
            {
                rollingSequence = DOTween.Sequence();
                rollingSequence
                    .Append(transform.DOMove(endPosition, 0.2f))
                    .OnKill(StopRolling);

                var emissionModule = pjStepDust.emission;
                emissionModule.rateOverTime = 100;
                var mainModule = pjStepDust.main;
                mainModule.loop = true;
                pjStepDust.Play();
                dustParticlesPlaying = true;
            }
            else
            {
                StopRolling();
            }
            
        }
    }

    private void RotateDashSprite(Transform dashTransform)
    {
        // Top direction
        if (lastDirection.x == 0 && lastDirection.z == 1)
        {
            dashTransform.eulerAngles = new Vector3(80, -90, 0);
        }
        // Diagonal right-top direction
        else if (lastDirection.x > 0 && lastDirection.z > 0)
        {
            dashTransform.eulerAngles = new Vector3(45, -45, 0);
        }
        // Diagonal left-top direction
        else if (lastDirection.x < 0 && lastDirection.z > 0)
        {
            dashTransform.eulerAngles = new Vector3(45, 45, 0);
        }
        // Diagonal right-bottom direction
        else if (lastDirection.x > 0 && lastDirection.z < 0)
        {
            dashTransform.eulerAngles = new Vector3(45, 45, 0);
        }
        // Diagonal left-bottom direction
        else if (lastDirection.x < 0 && lastDirection.z < 0)
        {
            dashTransform.eulerAngles = new Vector3(45, -45, 0);
        }
        // Bottom direction
        else if (lastDirection.x == 0 && lastDirection.z < 0)
        {
            dashTransform.eulerAngles = new Vector3(80, 90, 0);
        }
    }

    private void StopMovementSequencesWhenHitWall()
    {
        if (PjRaycastHit(Color.yellow) && (PjRayHitLayer(Layers.WALL_LAYER) || PjRayHitLayer(Layers.DOOR_LAYER)))
        {
            if (pjIsRolling)
            {
                rollingSequence.Kill();
            }

            if (pjIsImpulsing)
            {
                impulseSequence.Kill();
            }

            if (pjIsBeingDamaged)
            {
                knockbackSequence.Kill();
            }
        }
    }

    private void StopRolling()
    {
        pjIsRolling = false;
        rollReady = false;
        
        pjCollider.isTrigger = false;

        Sequence invulnerableAfterDashSequence = DOTween.Sequence();

        invulnerableAfterDashSequence
            .AppendInterval(invulnerableAfterDashCD)
            .AppendCallback(() => pjInvulnerable = false);
        
        StartRollCoolown();
        var emissionModule = pjStepDust.emission;
        emissionModule.rateOverTime = 40;
        var mainModule = pjStepDust.main;
        mainModule.loop = false;
        pjStepDust.Stop();
        dustParticlesPlaying = false;
    }

    private Vector3 GetRollEndPosition(GameDirector.ControlInputData controlInputData)
    {
        Vector3 endDirection;
        Vector3 inputDirection = controlInputData.inputDirection;
        
        if (!GameState.gameIn3D && inputDirection != Vector3.zero)
        {
            Vector3 directionalInput = Get8DirecionInput(inputDirection);
            endDirection = new Vector3(directionalInput.normalized.x, 0, directionalInput.normalized.y);
        }
        else
        {
            endDirection = lastDirection != Vector3.zero ? lastDirection.normalized : transform.right;
        }
        
        endDirection *= playerRollFactor;
        var position = transform.position;

        Vector3 endPosition;
        if (GameState.gameIn3D)
        {
            endPosition = new Vector3(position.x + endDirection.x, 0, position.z + endDirection.z);
        }
        else
        {
            endPosition = new Vector3(position.x + endDirection.x, 0, position.z + endDirection.z);
        }
        return endPosition;
    }

    private Vector3 Get8DirecionInput(Vector3 inputDirection)
    {
        Vector3 directionalInput = new Vector3();
        // Top direction
        if (inputDirection.x == 0 && inputDirection.y == 1)
        {
            directionalInput = new Vector3(0, 1, 0);
        }
        // Diagonal right-top direction
        else if (inputDirection.x > 0 && inputDirection.y > 0)
        {
            directionalInput = new Vector3(1, 1, 0);
        }
        // Right-forward direction
        else if (inputDirection.x > 0 && inputDirection.y == 0)
        {
            directionalInput = new Vector3(1, 0, 0);
        }
        // Diagonal right-bottom direction
        else if (inputDirection.x > 0 && inputDirection.y < 0)
        {
            directionalInput = new Vector3(1, -1, 0);
        }
        // Bottom direction
        else if (inputDirection.x == 0 && inputDirection.y < 0)
        {
            directionalInput = new Vector3(0, -1, 0);
        }
        // Diagonal left-bottom direction
        else if (inputDirection.x < 0 && inputDirection.y < 0)
        {
            directionalInput = new Vector3(-1, -1, 0);
        }
        // Left-forward direction
        else if (inputDirection.x < 0 && inputDirection.y == 0)
        {
            directionalInput = new Vector3(-1, 0, 0);
        }
        // Diagonal left-top direction
        else if (inputDirection.x < 0 && inputDirection.y > 0)
        {
            directionalInput = new Vector3(-1, 1, 0);
        }

        return directionalInput;
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

        inventory.RotateItems(gameIn3D, controlInputData);
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

        if (PjRayHitLayer(Layers.WALL_LAYER, true) || PjRayHitLayer(Layers.DOOR_LAYER, true))
        {
            direction = Vector3.zero;
        }

        if (stepReady && controlInputData.inputDirection != Vector3.zero)
        {
            //Core.Audio.Play(SOUND_TYPE.PjStep, 1, 0.1f, 0.01f);
            Core.Audio.PlayFMODAudio("event:/Characters/Player/Exploration/Steps", transform);

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
            pjAnimator.Play("PJ_walk_up");
        }
        // Diagonal top direction
        else if (direction.x > 0 && direction.y > 0 ||
                 direction.x < 0 && direction.y > 0)
        {
            pjAnimator.Play("PJ_walk_diagonal_up");
        }
        // Forward direction
        else if (direction.x > 0 && direction.y == 0 ||
                 direction.x < 0 && direction.y == 0)
        {
            pjAnimator.Play("PJ_walk_forward");
        }
        // Diagonal bottom direction
        else if (direction.x > 0 && direction.y < 0 ||
                 direction.x < 0 && direction.y < 0)
        {
            pjAnimator.Play("PJ_walk_diagonal_down");
        }
        // Bottom direction
        else if (direction.x == 0 && direction.y < 0)
        {
            pjAnimator.Play("PJ_walk_down");
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
        lastDirection = !pjDoingAction && !pjIsBeingDamaged ? (direction != Vector3.zero ? direction : lastDirection) : lastDirection;
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
        pjAttackSprite.flipX = flipX;
        pjDashSprite.flipX = flipX;
    }

    #endregion

    #region Raycast
    
    private void UpdatePjRay()
    {
        ray = new Ray(transform.position, lastDirection);
    }

    private bool PjRayHitLayer(int layer, bool isConeRay = false)
    {
        bool layerHit = false;
        
        Debug.DrawRay(ray.origin, ray.direction, Color.blue);
        hits = Physics.RaycastAll(ray.origin, ray.direction, playerRayMaxDistance);
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.layer == layer)
            {
                layerHit = true;
            }
        }

        if (isConeRay)
        {
            layerHit = RaycastRotatedDirection(layer, layerHit, 45f);
            layerHit = RaycastRotatedDirection(layer, layerHit, -45f);
        }

        return layerHit;
    }

    private bool RaycastRotatedDirection(int layer, bool layerHit, float angle)
    {
        Vector3 rotatedDirection = Quaternion.Euler(0, angle, 0) * ray.direction;
        Ray coneRay = new Ray(transform.position, rotatedDirection);
        Debug.DrawRay(coneRay.origin, coneRay.direction * initialPlayerRayMaxDistance, Color.red);
        RaycastHit[] coreHits = Physics.RaycastAll(coneRay.origin, coneRay.direction, playerRayMaxDistance);

        foreach (RaycastHit hit in coreHits)
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
        return PjRaycastHit(color, playerRayMaxDistance);
    }
    
    private bool PjRaycastHit(Color color, float playerRayDistance)
    {
        Debug.DrawRay(ray.origin, ray.direction, color);
        hits = Physics.RaycastAll(ray.origin, ray.direction, playerRayDistance);
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
            Core.AnimatorHelper.DoOnAnimationFinish(animLength, () =>
            {
                if (!pjRollingAttack)
                {
                    pjDoingAction = false;
            }   
            });
    }

    #endregion

    #region Main Action
    
    public void DoMainAction()
    {
        if (gameIn3D)
        {
            if (GameState.combatDemo)
            {
                TryAttack();
            }
            else
            {
                Interact();
            }
        }
        else
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (!pjDoingAction && attackReady && (inventory.HasWeapon || debugAttack)) // Basic attack
        {
            Attack();
        }
        else if (pjIsRolling) // Attack on dash Input Buffer
        {
            pjRollingAttack = true;
            
            Core.Audio.PlayFMODAudio("event:/Characters/Player/Combat/Weapons/Sword1_Attack1", transform);
            
            pjAnimator.Play("PJ_rollattack");
            pjWeaponAnimator.Play("RollAttack");

            float animLength = Core.AnimatorHelper.GetAnimLength(pjAnimator, "PJ_rollattack");

            Weapon activeWeapon = inventory.GetActiveWeapon() != null ? inventory.GetActiveWeapon() : null;
            if (activeWeapon != null)
            {
                activeWeapon.DoAttack();
            }
            
            Sequence dashAttackSequence = DOTween.Sequence();
            dashAttackSequence
                .AppendInterval(animLength)
                .AppendCallback(() =>
                {
                    comboCount = 0;
                    pjRollingAttack = false;
                    pjDoingAction = false;
                });
        }

        lastAttackInputTime = Time.time;
    }

    private bool Interact()
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

        return pjDoingAction;
    }

    private void Attack()
    {
        if (Time.time - lastAttackInputTime > comboTimeWindow)
        {
            comboCount = 0;
        }
        
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
                ShowAttackFeedback(combo1AttackCooldown, count);
                break;
            case 2:
                ShowAttackFeedback(combo2AttackCooldown, count);
                break;
            case 3:
                ShowAttackFeedback(combo3AttackCooldown, count);
                // Aquí podrías ejecutar una acción especial o el golpe final del combo
                // Luego resetea el combo
                comboCount = 0;
                break;
            default:
                break;
        }
    }

    private void ShowAttackFeedback(float comboAttackCooldown, int comboIndex)
    {
        pjAnimator.Play("PJ_attack" + comboIndex);
        pjWeaponAnimator.Play("Attack" + comboIndex);
        //Core.Audio.Play(SOUND_TYPE.SwordAttack, 1, 0.1f, 0.01f);
        Core.Audio.PlayFMODAudio("event:/Characters/Player/Combat/Weapons/Sword1_Attack1", transform);

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
        Vector3 damagerPosition = damager.position;
        bool damaged = false;
        
        if (!pjInvulnerable)
        {
            pjInvulnerable = true;
            damaged = true;
            
            //Core.Audio.Play(SOUND_TYPE.PjHitted, 1, 0, 0.5f);
            Core.Audio.PlayFMODAudio("event:/Characters/Player/Combat/GetImpact", transform);
            Core.Audio.PlayFMODAudio("event:/IngameUI/TimeLoop/Timeloop_MoveFordward", transform);
            Core.Event.Fire(new GameEvents.PlayerDamaged(){deathFrameDuration = deathFrameDuration});
            Core.GamepadVibrationService.SetControllerVibration(damagedGamepadVibrationIntensity, damagedGamepadVibrationDuration);
            
            Sequence deathFrameSequence = DOTween.Sequence();
            deathFrameSequence.AppendInterval(deathFrameDuration).AppendCallback(() =>
            {
                PlayDamagedKnockbackAnimation(damagerPosition);
                Core.CameraEffects.StartShakingEffect(damagedCamShakeIntensity, damageCamShakeFrequency, damagedCamShakeDuration);
                
            });
            
        }

        return damaged;
    }

    private void PlayDamagedKnockbackAnimation(Vector3 damagerPosition)
    {
        Vector3 position = transform.position;
        Vector3 enemyPosition = damagerPosition;
        Vector3 damagedDirection = (position - enemyPosition).normalized * knockbackMovementFactor;
        var previousLastDirection = lastDirection;
        lastDirection = damagedDirection;
        UpdatePjRay();
        
        if (!PjRaycastHit(Color.blue, 1.5f) || !PjRayHitLayer(Layers.WALL_LAYER))
        {
            lastDirection = previousLastDirection;
            pjIsBeingDamaged = true;
            knockbackSequence = DOTween.Sequence();
            
            knockbackSequence
                .Append(transform.DOMove(position + new Vector3(damagedDirection.x, 0, damagedDirection.z), 0.2f))
                .OnKill(() => { pjIsBeingDamaged = false; });
        }
        
        damagedBlinkingCounter = damageBlinkingDuration;
        StartCoroutine(SpriteBlinking());
    }
    
    private void PlayAttackImpulseAnimation()
    {
        Debug.DrawRay(transform.position, lastDirection, Color.blue);
        if (!PjRaycastHit(Color.blue) || !PjRayHitLayer(Layers.WALL_LAYER))
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

    public void SetDoingAction(bool value)
    {
        pjDoingAction = value;
    }
}