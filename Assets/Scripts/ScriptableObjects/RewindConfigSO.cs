using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RewindConfig", menuName = "Conf/Rewind Configuration")]
public class RewindConfigSO : ScriptableObject
{
    public float initialPorcentage = 0.25f;
    public float finalPorcentage = 0.10f;
    public float accelerationFactor = 0.8f;
    public float decelerationFactor = 1.2f;
    [FormerlySerializedAs("minimumDuration")] public float minimumSoundPeriod = 0.05f;
    public float initialDuration = 0.3f;
    [FormerlySerializedAs("minDuration")] public float minimumRewindStepDuration = 0.01f;
}