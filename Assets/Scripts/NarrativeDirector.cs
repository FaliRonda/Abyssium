using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NarrativeDirector : MonoBehaviour
{
    public string combatDemoText;
    public GameObject narrativeCurtain;
    public TMP_Text narrativeText;
    
    public DialogueSO[] narrativeDialogues;

    private int narrativeDialogueIndex = 0;
    private bool isShowingNarrative = false;

    private Color initialCurtainColor;
    private Color originalCurtainColor;
    private Image curtainImage;

    private Color initialTextColor;
    private Color originalTextColor;
    
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
        curtainImage = narrativeCurtain.GetComponent<Image>();
        originalCurtainColor = curtainImage.color;
        initialCurtainColor = new Color(originalCurtainColor.r, originalCurtainColor.g, originalCurtainColor.b, 0f);
        
        originalTextColor = narrativeText.color;
        initialTextColor = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
        
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

            curtainImage.color = initialCurtainColor;
            narrativeCurtain.SetActive(true);
            
            Sequence showCurtainSequence = DOTween.Sequence();

            showCurtainSequence
                .Append(DOTween.To(() => curtainImage.color, x => curtainImage.color = x, originalCurtainColor, 1f)
                    .SetEase(Ease.OutQuad))
                .AppendCallback(() => { ShowNextNarrative(); });
        }
        
    }

    private void ShowNextNarrative()
    {
        if (narrativeDialogueIndex < narrativeDialogues.Length)
        {
            isTypingText = true;

            var narrativeDialog = narrativeDialogues[narrativeDialogueIndex];

            narrativeText.color = initialTextColor;
            narrativeText.text = narrativeDialog.dialogueText;
            
            Sequence showTextSequence = DOTween.Sequence();
            
            showTextSequence
                .Append(DOTween.To(() => narrativeText.color, x => narrativeText.color = x, originalTextColor, 3f))
                .OnComplete(() => { isTypingText = false; });
            
            narrativeDialogueIndex++;
        }
        else
        {
            narrativeDialogueIndex = 0;
            EndNarrative();
        }
    }
    
    public void EndNarrative()
    {
        isShowingNarrative = false;
        isShowingNarrative = false;
        narrativeText.text = "";
        curtainImage.color = originalCurtainColor;
        narrativeText.color = originalTextColor;
        narrativeCurtain.SetActive(false);
    }

    public void ShowCombatEndNarrative()
    {
        curtainImage.color = initialCurtainColor;
        narrativeCurtain.SetActive(true);
            
        Sequence showCurtainSequence = DOTween.Sequence();

        showCurtainSequence
            .Append(DOTween.To(() => curtainImage.color, x => curtainImage.color = x, originalCurtainColor, 2f)
                .SetEase(Ease.OutQuad))
            .AppendInterval(1f)
            .AppendCallback(() => { narrativeText.text = combatDemoText; });
    }
}
