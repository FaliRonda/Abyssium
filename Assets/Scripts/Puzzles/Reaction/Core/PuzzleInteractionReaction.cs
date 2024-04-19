using UnityEngine;

public class PuzzleInteractionReaction : MonoBehaviour
{
    protected bool reactionPerformed;
    protected bool reactionStarted;
    
    public virtual void DoReaction(Conversable conversable)
    {
        reactionStarted = true;
    }
    
    public virtual void RevertReaction(Conversable conversable)
    {
        reactionStarted = false;
    }

    public void SetReactionPerformed(bool value)
    {
        reactionPerformed = value;
    }

    public void SetReactionStarted(bool value)
    {
        reactionStarted = value;
    }
}