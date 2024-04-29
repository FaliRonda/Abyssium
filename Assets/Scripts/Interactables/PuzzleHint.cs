using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleHint : Conversable
{
    public List<PuzzleHint> unblockablePuzzleHints;
    public List<Draggable> unblockableDraggables;
    
    public List<Draggable> interactingDraggables;

    [HideInInspector] public bool informationHidden;
    private PuzzleInformationHiddenReaction[] informationHiddenReactions;
    private PuzzleInformationHiddenCarryingDraggableReaction[] informationHiddenDraggablesReactions;
    private PuzzleYesChoiceDraggablesReaction[] yesChoiceDraggablesReactions;

    private List<PuzzleInteractionReaction> pendingReactions;

    protected override void Start()
    {
        base.Start();

        pendingReactions = new List<PuzzleInteractionReaction>();
        loopContent = true;
        
        foreach (PuzzleHint puzzleHint in unblockablePuzzleHints)
        {
            puzzleHint.informationHidden = true;
        }
        
        informationHiddenReactions = GetComponents<PuzzleInformationHiddenReaction>();
        informationHiddenDraggablesReactions = GetComponents<PuzzleInformationHiddenCarryingDraggableReaction>();
        yesChoiceDraggablesReactions = GetComponents<PuzzleYesChoiceDraggablesReaction>();
    }

    public override void Interact(PJ pj, bool cancel)
    {
        if (!interactionStartedOnce && cancel)
        {
            return;
        }

        if (!isSelectingChoice)
        {
            
            foreach (PuzzleHint puzzleHint in unblockablePuzzleHints)
            {
                if (puzzleHint.informationHidden)
                {
                    puzzleHint.informationHidden = false;
                }
            }

            if (!informationHidden)
            {
                foreach (Draggable draggable in unblockableDraggables)
                {
                    if (!draggable.GetCanBeDraggable())
                    {
                        draggable.SetCanBeDraggable(true);
                    }
                }

                foreach (PuzzleInformationHiddenReaction reaction in informationHiddenReactions)
                {
                    if (!pendingReactions.Contains(reaction))
                    {
                        reaction.DoReaction(this);
                        pendingReactions.Add(reaction);
                    }
                }

                if (interactingDraggables.Contains(pj.currentDraggable))
                {
                    foreach (PuzzleInformationHiddenCarryingDraggableReaction draggableReaction in informationHiddenDraggablesReactions)
                    {
                        if (!pendingReactions.Contains(draggableReaction))
                        {
                            draggableReaction.DoReaction(this);
                            pendingReactions.Add(draggableReaction);
                        }
                    }
                }
            }
            
            if (cancel && pendingReactions.Count > 0)
            {
                foreach (PuzzleInteractionReaction pendingReaction in pendingReactions)
                {
                    pendingReaction.RevertReaction(this);
                }
                
                pendingReactions.Clear();
            }
        }
        
        base.Interact(pj, cancel);
    }
    
    public override void ChoiceSelected(int choiceIndex)
    {
        if (choiceIndex == 0)
        {
            foreach (PuzzleInformationHiddenReaction reaction in informationHiddenReactions)
            {
                reaction.SetReactionPerformed(true);
                pendingReactions.Remove(reaction);
            }
            
            foreach (Draggable draggable in interactingDraggables)
            {
                foreach (PuzzleInformationHiddenCarryingDraggableReaction draggableReaction in informationHiddenDraggablesReactions)
                {
                    draggableReaction.SetReactionPerformed(true);
                    pendingReactions.Remove(draggableReaction);
                }
                
                foreach (PuzzleYesChoiceDraggablesReaction yesChoiceReaction in yesChoiceDraggablesReactions)
                {
                    yesChoiceReaction.DoReaction(this);
                }
            }
        }
        else
        {
            foreach (PuzzleInformationHiddenReaction reaction in informationHiddenReactions)
            {
                reaction.SetReactionStarted(false);
                pendingReactions.Remove(reaction);
            }
            
            foreach (PuzzleInformationHiddenCarryingDraggableReaction draggableReaction in informationHiddenDraggablesReactions)
            {
                draggableReaction.SetReactionStarted(false);
                pendingReactions.Remove(draggableReaction);
            }
            
            ResetDialoguesToOriginal();
        }

        ShowNextDialog();
    }
}
