using System.Collections.Generic;

namespace BehaviorTree;

/// <summary>
/// Runs child nodes in sequence, until one fails.
/// </summary>
public class SequenceNode(string name) : IParentBehaviorTreeNode
{
    /// <summary>
    /// Name of the node.
    /// </summary>
    private string name = name;

    /// <summary>
    /// List of child nodes.
    /// </summary>
    private List<IBehaviorTreeNode> children = []; //todo: this could be optimized as a baked array.

    public BehaviorTreeStatus Tick(TimeData time)
    {
        foreach (var child in children)
        {
            var childStatus = child.Tick(time);
            if (childStatus != BehaviorTreeStatus.Success)
            {
                return childStatus;
            }
        }

        return BehaviorTreeStatus.Success;
    }

    /// <summary>
    /// Add a child to the sequence.
    /// </summary>
    public void AddChild(IBehaviorTreeNode child)
    {
        children.Add(child);
    }
}
