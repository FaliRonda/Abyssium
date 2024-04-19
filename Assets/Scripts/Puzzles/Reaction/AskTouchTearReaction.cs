public class AskTouchTearReaction : PuzzleInformationHiddenReaction
{
    public DialogueSO askTouchTearDialogue;
    
    public override void DoReaction(Conversable conversable)
    {
        if (!reactionStarted && !reactionPerformed)
        {
            conversable.ExtendDialogues(askTouchTearDialogue);
            reactionStarted = true;
        }
    }
}
