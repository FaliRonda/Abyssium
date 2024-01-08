using Ju.Extensions;
using UnityEngine;

public class Interactable : MonoBehaviour, I_Interactable
{
    private Material material;
    private bool isInteracting = false;
    private bool canInteract = false;
    protected SpriteRenderer interactableSprite;
    protected SkinnedMeshRenderer interactableMesh;
    protected Collider interactableCollider;
    private bool gameIn3D = false;

    private void Awake()
    {
        //Try get material from sprite
        interactableSprite = GetComponent<SpriteRenderer>() != null ? GetComponent<SpriteRenderer>(): GetComponentInChildren<SpriteRenderer>();
        interactableMesh = GetComponent<SkinnedMeshRenderer>() != null ? GetComponent<SkinnedMeshRenderer>() : GetComponentInChildren<SkinnedMeshRenderer>();
        interactableCollider = GetComponent<Collider>() != null ? GetComponent<Collider>() : GetComponentInChildren<Collider>();

        if (interactableSprite != null)
        {
            material = interactableSprite.material;
        }
        else  if (interactableMesh != null)
        {
            material = interactableMesh.material;
        }
        
        this.EventSubscribe<GameEvents.SwitchPerspectiveEvent>(e => Switch2D3D(e.gameIn3D));
    }

    private void Switch2D3D(bool gameIn3D)
    {
        this.gameIn3D = gameIn3D;
        SetCanInteract(gameIn3D);
    }

    public bool IsInteracting()
    {
        return isInteracting;
    }

    public void SetInteracting(bool isInteracting)
    {
        this.isInteracting = isInteracting;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void SetCanInteract(bool canInteract)
    {
        this.canInteract = canInteract;
        if (!this.canInteract)
        {
            SetOutlineVisibility(false);
        }
    }

    private bool OtherIsPlayer(Collider other)
    {
        return other.gameObject.layer == Layers.PJ_LAYER;
    }
    
    public void SetOutlineVisibility(bool isActive)
    {
        int activeIntValue = GameState.gameIn3D && isActive ? 1 : 0;
        material.SetInt("_OutlineActive", activeIntValue);
    }
    
    public virtual void Interact(PJ pj)
    {
        throw new System.NotImplementedException();
    }
}
