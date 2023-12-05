// Selector node

using System.Collections.Generic;

public class BTSelector : BTNode
{
    public BTNode[] nodes;
    public bool AIActive = true;

    public BTSelector(BTNode[] nodes)
    {
        this.nodes = nodes;
    }

    public override BTNodeState Execute()
    {
        if (AIActive)
        {
            foreach (BTNode node in nodes)
            {
                BTNodeState state = node.Execute();
                if (state == BTNodeState.Success || state == BTNodeState.Running)
                {
                    return state;
                }
            }
            return BTNodeState.Failure;
        }
        else
        {
            return BTNodeState.Success;
        }
    }

    public override void InitializeNode(Dictionary<string, object> parameters)
    {
        foreach (BTNode node in nodes)
        {
            node.InitializeNode(parameters);
            node.ResetNode();
        }
    }

    public override void DrawGizmos()
    {
        foreach (BTNode node in nodes)
        {
            node.DrawGizmos();
        }
    }
}