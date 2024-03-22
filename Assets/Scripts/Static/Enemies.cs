using System.Collections.Generic;
using UnityEngine;

public static class Enemies
{
    private const string enemyTopologies = "EnemyTopologies/";
    private const string nodesParameters = "/NodesParameters/";
    private const string nodeParameters = "NodeParameters";
    
    public enum ENEMY_TYPE {
        Stalker,
        Jumper,
        Shooter,
        DarkHunter,
        DarkHunterSlower,
        Boss,
    }
    
    public static Dictionary<ENEMY_TYPE, string> enemiesNamesDictionary = new Dictionary<ENEMY_TYPE, string>
    {
        { ENEMY_TYPE.Stalker, "Stalker" },
        { ENEMY_TYPE.Jumper, "Jumper" },
        { ENEMY_TYPE.Shooter, "Shooter" },
        { ENEMY_TYPE.DarkHunter, "DarkHunter" },
        { ENEMY_TYPE.DarkHunterSlower, "DarkHunterSlower" },
        { ENEMY_TYPE.Boss, "Boss" },
    };

    public static string EnemiesParametersPathDictionary(ENEMY_TYPE enemyCode, string nodeName)
    {
        string path = enemyTopologies;
        
        if (enemiesNamesDictionary.TryGetValue(enemyCode, out string enemyName))
        {
            string[] pathChunks = { enemyTopologies, enemyName, nodesParameters, enemyName, nodeName, nodeParameters};
            path = string.Concat(pathChunks);
        }
        else
        {
            Debug.LogError("Enemies: Nombre de enemigo no encontrado en el diccionario.");
        }

        
        return path;
    }
}