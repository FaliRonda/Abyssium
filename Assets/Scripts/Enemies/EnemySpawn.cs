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

        if (debug)
        {
            this.EventSubscribe<GameEvents.EnemyDied>(e => EnemyDied());
        }
    }

    private void EnemyDied()
    {
        Sequence infiniteSpawnSequence = DOTween.Sequence();
        infiniteSpawnSequence
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
                Core.Event.Fire(new GameEvents.EnemySpawned(){ enemyAI = enemy.GetComponentInChildren<EnemyAI>() });
            });
    }
}
