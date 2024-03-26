using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class BTSummonNode : BTNode
{
    private Enemies.ENEMY_TYPE enemyCode;
    private bool standAfterSummon;
    private SummonNodeParametersSO summonNodeParameters;
    private Sequence summonSequence;
    private MaterialPropertyBlock propertyBlock;
    private bool waitBetweenSummons;
    private int enemyWaveCount;
    private Sequence standAfterSummonSequence;
    private Sequence waitBetweenSummonSequence;
    private bool canSummon;

    public BTSummonNode(Enemies.ENEMY_TYPE enemyCode)
    {
        this.enemyCode = enemyCode;
        canSummon = true;
    }

    public override BTNodeState Execute()
    {
        if (canSummon)
        {
            if (standAfterSummon)
            {
                return BTNodeState.Running;
            }
            
            if (waitBetweenSummons)
            {
                return BTNodeState.NextTree;
            }

            Vector3 direction = playerTransform.position - enemyTransform.position;
            Summon(direction);

            return BTNodeState.Running;
        }
        return BTNodeState.NextTree;
    }

    private void Summon(Vector3 direction)
    {
        standAfterSummon = true;
        waitBetweenSummons = true;
        enemySprite.flipX = direction.x > 0;

        float whiteHitTargetValue = 1 - summonNodeParameters.whiteHitPercentage;

        Color castingColor = summonNodeParameters.castingColor;
        
        summonSequence = DOTween.Sequence();
        summonSequence
            .Append(DOTween.To(() => 1, x => {
                whiteHitTargetValue = x;
                UpdateCastingGrading(x, castingColor);
            }, whiteHitTargetValue, summonNodeParameters.anticipacionDuration))
            .AppendCallback(() =>
            {
                UpdateCastingGrading(1, castingColor);
                SummonEnemies();
            })
            .OnKill(() =>
            {
                UpdateCastingGrading(1, castingColor);
            });
        
        standAfterSummonSequence = DOTween.Sequence();
        standAfterSummonSequence.AppendInterval(summonNodeParameters.anticipacionDuration);
        standAfterSummonSequence.AppendInterval(summonNodeParameters.standAfterSummonCD);
        standAfterSummonSequence.AppendCallback(() =>
        {
            standAfterSummon = false;
        });
        
        waitBetweenSummonSequence = DOTween.Sequence();
        waitBetweenSummonSequence.AppendInterval(summonNodeParameters.anticipacionDuration);
        waitBetweenSummonSequence.AppendInterval(summonNodeParameters.timeBetweenSummons);
        waitBetweenSummonSequence.AppendCallback(() =>
        {
            waitBetweenSummons = false;
        });
    }
    
    private void UpdateCastingGrading(float value, Color color)
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
        
        Renderer renderer = enemySprite.GetComponent<Renderer>();
        
        Material mat = renderer.material;
        mat.SetFloat("_AlphaCasting", value);
        mat.SetColor("_ColorCasting", color);

        renderer.material = mat;
    }

    private void SummonEnemies()
    {
        var nextWave = summonNodeParameters.enemiesToSpawn[enemyWaveCount];

        foreach (KeyValuePair<Vector3, GameObject> wavePair in nextWave.waveParametersDictionary)
        {
            GameObject enemySpawnGO = GameObject.Instantiate(summonNodeParameters.spawnPrefab, enemyTransform.parent);
            EnemySpawn enemySpawn = enemySpawnGO.GetComponent<EnemySpawn>();

            enemySpawn.Initialize(wavePair.Key, wavePair.Value);
            enemySpawn.DoSpawn();
        }

        enemyWaveCount++;
        
        if (enemyWaveCount >= summonNodeParameters.enemiesToSpawn.Length)
        {
            canSummon = false;
        }
    }
    
    public override void ResetNode(bool enemyDied)
    {
        if (enemyDied)
        {
            summonSequence.Kill();
        }
    }

    public override void InitializeNode(Dictionary<string, object> parameters)
    {
        base.InitializeNode(parameters);
        AssignNodeParameters();
    }
    
    private void AssignNodeParameters()
    {
        summonNodeParameters =
            Resources.Load<SummonNodeParametersSO>(Enemies.EnemiesParametersPathDictionary(enemyCode, "Summon"));

        // Check if the ScriptableObject was loaded successfully.
        if (summonNodeParameters == null)
        {
            Debug.LogError("EnemyParemeters not found in Resources folder.");
        }
    }
}