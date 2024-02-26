using UnityEngine;

[CreateAssetMenu(fileName = "Enemy ShootNodeParameters", menuName = "AI/Parameters/Enemy Shoot Node Parameters")]
public class ShootNodeParametersSO : ScriptableObject
{
    public float bulletLifeTime = 8f;
    public float bulletSpeed = 6f;
    public float attackVisibilityDistance = 14f;
    public float shootCD = 2f;
}