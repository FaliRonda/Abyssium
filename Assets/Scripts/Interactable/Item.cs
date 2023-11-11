using UnityEngine;
using UnityEngine.Serialization;

public class Item : Interactable
{
    public ITEM_TYPE itemType;
    public KEY_IDS keyId;
    
    [Header("Weapon attributes")]
    public int weaponDamage;
    public float weaponRange;
    public float weaponCd;

    private Collider itemCollider;
    public enum ITEM_TYPE
    {
        WEAPON,
        UTIL,
        KEY
    }

    public enum KEY_IDS
    {
        WHITE_ORB,
        BLACK_ORB,
        MEMORY
    }

    public override void Interact(PJ pj)
    {
        pj.CollectItem(this);
        Destroy(gameObject);
    }
}
