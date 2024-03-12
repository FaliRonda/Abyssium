using System.Collections.Generic;
using UnityEngine;

public static class Enemies
{
    private const string enemyTopologies = "EnemyTopologies/";
    private const string nodesParameters = "/NodesParameters/";
    private const string nodeParameters = "NodeParameters";
    
    public enum CODE_NAMES {
        Stalker,
        Jumper,
        Shooter,
        DarkHunter,
        DarkHunterSlower,
        Boss,
    }
    
    public static Dictionary<CODE_NAMES, string> enemiesNamesDictionary = new Dictionary<CODE_NAMES, string>
    {
        { CODE_NAMES.Stalker, "Stalker" },
        { CODE_NAMES.Jumper, "Jumper" },
        { CODE_NAMES.Shooter, "Shooter" },
        { CODE_NAMES.DarkHunter, "DarkHunter" },
        { CODE_NAMES.DarkHunterSlower, "DarkHunterSlower" },
        { CODE_NAMES.Boss, "Boss" },
    };

    public static string EnemiesParametersPathDictionary(CODE_NAMES enemyCode, string nodeName)
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