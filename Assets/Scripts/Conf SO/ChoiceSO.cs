using UnityEngine;

[CreateAssetMenu(fileName = "New Choice Data", menuName = "Data/Choice Data")]
public class ChoiceSO : ScriptableObject
{
    [SerializeField] public string choiceText;
    [SerializeField] public DialogueSO nextDialogue;
    [SerializeField] public GameObject drop;
}