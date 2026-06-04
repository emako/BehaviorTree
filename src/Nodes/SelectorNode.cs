using System.Collections.Generic;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// A composite node that tries children in order until one succeeds or is running.
/// </summary>
/// <param name="name">The display name of the selector node.</param>
public class SelectorNode(string name) : IParentBehaviorTreeNode
{
    /// <summary>
    /// Child nodes tried in order.
    /// </summary>
    protected readonly List<IBehaviorTreeNode> children = []; //todo: optimization, bake this to an array.

    /// <summary>
    /// The display name of the node.
    /// </summary>
    public string Name { get; } = name;

    /// <inheritdoc />
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

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public async Task<BehaviorTreeStatus> TickAsync(TimeData time)
    {
        foreach (var child in children)
        {
            var childStatus = await child.TickAsync(time).ConfigureAwait(false);
            if (childStatus != BehaviorTreeStatus.Failure)
            {
                return childStatus;
            }
        }

        return BehaviorTreeStatus.Failure;
    }
#endif

    /// <inheritdoc />
    public void AddChild(IBehaviorTreeNode child)
    {
        children.Add(child);
    }
}
