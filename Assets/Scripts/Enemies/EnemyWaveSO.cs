using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWave", menuName = "Enemy Wave")]
public class EnemyWaveSO : ScriptableObject
{
    [System.Serializable]
    public class WaveParametersDictionary : SerializableDictionary<Vector3, GameObject> { }
    
    [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Spawn position", ValueLabel = "Enemy")]
    public WaveParametersDictionary waveParametersDictionary = new WaveParametersDictionary();
}