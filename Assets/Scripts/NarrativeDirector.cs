using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            Image curtainImage = narrativeCurtain.GetComponent<Image>();
            Color originalColor = curtainImage.color;
            Color initialColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            curtainImage.color = initialColor;
            narrativeCurtain.SetActive(true);

            Sequence showCurtainSequence = DOTween.Sequence();

            showCurtainSequence
                .Append(DOTween.To(() => curtainImage.color, x => curtainImage.color = x, originalColor, 1f)
                    .SetEase(Ease.OutQuad))
                .AppendCallback(() => { ShowNextNarrative(); });
        }
        
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


            Color originalColor = narrativeText.color;
            Color initialColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            narrativeText.color = initialColor;
            narrativeText.text = narrativeDialog.dialogueText;
            
            Sequence showTextSequence = DOTween.Sequence();
            
            showTextSequence
                .Append(DOTween.To(() => narrativeText.color, x => narrativeText.color = x, originalColor, 3f))
                .OnComplete(() => { isTypingText = false; });
            
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
