#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// Contract implemented by every node in a behavior tree.
/// </summary>
public interface IBehaviorTreeNode
{
    /// <summary>
    /// Advances the node by one tick using the supplied time data.
    /// </summary>
    /// <param name="time">Time information for the current tick.</param>
    /// <returns>The status of the node after this tick.</returns>
    public BehaviorTreeStatus Tick(TimeData time);

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Asynchronously advances the node by one tick using the supplied time data.
    /// </summary>
    /// <param name="time">Time information for the current tick.</param>
    /// <returns>A task that completes with the status of the node after this tick.</returns>
    public Task<BehaviorTreeStatus> TickAsync(TimeData time);
#endif
}
