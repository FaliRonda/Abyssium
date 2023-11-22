using UnityEngine;

[CreateAssetMenu(fileName = "RewindConfig", menuName = "Conf/Rewind Configuration")]
public class RewindConfigSO : ScriptableObject
{
    public float initialPorcentage = 0.25f;
    public float finalPorcentage = 0.10f;
    public float accelerationFactor = 0.8f;
    public float decelerationFactor = 1.2f;
}