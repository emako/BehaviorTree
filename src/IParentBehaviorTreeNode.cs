namespace BehaviorTree;

/// <summary>
/// Interface for behavior tree nodes.
/// </summary>
public interface IParentBehaviorTreeNode : IBehaviorTreeNode
{
    /// <summary>
    /// Add a child to the parent node.
    /// </summary>
    public void AddChild(IBehaviorTreeNode child);
}
