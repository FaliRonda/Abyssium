using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Ju.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueService : IService
{
    private GameObject dialogueGO;
    public TMP_Text dialogueText;
    public List<GameObject> dialogueChoicesGO = new List<GameObject>();
    public List<TMP_Text> dialogueChoicesText = new List<TMP_Text>();
    private NPC currentNPC;
    private TypingConfig typingConfig;

    public void StartConversation()
    {
        dialogueGO.SetActive(true);
    }

    public void HideCanvas()
    {
        dialogueGO.SetActive(false);
    }

    public void Initialize(Canvas canvas)
    {
        dialogueGO = canvas.transform.GetChild(0).gameObject;
        dialogueText = dialogueGO.GetComponentInChildren<TMP_Text>();

        var choicesTransform = canvas.transform.GetChild(1).GetComponentInChildren<Transform>();

        for (int childIndex = 0; childIndex < choicesTransform.childCount; childIndex++)
        {
            var choiceTransform = choicesTransform.GetChild(childIndex);
            
            dialogueChoicesGO.Add(choiceTransform.gameObject);
            dialogueChoicesText.Add(choiceTransform.GetComponentInChildren<TMP_Text>());
        }
        
        typingConfig = Resources.Load<TypingConfig>("Conf/TypingConfig");

        // Check if the ScriptableObject was loaded successfully.
        if (typingConfig == null)
        {
            Debug.LogError("TypingConfig not found in Resources folder.");
        }
    }
    
    public void ShowText(string text)
    {
        int charIndex = 0;
        dialogueText.text = "";

        // Use DoTween to animate each letter in the text.
        DOTween.To(() => charIndex, x => charIndex = x, text.Length, text.Length * typingConfig.typingSpeed)
            .OnUpdate(() => {
                dialogueText.text = text.Substring(0, charIndex);
            });
    }

    public void ShowChoice(int choiceIndex, ChoiceSO choice, NPC npc)
    {
        var currentChoice = dialogueChoicesGO[choiceIndex];
        currentNPC = npc;
        dialogueChoicesGO[choiceIndex].SetActive(true);
        dialogueChoicesText[choiceIndex].text = choice.choiceText;
        
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
        for (int choiceIndex = 0; choiceIndex < dialogueChoicesGO.Count; choiceIndex++)
        {
            dialogueChoicesText[choiceIndex].text = "";
            dialogueChoicesGO[choiceIndex].SetActive(false);
        }
    }
}