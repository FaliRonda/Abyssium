// Selector node
public class BTSelector : BTNode
{
    private BTNode[] nodes;

    public BTSelector(params BTNode[] nodes)
    {
        this.nodes = nodes;
    }

    public override BTNodeState Execute()
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
}