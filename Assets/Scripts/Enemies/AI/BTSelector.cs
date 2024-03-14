// Selector node

using System.Collections.Generic;
using System.Linq;

public class BTSelector : BTNode
{
    public List<BTNode> nodes;

    public BTSelector(EnemyBTNodesSO.BEHAVIOURS[] behaviours, Enemies.CODE_NAMES enemyCode)
    {
        nodes = new List<BTNode>();
        
        foreach (EnemyBTNodesSO.BEHAVIOURS behaviour in behaviours)
        {
            switch (behaviour)
            {
                case EnemyBTNodesSO.BEHAVIOURS.Attack:
                    nodes.Add(new BTAttackNode(enemyCode));
                    break;
                case EnemyBTNodesSO.BEHAVIOURS.Chase:
                    nodes.Add(new BTChaseNode(enemyCode));
                    break;
                case EnemyBTNodesSO.BEHAVIOURS.Patrol:
                    nodes.Add(new BTPatrolNode(enemyCode));
                    break;
                case EnemyBTNodesSO.BEHAVIOURS.Shoot:
                    nodes.Add(new BTShootNode(enemyCode));
                    break;
                case EnemyBTNodesSO.BEHAVIOURS.Summon:
                    nodes.Add(new BTSummonNode(enemyCode));
                    break;
            }
        }
    }

    public override BTNodeState Execute()
    {
        if (enemyTransform.GetComponent<EnemyAI>().aIActive)
        {
            foreach (BTNode node in nodes)
            {
                BTNodeState state = node.Execute();
                if (state == BTNodeState.NextTree || state == BTNodeState.Running)
                {
                    return state;
                }
            }
            return BTNodeState.Failure;
        }
     
        return BTNodeState.NextTree;
    }

    public override void InitializeNode(Dictionary<string, object> parameters)
    {
        foreach (BTNode node in nodes)
        {
            node.InitializeNode(parameters);
            node.ResetNode();
        }
        
        base.InitializeNode(parameters);
    }

    public override void DrawGizmos()
    {
        foreach (BTNode node in nodes)
        {
            node.DrawGizmos();
        }
    }

    public void ResetNodes(bool force)
    {
        foreach (BTNode node in nodes)
        {
            node.ResetNode(force);
        }
    }
    
    public void ResetNodes()
    {
        foreach (BTNode node in nodes)
        {
            node.ResetNode();
        }
    }
}