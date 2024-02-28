using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyBTNodes", menuName = "AI/Enemy Behavior Tree Nodes Conf")]
public class EnemyBTNodesSO : ScriptableObject
{
    public Enemies.CODE_NAMES enemyCode;
    
    public enum BEHAVIOURS {
        Attack,
        Chase,
        Patrol,
        Shoot
    }
    
    public List<BEHAVIOURS> behaviorNodes = new List<BEHAVIOURS>();
}