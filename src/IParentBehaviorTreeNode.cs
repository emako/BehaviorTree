namespace BehaviorTree;

/// <summary>
/// A behavior tree node that owns one or more child nodes.
/// </summary>
public interface IParentBehaviorTreeNode : IBehaviorTreeNode
{
    /// <summary>
    /// Adds a child node to this parent.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    public void AddChild(IBehaviorTreeNode child);
}
