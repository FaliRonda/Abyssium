using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Dialogue Data")]
public class DialogueSO : ScriptableObject
{
    [SerializeField] private string[] diagogueLines;

    public string[] DialogueLines { get { return diagogueLines; } }
}
