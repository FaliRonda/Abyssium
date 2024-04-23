public class AskPutFireInBottle : PuzzleInformationHiddenCarryingDraggableReaction
{
    public DialogueSO askPutFireInBottleDialogue;

    public override void DoReaction(Conversable conversable)
    {
        if (!reactionStarted && !reactionPerformed)
        {
            conversable.ExtendDialogues(askPutFireInBottleDialogue);
            reactionStarted = true;
        }
    }
}
