using UnityEngine;

public class Interactable : MonoBehaviour, I_Interactable
{
    private Material material;
    private bool isInteracting = false;

    private void Awake()
    {
        //Try get material from sprite
        var sprite = GetComponent<SpriteRenderer>();
        var spriteInChildren = GetComponentInChildren<SpriteRenderer>();
        var mesh = GetComponent<SkinnedMeshRenderer>();
        var meshInChildren = GetComponentInChildren<SkinnedMeshRenderer>();

        if (sprite != null)
        {
            material = sprite.material;
        }
        else if (spriteInChildren != null)
        {
            material = spriteInChildren.material;
        } else if (mesh != null)
        {
            material = mesh.material;
        } else if (meshInChildren != null)
        {
            material = meshInChildren.material;
        }
    }
    
    public bool IsInteracting()
    {
        return isInteracting;
    }

    public void SetInteracting(bool isInteracting)
    {
        this.isInteracting = isInteracting;
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
    
    public virtual void Interact()
    {
        throw new System.NotImplementedException();
    }
}
