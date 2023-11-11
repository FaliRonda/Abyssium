using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class NPC : Interactable
{
    public DialogueSO[] dialogues;
    public DialogueSO[] finalDialogue;

    private DialogueSO[] dialoguesToShow;
    private int dialogueIndex = 0;
    private List<ChoiceSO> currentChoices = new List<ChoiceSO>();
    private bool isSelectingChoice = false;
    private bool choiceSelected = false;
    private bool memoryFound = false;
    private DialogueSO lastChoiceDialog;
    private DialogueSO lastDialog;

    public override void Interact(PJ pj)
    {
        if (pj.inventory.HasNPCMemory)
        {
            memoryFound = true;
            dialoguesToShow = finalDialogue;
        }
        else if (choiceSelected)
        {
            dialoguesToShow = new DialogueSO[] {lastDialog};
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

    public void StartDialogue()
    {
        SetInteracting(true);
        Core.Dialogue.StartConversation();

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
        if (dialogueIndex < dialoguesToShow.Length)
        {
            lastDialog = dialoguesToShow[dialogueIndex];
            Core.Dialogue.ShowText(dialoguesToShow[dialogueIndex].dialogueText);
            ShowChoices(dialoguesToShow[dialogueIndex]);

            dialogueIndex++;
        }
        else if (lastChoiceDialog != null)
        {
            lastDialog = lastChoiceDialog;
            Core.Dialogue.ShowText(lastChoiceDialog.dialogueText);
            ShowChoices(lastChoiceDialog);

            lastChoiceDialog = null;
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowChoices(DialogueSO dialogue)
    {
        ChoiceSO[] choices = dialogue.choices;
        int choicesCount = choices.Length;
        bool haveChoices = choicesCount > 0;

        if (haveChoices)
        {
            isSelectingChoice = true;
            for (int i = 0; i < choicesCount; i++)
            {
                currentChoices.Add(choices[i]);
                Core.Dialogue.ShowChoice(i, choices[i], this);
            }
        }
    }

    private void EndDialogue()
    {
        SetInteracting(false);
        Core.Dialogue.HideCanvas();

        dialogueIndex = 0;

        if (memoryFound)
        {
            Vanish();
        }
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
        isSelectingChoice = false;
        
        ShowNextDialog();
    }
}
