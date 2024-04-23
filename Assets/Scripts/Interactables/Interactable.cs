using Ju.Extensions;
using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour, I_Interactable
{
    public string tooltipText;
    
    private Material material;
    private bool isInteracting = false;
    private bool canInteract = false;
    protected SpriteRenderer interactableSprite;
    protected MeshRenderer interactableMesh;
    protected SkinnedMeshRenderer interactableSkinnedMesh;
    protected Collider interactableCollider;
    private bool gameIn3D = false;
    protected bool interactionEndedOnce;
    protected bool interactionStartedOnce;
    protected Canvas tooltipCanvas;

    private void Awake()
    {
        //Try get material from sprite
        interactableSprite = GetComponent<SpriteRenderer>() != null ? GetComponent<SpriteRenderer>(): GetComponentInChildren<SpriteRenderer>();
        interactableMesh = GetComponent<MeshRenderer>() != null ? GetComponent<MeshRenderer>() : GetComponentInChildren<MeshRenderer>();
        interactableSkinnedMesh = GetComponent<SkinnedMeshRenderer>() != null ? GetComponent<SkinnedMeshRenderer>() : GetComponentInChildren<SkinnedMeshRenderer>();
        interactableCollider = GetComponent<Collider>() != null ? GetComponent<Collider>() : GetComponentInChildren<Collider>();
        tooltipCanvas = GetComponentInChildren<Canvas>();

        if (tooltipCanvas != null)
        {
            tooltipCanvas.GetComponentInChildren<TMP_Text>().text = tooltipText;
        }

        if (interactableSprite != null)
        {
            material = interactableSprite.material;
        }
        else  if (interactableMesh != null)
        {
            material = interactableMesh.material;
        }
        else  if (interactableSkinnedMesh != null)
        {
            material = interactableSkinnedMesh.material;
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
    
    public void SetOutlineVisibility(bool isActive)
    {
        if (canInteract)
        {
            int activeIntValue = GameState.gameIn3D && isActive ? 1 : 0;
            material.SetInt("_OutlineActive", activeIntValue);

            if (interactionEndedOnce && tooltipCanvas != null && tooltipText != "")
            {
                tooltipCanvas.enabled = isActive;
            }
        }
    }
    
    public virtual void Interact(PJ pj, bool cancel)
    {
        Debug.LogWarning("Interact method not implemented.");
    }
    
    public void UpdateTooltipText(string newTooltipText)
    {
        if (tooltipCanvas != null)
        {
            tooltipCanvas.GetComponentInChildren<TMP_Text>().text = newTooltipText;
        }
    }
}
