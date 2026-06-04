using System;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// A decorator node that inverts the success and failure of its single child.
/// </summary>
/// <param name="name">The display name of the inverter node.</param>
public class InverterNode(string name) : IParentBehaviorTreeNode
{
    /// <summary>
    /// The single child whose result is inverted.
    /// </summary>
    private IBehaviorTreeNode childNode;

    /// <summary>
    /// The display name of the node.
    /// </summary>
    public string Name { get; } = name;

    /// <inheritdoc />
    /// <exception cref="ApplicationException">Thrown when the inverter has no child node.</exception>
    public BehaviorTreeStatus Tick(TimeData time)
    {
        if (childNode == null)
        {
            throw new ApplicationException("InverterNode must have a child node!");
        }

        return Invert(childNode.Tick(time));
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    /// <exception cref="ApplicationException">Thrown when the inverter has no child node.</exception>
    public async Task<BehaviorTreeStatus> TickAsync(TimeData time)
    {
        if (childNode == null)
        {
            throw new ApplicationException("InverterNode must have a child node!");
        }

        return Invert(await childNode.TickAsync(time).ConfigureAwait(false));
    }
#endif

    /// <inheritdoc />
    /// <exception cref="ApplicationException">Thrown when a second child is added.</exception>
    public void AddChild(IBehaviorTreeNode child)
    {
        if (childNode != null)
        {
            throw new ApplicationException("Can't add more than a single child to InverterNode!");
        }

        childNode = child;
    }

    /// <summary>
    /// Swaps success and failure while leaving <see cref="BehaviorTreeStatus.Running"/> unchanged.
    /// </summary>
    /// <param name="result">The child status to invert.</param>
    /// <returns>The inverted status.</returns>
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
