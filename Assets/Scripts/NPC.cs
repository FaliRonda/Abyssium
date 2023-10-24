using TMPro;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public DialogueSO dialogueData;
    
    private int dialogueIndex = 0;
    private Material material;
    private Canvas canvas;
    private TMP_Text dialogueText;
    private bool isInDialog = false;

    #region Unity Events
    
    private void Start()
    {
        material = GetComponentInChildren<SpriteRenderer>().material;
        canvas = GetComponentInChildren<Canvas>();
        dialogueText = GetComponentInChildren<TMP_Text>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (OtherIsPlayer(other))
        {
            SetOutlineVisibility(true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (OtherIsPlayer(other))
        {
            SetOutlineVisibility(false);
        }
    }

    #endregion

    #region Utils
    
    private bool OtherIsPlayer(Collider other)
    {
        return other.gameObject.layer == Layers.PJ_LAYER;
    }
    
    private void SetOutlineVisibility(bool isActive)
    {
        int activeIntValue = isActive ? 1 : 0;
        material.SetInt("_OutlineActive", activeIntValue);
    }

    #endregion
    
    public void StartDialogue()
    {
        isInDialog = true;
        canvas.enabled = true;

        ShowNextDialog();
    }

    public bool IsInDialog()
    {
        return isInDialog;
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
        isInDialog = false;
        canvas.enabled = false;

        dialogueIndex = 0;
    }
}
