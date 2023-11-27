using System.Collections.Generic;
using DG.Tweening;
using Ju.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueService : IService
{
    private GameObject gameUIGO;
    
    private TMP_Text conversationDialogueText;
    private List<GameObject> conversationDialogueChoicesGO = new List<GameObject>();
    private List<TMP_Text> conversationDialogueChoicesText = new List<TMP_Text>();
    private NPC currentNPC;

    private Transform lateralDialogsTransform;
    private GameObject lateralDialogPrefab;
    
    private TypingConfigSO typingConfig;

    public void StartConversation()
    {
        gameUIGO.SetActive(true);
    }

    public void HideCanvas()
    {
        gameUIGO.SetActive(false);
    }

    public void Initialize(Canvas canvas)
    {
        gameUIGO = canvas.transform.GetChild(0).gameObject;
        conversationDialogueText = gameUIGO.GetComponentInChildren<TMP_Text>();

        var choicesTransform = canvas.transform.GetChild(1).GetComponentInChildren<Transform>();

        for (int childIndex = 0; childIndex < choicesTransform.childCount; childIndex++)
        {
            var choiceTransform = choicesTransform.GetChild(childIndex);
            
            conversationDialogueChoicesGO.Add(choiceTransform.gameObject);
            conversationDialogueChoicesText.Add(choiceTransform.GetComponentInChildren<TMP_Text>());
        }
        
        lateralDialogsTransform = canvas.transform.GetChild(3).GetComponentInChildren<Transform>();
        lateralDialogPrefab = Resources.Load<GameObject>("Prefabs/LateralDialog");
        
        typingConfig = Resources.Load<TypingConfigSO>("Conf/TypingConfig");

        // Check if the ScriptableObject was loaded successfully.
        if (typingConfig == null)
        {
            Debug.LogError("TypingConfig not found in Resources folder.");
        }
    }
    
    public void ShowText(string text)
    {
        int charIndex = 0;
        conversationDialogueText.text = "";

        // Use DoTween to animate each letter in the text.
        DOTween.To(() => charIndex, x => charIndex = x, text.Length, text.Length * typingConfig.typingSpeed)
            .OnUpdate(() => {
                conversationDialogueText.text = text.Substring(0, charIndex);
            });
    }

    public void ShowChoice(int choiceIndex, ChoiceSO choice, NPC npc)
    {
        var currentChoice = conversationDialogueChoicesGO[choiceIndex];
        currentNPC = npc;
        conversationDialogueChoicesGO[choiceIndex].SetActive(true);
        conversationDialogueChoicesText[choiceIndex].text = choice.choiceText;
        
        var choiceButton = currentChoice.GetComponent<Button>();
        choiceButton.onClick.AddListener(delegate() { ChoiceSelected(choiceIndex); });
    }

    private void ChoiceSelected(int choiceIndex)
    {
        ResetChoices();
        currentNPC.ChoiceSelected(choiceIndex);
    }

    private void ResetChoices()
    {
        for (int choiceIndex = 0; choiceIndex < conversationDialogueChoicesGO.Count; choiceIndex++)
        {
            conversationDialogueChoicesText[choiceIndex].text = "";
            conversationDialogueChoicesGO[choiceIndex].SetActive(false);
        }
    }

    public void ShowLateralDialogs(GameDirector.DialogueDictionary lateralDialogs)
    {
        Sequence lateralDialogsSequence = DOTween.Sequence();

        foreach (var lateralDialog in lateralDialogs)
        {
            lateralDialogsSequence.AppendInterval(lateralDialog.Value)
                .AppendCallback(() => { CreateLateralDialog(lateralDialog.Key); });
        }

        lateralDialogsSequence.Play();
    }

    private void CreateLateralDialog(DialogueSO lateralDialog)
    {
        GameObject lateralDialogInstance = Object.Instantiate(lateralDialogPrefab, lateralDialogsTransform);
        TMP_Text lateralDialogTMP = lateralDialogInstance.GetComponentInChildren<TMP_Text>();
        lateralDialogTMP.text = lateralDialog.dialogueText;
        Image lateralDialogPortraitImage = lateralDialogInstance.GetComponentsInChildren<Image>()[1];
        if (lateralDialog.characterPortrait != null)
        {
            lateralDialogPortraitImage.sprite = lateralDialog.characterPortrait;
            lateralDialogPortraitImage.enabled = true;
        }
    }
}