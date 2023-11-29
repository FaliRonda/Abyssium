using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Extensions;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class PJ : MonoBehaviour
{
    public Inventory inventory;
    public float playerSpeed = 1;
    public float playerRotationSpeed = 1f;
    public float playerRollFactor = 2f;
    public float rollCooldown;
    public float attackCooldown;
    public float playerRayMaxDistance = 0.5f;
    public float playerDustParticlesDelay = 0.5f;
    
    private ParticleSystem pjStepDust;
    private Animator pjAnim;
    private SpriteRenderer pjSprite;
    
    private Quaternion initialPlayerSpriteRotation;
    private Quaternion initialPlayerRotation;
    private Vector3 lastDirection;

    private bool dustParticlesPlaying;
    private bool gameIn3D;
    private bool pjDoingAction;
    private bool pjIsRolling;
    private bool rollReady = true;
    private bool attackReady = true;
    private bool bufferedAttack;
    
    private Ray ray;
    private RaycastHit hit;
    private TweenerCore<Vector3, Vector3, VectorOptions> rollingTween;
    private Interactable interactableInContact;

    #region Unity events
    
    private void Awake()
    {
        pjStepDust = GetComponentInChildren<ParticleSystem>();
        pjSprite = GetComponentInChildren<SpriteRenderer>();
        pjAnim = GetComponentInChildren<Animator>();

        initialPlayerRotation = transform.rotation;
        initialPlayerSpriteRotation = pjSprite.transform.rotation;
        
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

    public void DoRoll(Vector3 direction)
    {
        if (!pjDoingAction && rollReady)
        {
            pjDoingAction = true;
            pjIsRolling = true;

            pjAnim.Play("PJ_roll");
            float animLenght = Core.AnimatorHelper.GetAnimLenght(pjAnim, "PJ_roll");
            
            PjActionFalseWhenAnimFinish(animLenght);

            Vector3 endPosition = GetRollEndPosition();
            
            Debug.DrawRay(transform.position, lastDirection, Color.red);
            if (!PjRaycastHit(Color.red) || !PjRayHitLayer(Layers.WALL_LAYER))
            {
                rollingTween = transform.DOMove(endPosition, animLenght)
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
            pjAnim.Play("PJ_walk");
            CreatePlayerDustParticles();
        }
        else
        {
            pjAnim.Play("PJ_idle");
        }

        direction = FixDiagonalSpeedMovement(direction);

        if (PjRaycastHit(Color.blue))
        {
            if (PjRayHitLayer(Layers.WALL_LAYER) || PjRayHitLayer(Layers.DOOR_LAYER))
            {
                direction = Vector3.zero;
            }
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
        Debug.DrawRay(ray.origin, ray.direction, color);
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
        
        if (gameIn3D)
        {
            pjSprite.transform.Rotate(new Vector3(-45, 0, 0));
        }
        else
        {
            transform.rotation = initialPlayerRotation;
            pjSprite.transform.rotation = initialPlayerSpriteRotation;
        }
    }
    
    private void PjActionFalseWhenAnimFinish(float animLenght)
    {
        Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () => { pjDoingAction = false; });
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
            if (!pjDoingAction && attackReady) // Basic attack
            {
                Attack();
            }
            else if (pjIsRolling) // Attack on dash Input Buffer
            {
                bufferedAttack = true;
                float animLenght = Core.AnimatorHelper.GetAnimLenght(pjAnim, "PJ_roll");
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(animLenght).AppendCallback((() => 
                {
                    if (bufferedAttack && !pjIsRolling)
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
        
        pjAnim.Play("PJ_attack");
        
        float animLenght = Core.AnimatorHelper.GetAnimLenght(pjAnim, "PJ_attack");

        Weapon activeWeapon = inventory.GetActiveWeapon() != null ? inventory.GetActiveWeapon() : null;
        if (activeWeapon != null)
        {
            activeWeapon.DoAttack();
        }

        PjActionFalseWhenAnimFinish(animLenght);
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

    public void GetDamage()
    {
        // PLay damaged anim
        Core.Event.Fire<GameEvents.PlayerDamaged>();
    }
}