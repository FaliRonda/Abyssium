
public class AskOpenDrawersReaction : PuzzleInformationHiddenCarryingDraggableReaction
{
    public DialogueSO askOpenDrawersDialogue;

    public override void DoReaction(Conversable conversable)
    {
        if (!reactionStarted && !reactionPerformed)
        {
            conversable.ExtendDialogues(askOpenDrawersDialogue);
            reactionStarted = true;
        }
    }
}
