using TMPro;
using UnityEngine;

public class NPC : Interactable
{
    public DialogueSO dialogueData;
    
    private int dialogueIndex = 0;
    private Canvas canvas;
    private TMP_Text dialogueText;

    private void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        dialogueText = GetComponentInChildren<TMP_Text>();

        canvas.worldCamera = Camera.main;
    }

    public override void Interact()
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
        canvas.enabled = true;

        ShowNextDialog();
    }

    public void ContinueDialog()
    {
        ShowNextDialog();
    }

    private void ShowNextDialog()
    {
        if (dialogueIndex < dialogueData.DialogueLines.Length)
        {
            dialogueText.text = dialogueData.DialogueLines[dialogueIndex];
            dialogueIndex++;
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        SetInteracting(false);
        canvas.enabled = false;

        dialogueIndex = 0;
    }
}
