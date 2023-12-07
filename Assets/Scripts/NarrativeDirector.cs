using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class NarrativeDirector : MonoBehaviour
{
    public GameObject narrativeCurtain;
    public TMP_Text narrativeText;
    
    public DialogueSO[] narrativeDialogues;

    private int narrativeDialogueIndex = 0;
    private bool isShowingNarrative = false;

    public bool IsShowingNarrative
    {
        get => isShowingNarrative;
    }

    public bool IsTypingText
    {
        get => isTypingText;
    }

    private bool isTypingText = false;
    
    private TypingConfigSO typingConfig;

    private void Start()
    {
        typingConfig = Resources.Load<TypingConfigSO>("Conf/TypingConfig");

        // Check if the ScriptableObject was loaded successfully.
        if (typingConfig == null)
        {
            Debug.LogError("TypingConfig not found in Resources folder.");
        }
    }

    public void ShowNarrative()
    {
        if (!isShowingNarrative)
        {
            isShowingNarrative = true;
            narrativeCurtain.SetActive(true);
        }
        
        ShowNextNarrative();
    }

    public void EndNarrative()
    {
        isShowingNarrative = false;
        narrativeCurtain.SetActive(false);
    }

    private void ShowNextNarrative()
    {
        if (narrativeDialogueIndex < narrativeDialogues.Length)
        {
            int charIndex = 0;
            isTypingText = true;

            Sequence narrativeSequence = DOTween.Sequence();

            var narrativeDialog = narrativeDialogues[narrativeDialogueIndex];

            narrativeSequence.AppendInterval(.2f)
                .AppendCallback(() =>
                {
                    DOTween.To(() => charIndex, x => charIndex = x, narrativeDialog.dialogueText.Length,
                            narrativeDialog.dialogueText.Length * typingConfig.typingSpeed * 1.5f)
                        .OnUpdate(() =>
                        {
                            narrativeText.text = narrativeDialog.dialogueText.Substring(0, charIndex);
                        }).OnComplete(() => { isTypingText = false; });
                });
            
            narrativeDialogueIndex++;
        }
        else
        {
            narrativeDialogueIndex = 0;
            isShowingNarrative = false;
            narrativeText.text = "";
            narrativeCurtain.SetActive(false);
        }

    }
}
