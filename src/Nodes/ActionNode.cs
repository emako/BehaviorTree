using System;

namespace BehaviorTree;

/// <summary>
/// A behavior tree leaf node for running an action.
/// </summary>
public class ActionNode : IBehaviorTreeNode
{
    /// <summary>
    /// The name of the node.
    /// </summary>
    private string name;

    /// <summary>
    /// Function to invoke for the action.
    /// </summary>
    private Func<TimeData, BehaviorTreeStatus> fn;

    public ActionNode(string name, Func<TimeData, BehaviorTreeStatus> fn)
    {
        this.name = name;
        this.fn = fn;
    }

    public BehaviorTreeStatus Tick(TimeData time)
    {
        return fn(time);
    }
}
