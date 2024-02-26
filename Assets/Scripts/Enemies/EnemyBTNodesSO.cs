using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyBTNodes", menuName = "AI/Enemy Behavior Tree Nodes Conf")]
public class EnemyBTNodesSO : ScriptableObject
{
    public List<BTNode> behaviorNodes = new List<BTNode>();
}