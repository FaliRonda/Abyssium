using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Data/Dialogue Data")]
public class DialogueSO : ScriptableObject
{
    [SerializeField] public string dialogueText;
    [SerializeField] public ChoiceSO[] choices;
    [SerializeField] public bool isLateral;
    [SerializeField] public Sprite characterPortrait;
}
