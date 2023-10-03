using Ju.Input;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PJ : MonoBehaviour
{
    public Inventory inventory;
    public float playerSpeed = 1;
    public float playerRotationSpeed = 1f;

    private Quaternion initialPlayerSpriteRotation;
    private Quaternion initialPlayerRotation;

    private SpriteRenderer pjSprite;
    private Light2D pjPointLight;
    private BoxCollider pjCollider;

    private float pjColliderInitialOffset;

    private Animator pjAnim;

    private bool _gameIn3D;

    private void Awake()
    {
        pjSprite = GetComponentInChildren<SpriteRenderer>();
        pjPointLight = GetComponentInChildren<Light2D>();
        pjCollider = GetComponent<BoxCollider>();

        pjColliderInitialOffset = pjCollider.center.z;
        
        pjAnim = GetComponentInChildren<Animator>();

        initialPlayerRotation = transform.rotation;
        initialPlayerSpriteRotation = pjSprite.transform.rotation;
    }

    public void DoMovement(Vector3 direction)
    {
        if (!inventory.GetActiveWeapon().IsCurrentlyAttacking())
        {
            if (direction.x != 0 || direction.z != 0)
            {
                if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.RightArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.D))
                {
                    pjSprite.flipX = false;
                }
                
                if (Core.Input.Keyboard.IsKeyHeld(KeyboardKey.LeftArrow) || Core.Input.Keyboard.IsKeyHeld(KeyboardKey.A))
                {
                    pjSprite.flipX = true;
                }

                pjAnim.Play("PJ_run");
            }
            else
            {
                pjAnim.Play("PJ_idle");
            }

            if (direction.x != 0 && direction.z != 0)
            {
                direction.x *= 0.75f;
                direction.z *= 0.75f;
            }
            transform.position += direction * (Time.deltaTime * playerSpeed);
            inventory.UpdatePosition(transform);
        }

    }

    public void DoRotation(bool gameIn3D, Vector3 direction)
    {
        _gameIn3D = gameIn3D;
        if (!inventory.GetActiveWeapon().IsCurrentlyAttacking())
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
    }

    public void Switch2D3D(bool gameIn3D)
    {
        _gameIn3D = gameIn3D;
        inventory.RestoreItemsRotation();
        
        if (gameIn3D)
        {
            pjSprite.transform.Rotate(new Vector3(-90, 0, 0));
            
            var pjColliderCenter = pjCollider.center;
            pjColliderCenter = new Vector3(pjColliderCenter.x, pjColliderCenter.y, 0);
            pjCollider.center = pjColliderCenter;
        }
        else
        {
            transform.rotation = initialPlayerRotation;
            pjSprite.transform.rotation = initialPlayerSpriteRotation;
            
            var pjColliderCenter = pjCollider.center;
            pjColliderCenter = new Vector3(pjColliderCenter.x, pjColliderCenter.y, pjColliderInitialOffset);
            pjCollider.center = pjColliderCenter;
        }
        
    }

    public void Attack()
    {
        pjAnim.Play("PJ_attack");
        
        WeaponDamage activeWeapon = inventory.GetActiveWeapon();
        activeWeapon.Attack();
    }
}
