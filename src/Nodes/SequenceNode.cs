using System.Collections.Generic;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// Runs child nodes in sequence, until one fails.
/// </summary>
public class SequenceNode(string name) : IParentBehaviorTreeNode
{
    /// <summary>
    /// List of child nodes.
    /// </summary>
    protected readonly List<IBehaviorTreeNode> children = []; //todo: this could be optimized as a baked array.

    /// <summary>
    /// Name of the node.
    /// </summary>
    public string Name { get; } = name;

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

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    public async Task<BehaviorTreeStatus> TickAsync(TimeData time)
    {
        foreach (var child in children)
        {
            var childStatus = await child.TickAsync(time).ConfigureAwait(false);
            if (childStatus != BehaviorTreeStatus.Success)
            {
                return childStatus;
            }
        }

        return BehaviorTreeStatus.Success;
    }
#endif

    /// <summary>
    /// Add a child to the sequence.
    /// </summary>
    public void AddChild(IBehaviorTreeNode child)
    {
        children.Add(child);
    }
}
