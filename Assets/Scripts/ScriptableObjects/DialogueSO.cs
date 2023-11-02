using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Dialogue Data")]
public class DialogueSO : ScriptableObject
{
    [SerializeField] public string dialogueText;
    [SerializeField] public ChoiceSO[] choices;

}
