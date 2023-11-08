using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class NPC : Interactable
{
    public DialogueSO[] dialogues;
    private int dialogueIndex = 0;
    private List<ChoiceSO> currentChoices = new List<ChoiceSO>();
    private bool isSelectingChoice = false;
    private DialogueSO currentChoiceDialog;

    public override void Interact(PJ pj)
    {
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
        if (dialogueIndex < dialogues.Length)
        {
            Core.Dialogue.ShowText(dialogues[dialogueIndex].dialogueText);
            ShowChoices(dialogues[dialogueIndex]);

            dialogueIndex++;
        }
        else if (currentChoiceDialog != null)
        {
            Core.Dialogue.ShowText(currentChoiceDialog.dialogueText);
            ShowChoices(currentChoiceDialog);

            currentChoiceDialog = null;
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
    }

    public void ChoiceSelected(int choiceIndex)
    {
        currentChoiceDialog = currentChoices[choiceIndex].nextDialogue;
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
