using UnityEngine;

[CreateAssetMenu(fileName = "Enemy SummonNodeParameters", menuName = "AI/Parameters/Enemy Summon Node Parameters")]
public class SummonNodeParametersSO : ScriptableObject
{
    public float standAfterSummonCD = 2f;
    public GameObject spawnPrefab;
    public EnemyWaveSO[] enemiesToSpawn;
    public float anticipacionDuration;
    public float whiteHitPercentage = 0.25f;
    public float timeBetweenSummons;
}