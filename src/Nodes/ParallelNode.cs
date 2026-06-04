using System.Collections.Generic;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// A composite node that ticks every child each frame and terminates when enough
/// children succeed or fail.
/// </summary>
/// <param name="name">The display name of the parallel node.</param>
/// <param name="numRequiredToFail">Number of child failures required to terminate with failure.</param>
/// <param name="numRequiredToSucceed">Number of child successes required to terminate with success.</param>
public class ParallelNode(string name, int numRequiredToFail, int numRequiredToSucceed) : IParentBehaviorTreeNode
{
    /// <summary>
    /// Child nodes ticked in parallel each frame.
    /// </summary>
    protected readonly List<IBehaviorTreeNode> children = [];

    /// <summary>
    /// The display name of the node.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Number of child failures required to terminate with failure.
    /// </summary>
    public int NumRequiredToFail { get; } = numRequiredToFail;

    /// <summary>
    /// Number of child successes required to terminate with success.
    /// </summary>
    public int NumRequiredToSucceed { get; } = numRequiredToSucceed;

    /// <inheritdoc />
    public BehaviorTreeStatus Tick(TimeData time)
    {
        var numChildrenSuceeded = 0;
        var numChildrenFailed = 0;

        foreach (var child in children)
        {
            var childStatus = child.Tick(time);
            switch (childStatus)
            {
                case BehaviorTreeStatus.Success: ++numChildrenSuceeded; break;
                case BehaviorTreeStatus.Failure: ++numChildrenFailed; break;
            }
        }

        if (NumRequiredToSucceed > 0 && numChildrenSuceeded >= NumRequiredToSucceed)
        {
            return BehaviorTreeStatus.Success;
        }

        if (NumRequiredToFail > 0 && numChildrenFailed >= NumRequiredToFail)
        {
            return BehaviorTreeStatus.Failure;
        }

        return BehaviorTreeStatus.Running;
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public async Task<BehaviorTreeStatus> TickAsync(TimeData time)
    {
        var numChildrenSuceeded = 0;
        var numChildrenFailed = 0;

        foreach (var child in children)
        {
            var childStatus = await child.TickAsync(time).ConfigureAwait(false);
            switch (childStatus)
            {
                case BehaviorTreeStatus.Success: ++numChildrenSuceeded; break;
                case BehaviorTreeStatus.Failure: ++numChildrenFailed; break;
            }
        }

        if (NumRequiredToSucceed > 0 && numChildrenSuceeded >= NumRequiredToSucceed)
        {
            return BehaviorTreeStatus.Success;
        }

        if (NumRequiredToFail > 0 && numChildrenFailed >= NumRequiredToFail)
        {
            return BehaviorTreeStatus.Failure;
        }

        return BehaviorTreeStatus.Running;
    }
#endif

    /// <inheritdoc />
    public void AddChild(IBehaviorTreeNode child)
    {
        children.Add(child);
    }
}
