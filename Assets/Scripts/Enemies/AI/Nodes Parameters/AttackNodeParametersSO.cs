using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Enemy AttackNodeParameters", menuName = "AI/Parameters/Enemy Attack Node Parameters")]
public class AttackNodeParametersSO : ScriptableObject
{
    public float attackVisibilityDistance = 2.75f;
    public float standAfterAttackCD = 1.5f;
    public float anticipationDistance = 0.4f;
    public float anticipacionDuration = 0.75f;
    public float attackMovementDistance = 2.5f;
    public float attackMovementDuration = 0.2f;
    public float enemyRayMaxDistance = .25f;
    public float waitForNextAttackCD = 3f;
    public float whiteHitPercentage = 0.25f;
}