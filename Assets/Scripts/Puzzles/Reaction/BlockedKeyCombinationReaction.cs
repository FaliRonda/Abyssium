using UnityEngine;

public class BlockedKeyCombinationReaction : CombinationReaction
{
    public Draggable blockedKey;
    public GameObject runes;
    public Color runesReactionColor;

    public override void DoReaction(Conversable conversable)
    {
        base.DoReaction(conversable);

        if (!reactionPerformed)
        {
            runes.GetComponent<EmissionGlow>().UpdateValues(30, 1, 0.75f, 0, 1);
            runes.GetComponent<EmissionGlow>().UpdateAudioEvent("event:/Puzzle/Rituals/UnblockCombinationGlow");
            runes.GetComponent<EmissionGlow>().SetEmissionColor(runesReactionColor);
            runes.GetComponent<EmissionGlow>().enabled = true;
            
            blockedKey.GetComponent<EmissionGlow>().enabled = true;
            blockedKey.SetCanBeDraggable(true);
            // Actualizar el texto del bloque
            reactionPerformed = true;
        }
    }

    public override void RevertReaction(Conversable conversable)
    {
        base.RevertReaction(conversable);
        
        runes.GetComponent<EmissionGlow>().enabled = false;
        
        blockedKey.GetComponent<EmissionGlow>().enabled = false;
        blockedKey.SetCanBeDraggable(false);
        // Actualizar el texto del bloque
        reactionPerformed = false;
    }
}
