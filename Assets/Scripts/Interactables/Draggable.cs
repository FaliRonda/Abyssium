using UnityEngine;

[System.Serializable]
public class Draggable : Conversable
{
    private bool canBeDragged;
    private DialogueSO currentQuestion;
    private DialogueSO grabQuestion;
    private DialogueSO replaceQuestion;
    private PJ pj;

    protected override void Start()
    {
        base.Start();

        loopContent = true;
        
        grabQuestion = Resources.Load<DialogueSO>("Narrative/Puzzle prototype/DraggableChoices/DragQuestion");
        replaceQuestion = Resources.Load<DialogueSO>("Narrative/Puzzle prototype/DraggableChoices/ReplaceQuestion");

        currentQuestion = grabQuestion;
    }

    public override void Interact(PJ pj, bool cancel)
    {
        if (canBeDragged && !dialoguesExtended)
        {
            if (pj.currentDraggable == null)
            {
                currentQuestion = grabQuestion;
            }
            else
            {
                currentQuestion = replaceQuestion;
            }
            
            ResetDialoguesToOriginal();
            ExtendDialogues(currentQuestion);
        }
        
        base.Interact(pj, cancel);
        this.pj = pj;
        
        if (tooltipCanvas != null && !tooltipCanvas.enabled && interactionEndedOnce)
        {
            tooltipCanvas.enabled = true;
        }
    }

    public void SetCanBeDraggable(bool canBeDraggedValue)
    {
        canBeDragged = canBeDraggedValue;
        GetComponent<Collider>().enabled = canBeDraggedValue;
    }

    public bool GetCanBeDraggable()
    {
        return canBeDragged;
    }

    public override void ChoiceSelected(int choiceIndex)
    {
        if (canBeDragged && choiceIndex == 0)
        {
            Core.Event.Fire(new GameEvents.PlayerCarryDraggable() { grabbedDraggable = this, replacedDraggable = pj.currentDraggable });
            
            if (pj.currentDraggable != null)
            {
                pj.currentDraggable.transform.position = transform.position;
                pj.currentDraggable.gameObject.SetActive(true);
            }
            
            // Poner objeto en la UI
                
            gameObject.SetActive(false);
            pj.currentDraggable = this;
            pj.interactableInContact = null;
            SetOutlineVisibility(false);
            
            if (tooltipCanvas != null && tooltipCanvas.enabled)
            {
                tooltipCanvas.enabled = false;
            }
                
            ShowNextDialog();
        }
        else
        {
            ShowNextDialog();
        }
    }
}