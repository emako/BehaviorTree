namespace BehaviorTree;

/// <summary>
/// Interface for behavior tree nodes.
/// </summary>
public interface IBehaviorTreeNode
{
    /// <summary>
    /// Update the time of the behavior tree.
    /// </summary>
    public BehaviorTreeStatus Tick(TimeData time);
}
