using DG.Tweening;
using UnityEngine;

public class TouchTearReaction : PuzzleYesChoiceDraggablesReaction
{
    public Draggable moonTear;
    public DialogueSO tearFallDialgue;
    
    public override void DoReaction(Conversable conversable)
    {
        base.DoReaction(conversable);
        
        if (!reactionPerformed)
        {
            Vector3 initialPosition = moonTear.transform.position;
            Sequence tearMovementSequence = DOTween.Sequence();
            conversable.ExtendDialogues(tearFallDialgue);
            reactionPerformed = true;
            
            conversable.DissableOnConversationEnded();

            Core.Audio.PlayFMODAudio("event:/Puzzle/PuzzleHints/TearFall", transform);
            
            tearMovementSequence
                .Append(moonTear.transform.DOMoveZ(initialPosition.z - 0.5f, 0.2f))
                .Join(moonTear.transform.DOMoveY(0.1f, 0.5f))
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    moonTear.GetComponent<Collider>().enabled = true;
                    moonTear.GetComponent<EmissionGlow>().enabled = true;
                });
        }
    }
}
