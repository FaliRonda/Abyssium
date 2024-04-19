using System;
using UnityEngine;

public class SpecialFireCombinationReaction : CombinationReaction
{
    public GameObject runes;
    public Color runesReactionColor;
    
    public override void DoReaction(Conversable conversable)
    {
        base.DoReaction(conversable);

        DropPoint ritualDropPoint = (DropPoint)conversable;

        if (MagicBottleContainsFire(ritualDropPoint))
        {
            Material runesMaterial = runes.GetComponent<MeshRenderer>().material; 
            runesMaterial.SetColor("_EmissionColor", runesReactionColor);
            runes.GetComponent<EmissionGlow>().enabled = true;
            
            // Se activa el GameObject de las paredes con pinturas mágicas
            
            reactionPerformed = true;
        }
    }

    private bool MagicBottleContainsFire(DropPoint dropPoint)
    {
        bool containsFire = false;
        foreach (Draggable droppedDraggable in dropPoint.droppedDraggables)
        {
            if (droppedDraggable.name == "MagicalBottle" &&
                droppedDraggable.GetComponentInChildren<Renderer>().materials[1].name == "MagicFireBottle (Instance)")
            {
                containsFire = true;
            }
        }

        return containsFire;
    }
}
