using UnityEngine;

[CreateAssetMenu(fileName = "Enemy ChaseNodeParameters", menuName = "AI/Parameters/Enemy Chase Node Parameters")]
public class ChaseNodeParametersSO : ScriptableObject
{
    public float visibilityRadius = 5f;
    public float minimumDistanceToPlayer = 0.8f;
    public float lightRadius = 2f;
    public float chaseSpeed = 2.25f;
    public float chaseInLightSpeed = 0.5f;
    public bool stoppedByStun = true;
}