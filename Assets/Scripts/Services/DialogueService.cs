using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Ju.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueService : IService
{
    public bool ChoicesInScreen => choicesInScreen;
    public bool IsShowingText => isShowingText;
    private GameObject gameUIGO;
    
    private TMP_Text conversationDialogueText;
    private Image conversationPJImage;
    private Image conversatioNPCImage;
    
    private List<GameObject> conversationDialogueChoicesGO = new List<GameObject>();
    private List<TMP_Text> conversationDialogueChoicesText = new List<TMP_Text>();
    private NPC currentNPC;

    private Transform lateralDialogsTransform;
    private GameObject lateralDialogPrefab;
    
    private TypingConfigSO typingConfig;
    private bool choicesInScreen;
    private int preselectedChoiceIndex;
    private bool isShowingText;
    private Sequence textShowSequence;

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
        conversationPJImage = gameUIGO.transform.GetChild(1).GetComponent<Image>();
        conversatioNPCImage = gameUIGO.transform.GetChild(2).GetComponent<Image>();
        
        Vector3 scale = conversationPJImage.transform.localScale;
        scale.x *= -1;
        conversationPJImage.transform.localScale = scale;
        
        scale = conversatioNPCImage.transform.localScale;
        scale.x *= -1;
        conversatioNPCImage.transform.localScale = scale;

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
    
    public void ShowText(DialogueSO dialogue)
    {
        conversatioNPCImage.enabled = false;
        conversationPJImage.enabled = false;
        if (dialogue.conversationCharacterPortrait != null)
        {
            if (dialogue.isSpeakingNPC)
            {
                conversatioNPCImage.sprite = dialogue.conversationCharacterPortrait;
                conversatioNPCImage.enabled = true;
            }
            else
            {
                conversationPJImage.sprite = dialogue.conversationCharacterPortrait;
                conversationPJImage.enabled = true;
            }
        }

        isShowingText = true;
        int charIndex = 0;
        conversationDialogueText.text = "";

        textShowSequence = DOTween.Sequence();
        
        textShowSequence
            .Append(DOTween.To(() => charIndex, x => charIndex = x, dialogue.dialogueText.Length, dialogue.dialogueText.Length * typingConfig.typingSpeed)
                .OnUpdate(() => {
                    conversationDialogueText.text = dialogue.dialogueText.Substring(0, charIndex);
                })
                .OnComplete(() => { isShowingText = false; })
            );
    }

    public void ShowChoice(int choiceIndex, ChoiceSO choice, NPC npc)
    {
        choicesInScreen = true;
        var currentChoice = conversationDialogueChoicesGO[choiceIndex];
        currentNPC = npc;
        conversationDialogueChoicesGO[choiceIndex].SetActive(true);
        conversationDialogueChoicesText[choiceIndex].text = choice.choiceText;

        if (choiceIndex == 0)
        {
            Color color = Color.white;
            ColorUtility.TryParseHtmlString("#D19B50", out color);
            conversationDialogueChoicesGO[choiceIndex].GetComponent<Image>().color = color;
        }
        
        var choiceButton = currentChoice.GetComponent<Button>();
        choiceButton.onClick.AddListener(delegate() { ChoiceSelected(choiceIndex); });
    }
    
    public void SelectChoicesWithControl(Vector3 inputDirection)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString("#D19B50", out color);

        if (inputDirection.y == 1)
        {
            preselectedChoiceIndex = 0;
            conversationDialogueChoicesGO[0].GetComponent<Image>().color = color;
            conversationDialogueChoicesGO[1].GetComponent<Image>().color = Color.white;
        }
        else if (inputDirection.y == -1)
        {
            preselectedChoiceIndex = 1;
            conversationDialogueChoicesGO[0].GetComponent<Image>().color = Color.white;
            conversationDialogueChoicesGO[1].GetComponent<Image>().color = color;
        }
    }

    public void ChoiceSelected(int choiceIndex)
    {
        if (choiceIndex == -1)
        {
            choiceIndex = preselectedChoiceIndex;
        }
        
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
        
        choicesInScreen = false;
    }

    public void ShowLateralDialogs(GameDirector.DialogueDictionary lateralDialogs)
    {
        var orderedLateralDialogs = lateralDialogs.OrderBy(x => x.Key.name).ToDictionary(x => x.Key, x => x.Value);
        Sequence lateralDialogsSequence = DOTween.Sequence();

        foreach (var lateralDialog in orderedLateralDialogs)
        {
            lateralDialogsSequence.AppendInterval(lateralDialog.Value)
                .AppendCallback(() => { CreateLateralDialog(lateralDialog.Key); });
        }

        lateralDialogsSequence.Play();
    }

    private void CreateLateralDialog(DialogueSO lateralDialogSO)
    {
        GameObject lateralDialogInstance = Object.Instantiate(lateralDialogPrefab, lateralDialogsTransform);
        TMP_Text lateralDialogTMP = lateralDialogInstance.GetComponentInChildren<TMP_Text>();
        LateralDialog lateralDialog = lateralDialogInstance.GetComponent<LateralDialog>();

        lateralDialog.dialogLifeTime = lateralDialogSO.lateralDialogLifeTime;
        
        int charIndex = 0;

        Sequence lateralDialogTextSequence = DOTween.Sequence();

        lateralDialogTextSequence.AppendInterval(.2f)
            .AppendCallback(() =>
            {
                DOTween.To(() => charIndex, x => charIndex = x, lateralDialogSO.dialogueText.Length,
                        lateralDialogSO.dialogueText.Length * typingConfig.typingSpeed)
                    .OnUpdate(() =>
                    {
                        lateralDialogTMP.text = lateralDialogSO.dialogueText.Substring(0, charIndex);
                    });
            });
        
        Image lateralDialogPortraitImage = lateralDialogInstance.GetComponentsInChildren<Image>()[1];
        if (lateralDialogSO.lateralCharacterPortrait != null)
        {
            lateralDialogPortraitImage.sprite = lateralDialogSO.lateralCharacterPortrait;
            lateralDialogPortraitImage.enabled = true;
        }
    }

    public void ShowFullCurrentText(DialogueSO lastDialog)
    {
        textShowSequence.Kill();
        conversationDialogueText.text = lastDialog.dialogueText;
        isShowingText = false;
    }
}