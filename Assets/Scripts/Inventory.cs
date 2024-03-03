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

    private Light lanternEnv;
    private Light lanternPj;
    private float lastAngle;

    private void Start()
    {
        lanternEnv = GetComponentsInChildren<Light>()[0];
        lanternEnv.enabled = hasLantern;
        lanternPj = GetComponentsInChildren<Light>()[1];
        lanternPj.enabled = hasLantern;
        
        lastAngle = -90;
        
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

    public void RotateItems(bool gameIn3D, GameDirector.ControlInputData controlInputData)
    {
        foreach (GameObject item in items)
        {
            switch (item.layer)
            {
                case Layers.WEAPON_LAYER:
                    RotateWeapon(item, controlInputData, gameIn3D);
                    break;
                case Layers.LIGHT_LAYER:
                    RotateLight(item, controlInputData, gameIn3D);
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
            item.transform.DORotate(new Vector3(xRotation, yRotation, zRotation), 0.2f);
            /*if (!gameIn3D)
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
                -/
            }*/
        }
    }
    
    private void RotateLight(GameObject item, GameDirector.ControlInputData controlInputData, bool gameIn3D)
    {
        RotateItem(0, -CalculateRotationAngle(controlInputData), 0, item, gameIn3D);
    }

    private void RotateWeapon(GameObject item, GameDirector.ControlInputData controlInputData, bool gameIn3D)
    {
        RotateItem(0, -CalculateRotationAngle(controlInputData), 0, item, gameIn3D);
    }

    private float CalculateRotationAngle(GameDirector.ControlInputData controlInputData)
    {
        float angle = lastAngle;
        
        var x = controlInputData.inputDirection.x;
        var y = controlInputData.inputDirection.y;
        
        if (x == 0 && y > 0)
        {
            angle = 0;
        }
        else if (x > 0 && y > 0)
        {
            angle = -45;
        }
        else if (x > 0 && y == 0)
        {
            angle = -90;
        }
        else if (x > 0 && y < 0)
        {
            angle = -135;
        }
        else if (x == 0 && y < 0)
        {
            angle = 180;
        }
        else if (x < 0 && y < 0)
        {
            angle = 135;
        }
        else if (x < 0 && y == 0)
        {
            angle = 90;
        }
        else if (x < 0 && y > 0)
        {
            angle = 45;
        }

        lastAngle = angle;

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
                lanternEnv.enabled = true;
                lanternPj.enabled = true;
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
        lanternEnv.enabled = false;
        lanternPj.enabled = false;

        activeWeapon.ResetValues();
    }
}
