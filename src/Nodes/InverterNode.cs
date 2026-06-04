using System;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// Decorator node that inverts the success/failure of its child.
/// </summary>
/// <param name="name">
/// Name of the node.
/// </param>
public class InverterNode(string name) : IParentBehaviorTreeNode
{
    /// <summary>
    /// The child to be inverted.
    /// </summary>
    private IBehaviorTreeNode childNode;

    public string Name { get; } = name;

    public BehaviorTreeStatus Tick(TimeData time)
    {
        if (childNode == null)
        {
            throw new ApplicationException("InverterNode must have a child node!");
        }

        return Invert(childNode.Tick(time));
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    public async Task<BehaviorTreeStatus> TickAsync(TimeData time)
    {
        if (childNode == null)
        {
            throw new ApplicationException("InverterNode must have a child node!");
        }

        return Invert(await childNode.TickAsync(time).ConfigureAwait(false));
    }
#endif

    /// <summary>
    /// Add a child to the parent node.
    /// </summary>
    public void AddChild(IBehaviorTreeNode child)
    {
        if (childNode != null)
        {
            throw new ApplicationException("Can't add more than a single child to InverterNode!");
        }

        childNode = child;
    }

    private static BehaviorTreeStatus Invert(BehaviorTreeStatus result)
    {
        if (result == BehaviorTreeStatus.Failure)
        {
            return BehaviorTreeStatus.Success;
        }

        if (result == BehaviorTreeStatus.Success)
        {
            return BehaviorTreeStatus.Failure;
        }

        return result;
    }
}
