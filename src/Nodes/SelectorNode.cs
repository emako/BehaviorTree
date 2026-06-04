using System.Collections.Generic;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// Selects the first node that succeeds. Tries successive nodes until it finds one that doesn't fail.
/// </summary>
/// <param name="name">
/// The name of the node.
/// </param>
public class SelectorNode(string name) : IParentBehaviorTreeNode
{
    /// <summary>
    /// List of child nodes.
    /// </summary>
    protected readonly List<IBehaviorTreeNode> children = []; //todo: optimization, bake this to an array.

    public string Name { get; } = name;

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

    /// <summary>
    /// Add a child node to the selector.
    /// </summary>
    public void AddChild(IBehaviorTreeNode child)
    {
        children.Add(child);
    }
}
