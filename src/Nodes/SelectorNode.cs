using System.Collections.Generic;

namespace BehaviorTree;

/// <summary>
/// Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
/// </summary>
public class SelectorNode : IParentBehaviorTreeNode
{
    /// <summary>
    /// The name of the node.
    /// </summary>
    private string name;

    /// <summary>
    /// List of child nodes.
    /// </summary>
    private List<IBehaviorTreeNode> children = new List<IBehaviorTreeNode>(); //todo: optimization, bake this to an array.

    public SelectorNode(string name)
    {
        this.name = name;
    }

    public BehaviorTreeStatus Tick(TimeData time)
    {
        foreach (var child in children)
        {
            var childStatus = child.Tick(time);
            if (childStatus != BehaviorTreeStatus.Failure)
            {
                return childStatus;
            }
        }

        return BehaviorTreeStatus.Failure;
    }

    /// <summary>
    /// Add a child node to the selector.
    /// </summary>
    public void AddChild(IBehaviorTreeNode child)
    {
        children.Add(child);
    }
}
