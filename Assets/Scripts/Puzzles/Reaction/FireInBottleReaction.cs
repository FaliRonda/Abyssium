using UnityEngine;
using UnityEngine.Serialization;

public class FireInBottleReaction : PuzzleYesChoiceDraggablesReaction
{
    [FormerlySerializedAs("strangeBottle")] public Draggable magicBottle;
    public Material fireBottleMaterial;
    public DialogueSO fireInBottleDialgue;
    
    public override void DoReaction(Conversable conversable)
    {
        base.DoReaction(conversable);
        
        if (!reactionPerformed)
        {
            // Cambiar el icono de la UI
            
            Renderer renderer = magicBottle.GetComponentInChildren<Renderer>();
            Material[] materials = renderer.materials;
            materials[1] = fireBottleMaterial;
            renderer.materials = materials;
            
            magicBottle.GetComponent<EmissionGlow>().enabled = true;
            magicBottle.UpdateTooltipText("Fire essence");
            
            conversable.ExtendDialogues(fireInBottleDialgue);
            reactionPerformed = true;
        }
    }
}
