using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Extensions;
using Ju.Input;
using UnityEngine;

public class PJ : MonoBehaviour
{
    public Inventory inventory;
    public float playerSpeed = 1;
    public float playerRotationSpeed = 1f;
    public float playerRollFactor = 2f;
    public float rollCooldown;
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

    public void DoUpdate(Vector3 direction)
    {
        UpdateLastDirection(direction);
        UpdatePjRay();
        
        if (!pjDoingAction)
        {
            DoPjMovement(direction);
            inventory.UpdatePosition(transform);
            
            PjDoRotation(lastDirection);
        } else
        {
            StopRollingWhenHitWall();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layers.INTERACTABLE_LAYER)
        {
            if (interactableInContact == null || !interactableInContact.IsInteracting())
            {
                interactableInContact = other.GetComponent<Interactable>();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == Layers.INTERACTABLE_LAYER)
        {
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
        startRollCoolown();
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

    private void startRollCoolown()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(rollCooldown).AppendCallback(() => rollReady = true);
        Debug.Log("Ready to dash again!");
    }
    

    #endregion

    #region Movement, rotation and orientation

    private void PjDoRotation(Vector3 direction)
    {
        float mouseX;
        float mouseY;
        Core.Input.Mouse.GetPositionDelta(out mouseX, out mouseY);
        mouseY = Mathf.Clamp(mouseY, -1f, 1f);
        Vector3 anglesIncrement = playerRotationSpeed * new Vector3(0, mouseX, 0);

        if (gameIn3D && (inventory.GetActiveWeapon() == null || !inventory.GetActiveWeapon().IsCurrentlyAttacking()))
        {
            transform.eulerAngles += anglesIncrement;
        }

        inventory.RotateItems(gameIn3D, lastDirection);
    }

    private void DoPjMovement(Vector3 direction)
    {
        bool directionIsZero = direction.x == 0 && direction.z == 0;
        if (!directionIsZero)
        {
            SetSpriteXOrientation();
            pjAnim.Play("PJ_run");
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

    private void SetSpriteXOrientation()
    {
        if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.RightArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.D))
        {
            pjSprite.flipX = false;
        }

        if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.LeftArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.A))
        {
            pjSprite.flipX = true;
        }
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
            if (!pjDoingAction)
            {
                Attack();
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
        
        //He cambiado el nombre de referencia para atender al nuevo.
        float animLenght = Core.AnimatorHelper.GetAnimLenght(pjAnim, "PJ_attack1");

        Weapon activeWeapon = inventory.GetActiveWeapon() != null ? inventory.GetActiveWeapon() : null;
        if (activeWeapon != null)
        {
            activeWeapon.Attack();
        }

        PjActionFalseWhenAnimFinish(animLenght);
    }

    private void PjActionFalseWhenAnimFinish(float animLenght)
    {
        Core.AnimatorHelper.DoOnAnimationFinish(animLenght, () =>
        {
            pjDoingAction = false;
        });
    }

    #endregion

    public void CollectItem(Item item)
    {
        inventory.AddItem(item);
    }
}
