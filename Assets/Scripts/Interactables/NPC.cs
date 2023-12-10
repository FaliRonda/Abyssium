using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class NPC : Interactable
{
    public DialogueSO[] dialogues;
    public DialogueSO[] finalDialogues;

    public DialogueSO[] dialoguesToShow;
    public DialogueSO lastDialog;
    public bool dialogueEnded;
    
    private int dialogueIndex = 0;
    public List<ChoiceSO> currentChoices = new List<ChoiceSO>();
    public bool isSelectingChoice;
    private bool choiceSelected;
    private bool memoryFound;
    private DialogueSO lastChoiceDialog;

    public override void Interact(PJ pj)
    {
        if (isSelectingChoice)
        {
            isSelectingChoice = false;
        }
        else
        {
            if (Core.Dialogue.IsShowingText)
            {
                Core.Dialogue.ShowFullCurrentText(lastDialog);
            }
            else
            {
                if (pj.inventory.HasNPCMemory)
                {
                    memoryFound = true;
                    dialoguesToShow = finalDialogues;
                }
                else if (choiceSelected || dialogueEnded) // Dialogue ended
                {
                    dialoguesToShow = new DialogueSO[] {lastDialog};
                    Core.Event.Fire(new GameEvents.NPCDialogueEnded() {npc = this, lastDialogue = lastDialog});
                }
                else
                {
                    dialoguesToShow = dialogues;
                }
                

                if (!IsInteracting())
                {
                    StartDialogue();
                }
                else
                {
                    ContinueDialog();
                }
            }
        }
    }

    public void StartDialogue()
    {
        SetInteracting(true);
        
        Core.Event.Fire(new GameEvents.NPCDialogue(){ started = true });
        Core.Dialogue.StartConversation(this);

        ShowNextDialog();
    }

    public void ContinueDialog()
    {
        if (!isSelectingChoice)
        {
            ShowNextDialog();
        }
    }

    private void ShowNextDialog()
    {
        // Hay más diálogos que mostrar
        if (dialogueIndex < dialoguesToShow.Length)
        {
            lastDialog = dialoguesToShow[dialogueIndex];
            Core.Dialogue.ShowText(dialoguesToShow[dialogueIndex]);

            if (lastDialog.drop != null)
            {
                Dropper dropper = GetComponent<Dropper>();
                if (dropper != null)
                {
                    dropper.Drop(lastDialog.drop);
                }
            }

            dialogueIndex++;
        }
        // Se muestra el último diálogo a raíz de una respuesta
        else if (lastChoiceDialog != null)
        {
            lastDialog = lastChoiceDialog;
            Core.Dialogue.ShowText(lastChoiceDialog);
            lastChoiceDialog = null;
        }
        // Termina la conversación
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        SetInteracting(false);
        Core.Dialogue.HideCanvas();
        
        Core.Event.Fire(new GameEvents.NPCDialogue(){ started = false });

        dialogueIndex = 0;

        if (memoryFound)
        {
            Vanish();
        }

        dialogueEnded = true;
    }

    private void Vanish()
    {
        Core.Event.Fire(new GameEvents.NPCVanished());
        Destroy(this.gameObject);
    }

    public void ChoiceSelected(int choiceIndex)
    {
        choiceSelected = true;
        lastChoiceDialog = currentChoices[choiceIndex].nextDialogue;
        if (currentChoices[choiceIndex].drop != null)
        {
            Dropper dropper = GetComponent<Dropper>();
            if (dropper != null)
            {
                dropper.Drop(currentChoices[choiceIndex].drop);
            }
        }
        
        currentChoices.Clear();

        dialogueIndex++;
        ShowNextDialog();
    }
}
