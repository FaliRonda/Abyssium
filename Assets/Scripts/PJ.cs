using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Ju.Extensions;
using Ju.Input;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PJ : MonoBehaviour
{
    public Inventory inventory;
    public float playerSpeed = 1;
    public float playerRotationSpeed = 1f;
    public float playerRollFactor = 2f;
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
    
    private Ray ray;
    private RaycastHit hit;
    private TweenerCore<Vector3, Vector3, VectorOptions> rollingTween;

    
    private void Awake()
    {
        pjStepDust = GetComponentInChildren<ParticleSystem>();
        pjSprite = GetComponentInChildren<SpriteRenderer>();
        pjAnim = GetComponentInChildren<Animator>();

        initialPlayerRotation = transform.rotation;
        initialPlayerSpriteRotation = pjSprite.transform.rotation;
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

    private void StopRollingWhenHitWall()
    {
        if (pjIsRolling && PjRaycastHit(Color.yellow) && PjRayHitWall())
        {
            rollingTween.Kill();
        }
    }
    
    private void PjDoRotation(Vector3 direction)
    {
        float mouseX;
        float mouseY;
        Core.Input.Mouse.GetPositionDelta(out mouseX, out mouseY);
        Vector3 anglesIncrement = playerRotationSpeed * new Vector3(0, mouseX, 0);

        if (gameIn3D && !inventory.GetActiveWeapon().IsCurrentlyAttacking())
        {
            transform.eulerAngles += anglesIncrement;
        }

        inventory.RotateItems(gameIn3D, direction);
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

        if (PjRaycastHit(Color.blue) && PjRayHitWall())
        {
            direction = Vector3.zero;
        }

        transform.position += direction * (Time.deltaTime * playerSpeed);
    }

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

    private static Vector3 FixDiagonalSpeedMovement(Vector3 direction)
    {
        bool directionIsDiagonal = direction.x != 0 && direction.z != 0;
        if (directionIsDiagonal)
        {
            direction = direction.normalized;
        }

        return direction;
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

    private void UpdatePjRay()
    {
        ray = new Ray(transform.position, lastDirection);
    }

    private void UpdateLastDirection(Vector3 direction)
    {
        lastDirection = !pjDoingAction ? (direction != Vector3.zero ? direction : lastDirection) : lastDirection;
    }

    private bool PjRayHitWall()
    {
        return hit.transform.gameObject.layer == Layers.WALL_LAYER;
    }

    private bool PjRaycastHit(Color color)
    { 
        Debug.DrawRay(transform.position, lastDirection.normalized, color);
        return Physics.Raycast(ray, out hit, playerRayMaxDistance);
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

    public void Attack()
    {
        if (!pjDoingAction)
        {
            pjDoingAction = true;
            
            pjAnim.Play("PJ_attack");
            float animLenght = GetPlayerAnimLenght("PJ_attack");
            
            WeaponDamage activeWeapon = inventory.GetActiveWeapon();
            activeWeapon.Attack();
            
            
            PjActionFalseWhenAnimFinish(animLenght);
        }
    }

    private void PjActionFalseWhenAnimFinish(float animLenght)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(animLenght)
            .AppendCallback(() => { pjDoingAction = false; });
    }

    public void Roll(Vector3 direction)
    {
        if (!pjDoingAction)
        {
            pjDoingAction = true;
            pjIsRolling = true;

            pjAnim.Play("PJ_roll");
            float animLenght = GetPlayerAnimLenght("PJ_roll");
            
            PjActionFalseWhenAnimFinish(animLenght);

            Vector3 endPosition = GetRollEndPosition();
            
            Debug.DrawRay(transform.position, lastDirection, Color.red);
            if (!PjRaycastHit(Color.red) || !PjRayHitWall())
            {
                rollingTween = transform.DOMove(endPosition, animLenght)
                    .OnComplete(StopRolling)
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

    private void StopRolling()
    {
        pjIsRolling = false;
        
        var emissionModule = pjStepDust.emission;
        emissionModule.rateOverTime = 40;
        var mainModule = pjStepDust.main;
        mainModule.loop = false;
        pjStepDust.Stop();
        dustParticlesPlaying = false;
    }

    private float GetPlayerAnimLenght(String animName)
    {
        return pjAnim.runtimeAnimatorController.animationClips.Find(element => element.name == animName).length;
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
}
