using UnityEngine;

public class TakePowdersFromDrawersReaction : PuzzleYesChoiceDraggablesReaction
{
    public DialogueSO powdersInDeskDialogue;
    public GameObject powders;

    public override void DoReaction(Conversable conversable)
    {
        if (!reactionStarted && !reactionPerformed)
        {
            Core.Audio.PlayFMODAudio("event:/Puzzle/PuzzleHints/DrawersOpen", transform);
            conversable.ExtendDialogues(powdersInDeskDialogue);
            
            powders.SetActive(true);
            powders.GetComponent<Interactable>().SetCanInteract(true);

            conversable.DissableOnConversationEnded();
            conversable.pj.currentDraggable = null;
            
            reactionStarted = true;
            reactionPerformed = true;
        }
    }
}