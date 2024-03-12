using DG.Tweening;
using Ju.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnTime;
    public bool debug;

    private GameObject fxGO;

    private void Awake()
    {
        fxGO = gameObject.transform.GetChild(0).gameObject;
        fxGO.SetActive(false);
    }

    public void DoSpawn()
    {
        Sequence spawnSequence = DOTween.Sequence();
        spawnSequence
            .AppendInterval(spawnTime)
            .AppendCallback(SpawnEnemy);
    }

    [Button]
    public void SpawnEnemy()
    {
        fxGO.gameObject.SetActive(true);
        //Core.Audio.Play(SOUND_TYPE.EnemySpawn, 1, 0, 0.03f);
        Core.Audio.PlayFMODAudio("event:/Characters/Enemies/Stalker/Spawn", transform);
        
        Sequence fxSpawnSequence = DOTween.Sequence();
        fxSpawnSequence
            .AppendInterval(spawnTime)
            .AppendCallback(() =>
            {
                fxGO.gameObject.SetActive(false);
                GameObject enemy = Instantiate(enemyPrefab, transform.parent);
                enemy.transform.position = transform.position;
                Core.Event.Fire(new GameEvents.EnemySpawned(){ enemyAI = enemy.GetComponentInChildren<EnemyAI>() });
                Destroy(gameObject);
            });
    }

    public void Initialize(Vector3 spawnPosition, GameObject enemyGO)
    {
        transform.position = spawnPosition;
        enemyPrefab = enemyGO;
    }
}
