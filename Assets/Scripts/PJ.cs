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

    private void Awake()
    {
        pjSprite = GetComponentInChildren<SpriteRenderer>();
        pjPointLight = GetComponentInChildren<Light2D>();

        initialPlayerRotation = transform.rotation;
        initialPlayerSpriteRotation = pjSprite.transform.rotation;
    }

    public void DoMovement(Vector3 direction)
    {
        if (!inventory.GetActiveWeapon().IsCurrentlyAttacking())
        {
            transform.position += direction * (Time.deltaTime * playerSpeed);
            inventory.UpdatePosition(transform);
        }

    }

    public void DoRotation(bool gameIn3D, Vector3 direction)
    {
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
        inventory.RestoreItemsRotation();
        
        if (gameIn3D)
        {
            pjSprite.transform.Rotate(new Vector3(-90, 0, 0));
        }
        else
        {
            transform.rotation = initialPlayerRotation;
            pjSprite.transform.rotation = initialPlayerSpriteRotation;
        }
        
    }

    public void Attack()
    {
        WeaponDamage activeWeapon = inventory.GetActiveWeapon();
        activeWeapon.Attack();
    }
}
