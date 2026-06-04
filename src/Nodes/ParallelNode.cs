using System.Collections.Generic;

namespace BehaviorTree;

/// <summary>
/// Runs childs nodes in parallel.
/// </summary>
public class ParallelNode : IParentBehaviorTreeNode
{
    /// <summary>
    /// Name of the node.
    /// </summary>
    private string name;

    /// <summary>
    /// List of child nodes.
    /// </summary>
    private List<IBehaviorTreeNode> children = new List<IBehaviorTreeNode>();

    /// <summary>
    /// Number of child failures required to terminate with failure.
    /// </summary>
    private int numRequiredToFail;

    /// <summary>
    /// Number of child successess require to terminate with success.
    /// </summary>
    private int numRequiredToSucceed;

    public ParallelNode(string name, int numRequiredToFail, int numRequiredToSucceed)
    {
        this.name = name;
        this.numRequiredToFail = numRequiredToFail;
        this.numRequiredToSucceed = numRequiredToSucceed;
    }

    public BehaviorTreeStatus Tick(TimeData time)
    {
        var numChildrenSuceeded = 0;
        var numChildrenFailed = 0;

        foreach (var child in children)
        {
            var childStatus = child.Tick(time);
            switch (childStatus)
            {
                case BehaviorTreeStatus.Success: ++numChildrenSuceeded; break;
                case BehaviorTreeStatus.Failure: ++numChildrenFailed; break;
            }
        }

        if (numRequiredToSucceed > 0 && numChildrenSuceeded >= numRequiredToSucceed)
        {
            return BehaviorTreeStatus.Success;
        }

        if (numRequiredToFail > 0 && numChildrenFailed >= numRequiredToFail)
        {
            return BehaviorTreeStatus.Failure;
        }

        return BehaviorTreeStatus.Running;
    }

    public void AddChild(IBehaviorTreeNode child)
    {
        children.Add(child);
    }
}
