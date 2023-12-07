using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [NonSerialized]
    public List<GameObject> items = new List<GameObject>();

    private List<Quaternion> initialRotations = new List<Quaternion>();
    private Weapon activeWeapon;

    private bool hasWhiteOrb = false;
    private bool hasBlackOrb = false;
    private bool hasLantern = false;
    private bool hasWeapon;
    private bool hasNPCMemory = false;
    
    public bool HasWhiteOrb => hasWhiteOrb;
    public bool HasBlackOrb => hasBlackOrb;
    public bool HasLantern => hasLantern;
    public bool HasWeapon => hasWeapon;
    public bool HasNPCMemory => hasNPCMemory;

    private Light lantern;

    private void Start()
    {
        lantern = GetComponentInChildren<Light>();
        lantern.enabled = hasLantern;
        
        bool activeWeaponFilled = false;
        
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            items.Add(t.gameObject);
            initialRotations.Add(t.localRotation);

            if (!activeWeaponFilled && t.gameObject.layer == Layers.WEAPON_LAYER)
            {
                activeWeapon = t.GetComponent<Weapon>();
                if (activeWeapon != null)
                {
                    activeWeaponFilled = true;
                }
            }
        }
    }

    public void UpdatePosition(Transform playerTransform)
    {
        Vector3 position = playerTransform.position;
        transform.position = new Vector3(position.x, transform.position.y, position.z);
    }

    public void RotateItems(bool gameIn3D, Vector3 direction)
    {
        foreach (GameObject item in items)
        {
            switch (item.layer)
            {
                case Layers.WEAPON_LAYER:
                    RotateWeapon(item, direction, gameIn3D);
                    break;
                case Layers.LIGHT_LAYER:
                    RotateLight(item, direction, gameIn3D);
                    break;
                default:
                    break;
            }
        }
    }

    private void RotateItem(float xRotation, float yRotation, float zRotation, GameObject item, bool gameIn3D)
    {
        if (item.gameObject.name.Contains("pivot"))
        {
            if (!gameIn3D)
            {
                item.transform.DORotate(new Vector3(xRotation, yRotation, zRotation), 0.2f);
            }
            else
            {
                Vector3 cameraRotation = Camera.main.transform.rotation.eulerAngles;
                item.transform.DORotate(new Vector3(cameraRotation.x, cameraRotation.y, cameraRotation.z), 0.2f);
                /*
                 float mouseX;
                float mouseY;
                
                Core.Input.Mouse.GetPositionDelta(out mouseX, out mouseY);
                item.transform.eulerAngles += new Vector3(0, mouseX,0);
                */
            }
        }
    }
    
    private void RotateLight(GameObject item, Vector3 direction, bool gameIn3D)
    {
        RotateItem(0, -CalculateRotationAngle(direction), 0, item, gameIn3D);
    }

    private void RotateWeapon(GameObject item, Vector3 direction, bool gameIn3D)
    {
        RotateItem(0, -CalculateRotationAngle(direction), 0, item, gameIn3D);
    }

    private float CalculateRotationAngle(Vector3 direction)
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
    
    public void RestoreItemsRotation()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].transform.localRotation = initialRotations[i];
        }
    }

    public Weapon GetActiveWeapon()
    {
        return activeWeapon;
    }

    public void AddItem(Item item)
    {
        switch (item.itemType)
        {
            case Item.ITEM_TYPE.WEAPON:
                hasWeapon = true;
                activeWeapon.UptadeWeaponStats(item);
                break;
            case Item.ITEM_TYPE.UTIL:
                hasLantern = true;
                lantern.enabled = true;
                break;
            case Item.ITEM_TYPE.KEY:
                if (item.keyId == Item.KEY_IDS.BLACK_ORB)
                {
                    Core.Event.Fire(new GameEvents.OrbGot(){});
                    hasBlackOrb = true;
                } else if (item.keyId == Item.KEY_IDS.WHITE_ORB)
                {
                    Core.Event.Fire(new GameEvents.OrbGot(){});
                    hasWhiteOrb = true;
                } else if (item.keyId == Item.KEY_IDS.MEMORY)
                {
                    Core.Event.Fire(new GameEvents.NPCMemoryGot(){});
                    hasNPCMemory = true;
                }
                break;
            default:
                break;
        }
    }

    public void ResetItems()
    {
        hasWhiteOrb = false;
        hasBlackOrb = false;
        hasLantern = false;
        hasWeapon = false;
        hasNPCMemory = false;
        lantern.enabled = false;

        activeWeapon.ResetValues();
    }
}
