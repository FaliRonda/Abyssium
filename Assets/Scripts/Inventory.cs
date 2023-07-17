using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [NonSerialized]
    public List<GameObject> items = new List<GameObject>();

    private List<Quaternion> initialRotations = new List<Quaternion>();
    private WeaponDamage activeWeapon;
    
    private void Start()
    {
        bool activeWeaponFilled = false;
        
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            items.Add(t.gameObject);
            initialRotations.Add(t.localRotation);

            if (!activeWeaponFilled && t.gameObject.layer == Layers.WEAPON_LAYER)
            {
                activeWeapon = t.GetComponent<WeaponDamage>();
                activeWeaponFilled = true;
            }
        }
    }

    public void UpdatePosition(Transform playerTransform)
    {
        transform.position = playerTransform.position;
    }

    public void RotateItems(bool gameIn3D, Vector3 direction)
    {
        foreach (GameObject item in items)
        {
            switch (item.layer)
            {
                case Layers.WEAPON_LAYER:
                    RotateLight(item, direction, gameIn3D);
                    break;
                case Layers.LIGHT_LAYER:
                    RotateLight(item, direction, gameIn3D);
                    break;
                default:
                    break;
            }
        }
    }

    private void RotateLight(GameObject item, Vector3 direction, bool gameIn3D)
    {
        if (!gameIn3D)
        {
            item.transform.DORotate(new Vector3(90, 0, CalculateLightRotationAngle(direction)), 0.5f);
        }
        else
        {
            float mouseX;
            float mouseY;
            
            Core.Input.Mouse.GetPositionDelta(out mouseX, out mouseY);
            item.transform.eulerAngles += new Vector3(0, 0,-mouseX);
        }
    }

    private float CalculateLightRotationAngle(Vector3 direction)
    {
        float angle = 0;

        if (direction.x != 0)
        {
            angle = direction.x * -90;
        }
        else if (direction.z < 0)
        {
            angle = -180;
        }

        return angle;
    }

    private void RotateWeapon(GameObject item, Vector3 direction, bool gameIn3D)
    {
        
    }

    public void RestoreItemsRotation()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].transform.localRotation = initialRotations[i];
        }
    }

    public WeaponDamage GetActiveWeapon()
    {
        return activeWeapon;
    }
}
