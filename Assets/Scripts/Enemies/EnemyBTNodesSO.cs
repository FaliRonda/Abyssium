using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyBTNodes", menuName = "AI/Enemy Behavior Tree Nodes Conf")]
public class EnemyBTNodesSO : ScriptableObject
{
    public Enemies.ENEMY_TYPE enemyCode;
    
    public enum BEHAVIOURS {
        Attack,
        Chase,
        Patrol,
        Shoot,
        Summon,
    }
    
    public List<BEHAVIOURS> behaviorNodes = new List<BEHAVIOURS>();
}