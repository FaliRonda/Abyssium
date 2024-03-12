using UnityEngine;

[CreateAssetMenu(fileName = "Enemy PatrolNodeParameters", menuName = "AI/Parameters/Enemy Patrol Node Parameters")]
public class PatrolNodeParametersSO : ScriptableObject
{
    public float patrolSpeed = 2f;
    public float innerRadius = 2f;
    public float outerRadius = 5f;
    public float minWait = 0.2f;
    public float maxWait = 1f;
    public bool stoppedByStun = true;
}