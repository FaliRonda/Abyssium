using UnityEngine;

public class NPC : MonoBehaviour
{
    private Material material;

    private void Start()
    {
        material = GetComponentInChildren<SpriteRenderer>().material;
    }

    private void OnTriggerStay(Collider other)
    {
        if (OtherIsPlayer(other))
        {
            SetOutlineVisibility(true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (OtherIsPlayer(other))
        {
            SetOutlineVisibility(false);
        }
    }
    
    private bool OtherIsPlayer(Collider other)
    {
        return other.gameObject.layer == Layers.PJ_LAYER;
    }
    
    private void SetOutlineVisibility(bool isActive)
    {
        int activeIntValue = isActive ? 1 : 0;
        material.SetInt("_OutlineActive", activeIntValue);
    }
}
