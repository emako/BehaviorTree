#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

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

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Asynchronously update the time of the behavior tree.
    /// </summary>
    public Task<BehaviorTreeStatus> TickAsync(TimeData time);
#endif
}
