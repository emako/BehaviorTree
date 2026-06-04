using System.Collections.Generic;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// Runs childs nodes in parallel.
/// </summary>
/// <param name="name">
/// Name of the node.
/// </param>
/// <param name="numRequiredToFail">
/// Number of child failures required to terminate with failure.
/// </param>
/// <param name="numRequiredToSucceed">
/// Number of child successess require to terminate with success.
/// </param>
public class ParallelNode(string name, int numRequiredToFail, int numRequiredToSucceed) : IParentBehaviorTreeNode
{
    /// <summary>
    /// List of child nodes.
    /// </summary>
    protected readonly List<IBehaviorTreeNode> children = [];

    public string Name { get; } = name;

    public int NumRequiredToFail { get; } = numRequiredToFail;

    public int NumRequiredToSucceed { get; } = numRequiredToSucceed;

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

    public void AddChild(IBehaviorTreeNode child)
    {
        children.Add(child);
    }
}
