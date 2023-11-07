using TMPro;
using UnityEngine;

public class NarrativeDirector : MonoBehaviour
{
    public GameObject narrativeCurtain;
    public TMP_Text narrativeText;
    
    public DialogueSO[] narrativeDialogues;

    private int narrativeDialogueIndex = 0;
    private bool isShowingNarrative = false;

    public void ShowNarrative()
    {
        if (!isShowingNarrative)
        {
            isShowingNarrative = true;
            narrativeCurtain.SetActive(true);
        }
        
        ShowNextNarrative();
    }

    private void ShowNextNarrative()
    {
        if (narrativeDialogueIndex < narrativeDialogues.Length)
        {
            narrativeText.text = narrativeDialogues[narrativeDialogueIndex].dialogueText;
            narrativeDialogueIndex++;
        }
        else
        {
            narrativeDialogueIndex = 0;
            isShowingNarrative = false;
            narrativeCurtain.SetActive(false);
        }

    }

    public bool IsShowingNarrative()
    {
        return isShowingNarrative;
    }
}
