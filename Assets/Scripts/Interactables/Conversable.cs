using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class Conversable : Interactable
{
    [FormerlySerializedAs("interactionAudioEventName")] public string conversatingAudioEventName;
    public string conversationEndAudioEventName;
    public DialogueSO[] dialogues;
    public DialogueSO[] finalDialogues;

    [HideInInspector] public DialogueSO[] dialoguesToShow;
    [HideInInspector] public DialogueSO lastDialog;
    [HideInInspector] public bool dialogueEnded;

    protected bool loopContent;
    protected int dialogueIndex = 0;
    [HideInInspector] public List<ChoiceSO> currentChoices = new List<ChoiceSO>();
    [HideInInspector] public bool isSelectingChoice;
    private bool choiceSelected;
    private DialogueSO lastChoiceDialog;
    protected DialogueSO[] originalDialogues;
    protected bool dialoguesExtended;
    [HideInInspector] public PJ pj;
    private bool disableOnConversationEnded;

    protected virtual void Start()
    {
        loopContent = true;
        originalDialogues = dialogues;
    }

    public override void Interact(PJ pj, bool cancel)
    {
        this.pj = pj;
        if (Core.Dialogue.IsShowingText && lastDialog != null)
        {
            if (!cancel)
            {
                Core.Dialogue.ShowFullCurrentText(lastDialog);
            }
            else if (!isSelectingChoice)
            {
                EndDialogue();
            }
        }
        else
        {
            
            if (isSelectingChoice && !cancel)
            {
                isSelectingChoice = false;
            }
            else
            {
                if (pj.inventory.HasNPCMemory)
                {
                    dialoguesToShow = finalDialogues;
                }
                else if ((choiceSelected || dialogueEnded) && !loopContent) // Dialogue ended
                {
                    dialoguesToShow = new DialogueSO[] {lastDialog};
                }
                else
                {
                    dialoguesToShow = dialogues;
                }

                if (!cancel)
                {
                    if (!IsInteracting())
                    {
                        if (!interactionStartedOnce)
                        {
                            interactionStartedOnce = true;
                        }
                        
                        StartDialogue();
                    }
                    else
                    {
                        ContinueDialog();
                    }
                } else
                {
                    if (!isSelectingChoice)
                    {
                        EndDialogue();
                    }
                }
            }
        }
    }

    public void StartDialogue()
    {
        SetInteracting(true);
        
        Core.Event.Fire(new GameEvents.ConversableDialogue(){ started = true });
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

    protected virtual void ShowNextDialog()
    {
        // Hay más diálogos que mostrar
        if (dialogueIndex < dialoguesToShow.Length)
        {
            if (conversatingAudioEventName != "")
            {
                Core.Audio.PlayFMODAudio(conversatingAudioEventName, transform);
            }
            
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
            if (conversatingAudioEventName != "")
            {
                Core.Audio.PlayFMODAudio(conversatingAudioEventName, transform);
            }

            lastDialog = lastChoiceDialog;
            Core.Dialogue.ShowText(lastChoiceDialog);
            lastChoiceDialog = null;
        }
        // Termina la conversación
        else
        {
            if (conversationEndAudioEventName != "")
            {
                Core.Audio.PlayFMODAudio(conversationEndAudioEventName, transform);
            }

            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        SetInteracting(false);
        Core.Dialogue.HideCanvas();

        Core.Event.Fire(new GameEvents.ConversableDialogue(){ started = false });
        Core.Event.Fire(new GameEvents.ConversableDialogueEnded() {conversable = this, lastDialogue = lastDialog});

        dialogueIndex = 0;

        if (!interactionEndedOnce)
        {
            interactionEndedOnce = true;
        }
        
        ResetDialoguesToOriginal();
        
        dialogueEnded = true;

        if (disableOnConversationEnded)
        {
            interactableCollider.enabled = false;
            SetOutlineVisibility(false);
            enabled = false;
            pj.interactableInContact = null;
        }
    }

    public virtual void ChoiceSelected(int choiceIndex)
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
    
    public void ExtendDialogues(DialogueSO newDialogue)
    {
        if (!dialoguesExtended)
        {
            dialoguesExtended = true;
        }
        DialogueSO[] extendedDialogues = new DialogueSO[dialogues.Length + 1];

        for (int dialogueIndex = 0; dialogueIndex < dialogues.Length; dialogueIndex++)
        {
            extendedDialogues[dialogueIndex] = dialogues[dialogueIndex];
        }

        extendedDialogues[dialogues.Length] = newDialogue;

        dialogues = extendedDialogues;

        RefreshDialoguesToShow();
    }
    
    public void ExtendOriginalDialogues(DialogueSO newDialogue)
    {
        DialogueSO[] extendedDialogues = new DialogueSO[originalDialogues.Length + 1];

        for (int dialogueIndex = 0; dialogueIndex < originalDialogues.Length; dialogueIndex++)
        {
            extendedDialogues[dialogueIndex] = originalDialogues[dialogueIndex];
        }

        extendedDialogues[originalDialogues.Length] = newDialogue;

        originalDialogues = extendedDialogues;

        RefreshDialoguesToShowWithOriginal();
    }

    protected void ResetDialoguesToOriginal()
    {
        dialogues = originalDialogues;
        dialoguesExtended = false;
    }

    protected void RefreshDialoguesToShow()
    {
        dialoguesToShow = dialogues;
    }

    protected void RefreshDialoguesToShowWithOriginal()
    {
        dialoguesToShow = originalDialogues;
    }
    
    public void ShowExtraDialog(DialogueSO extraDialogue)
    {
        lastDialog = extraDialogue;
        Core.Dialogue.ShowText(extraDialogue);
    }

    public void DissableOnConversationEnded()
    {
        disableOnConversationEnded = true;
    }
}
