// Base class for behavior tree nodes
public abstract class BTNode
{
    public abstract BTNodeState Execute();
}

// Enum to represent the state of a behavior tree node
public enum BTNodeState
{
    Success,
    Failure,
    Running
}