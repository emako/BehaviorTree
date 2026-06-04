using System;
using System.Collections.Generic;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// Fluent API for building a behavior tree.
/// </summary>
public class BehaviorTreeBuilder
{
    /// <summary>
    /// Last node created.
    /// </summary>
    private IBehaviorTreeNode curNode = null;

    /// <summary>
    /// Stack node nodes that we are build via the fluent API.
    /// </summary>
    private readonly Stack<IParentBehaviorTreeNode> parentNodeStack = new();

    /// <summary>
    /// Create an action node.
    /// </summary>
    public BehaviorTreeBuilder Do(string name, Func<TimeData, BehaviorTreeStatus> fn)
    {
        if (parentNodeStack.Count <= 0)
        {
            throw new ApplicationException("Can't create an unnested ActionNode, it must be a leaf node.");
        }

        var actionNode = new ActionNode(name, fn);
        parentNodeStack.Peek().AddChild(actionNode);
        return this;
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Create an action node that returns a task.
    /// </summary>
    public BehaviorTreeBuilder Do(string name, Func<TimeData, Task<BehaviorTreeStatus>> fn)
    {
        if (parentNodeStack.Count <= 0)
        {
            throw new ApplicationException("Can't create an unnested ActionNode, it must be a leaf node.");
        }

        var actionNode = new ActionNode(name, fn);
        parentNodeStack.Peek().AddChild(actionNode);
        return this;
    }
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Create an action node that returns a value task.
    /// </summary>
    public BehaviorTreeBuilder DoValue(string name, Func<TimeData, ValueTask<BehaviorTreeStatus>> fn)
    {
        if (parentNodeStack.Count <= 0)
        {
            throw new ApplicationException("Can't create an unnested ActionNode, it must be a leaf node.");
        }

        var actionNode = new ActionNode(name, fn);
        parentNodeStack.Peek().AddChild(actionNode);
        return this;
    }
#endif

    /// <summary>
    /// Like an action node... but the function can return true/false and is mapped to success/failure.
    /// </summary>
    public BehaviorTreeBuilder Condition(string name, Func<TimeData, bool> fn)
    {
        return Do(name, t => fn(t) ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Failure);
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Like a condition node, but the predicate returns a task of bool.
    /// </summary>
    public BehaviorTreeBuilder Condition(string name, Func<TimeData, Task<bool>> fn)
    {
        async Task<BehaviorTreeStatus> Action(TimeData t)
        {
            return await fn(t).ConfigureAwait(false)
                ? BehaviorTreeStatus.Success
                : BehaviorTreeStatus.Failure;
        }
        return Do(name, Action);
    }
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Like a condition node, but the predicate returns a value task of bool.
    /// </summary>
    public BehaviorTreeBuilder Condition(string name, Func<TimeData, ValueTask<bool>> fn)
    {
        async ValueTask<BehaviorTreeStatus> Action(TimeData t)
        {
            return await fn(t).ConfigureAwait(false)
                ? BehaviorTreeStatus.Success
                : BehaviorTreeStatus.Failure;
        }
        return DoValue(name, Action);
    }
#endif

    /// <summary>
    /// Create an inverter node that inverts the success/failure of its children.
    /// </summary>
    public BehaviorTreeBuilder Inverter(string name)
    {
        var inverterNode = new InverterNode(name);

        if (parentNodeStack.Count > 0)
        {
            parentNodeStack.Peek().AddChild(inverterNode);
        }

        parentNodeStack.Push(inverterNode);
        return this;
    }

    /// <summary>
    /// Create a sequence node.
    /// </summary>
    public BehaviorTreeBuilder Sequence(string name)
    {
        var sequenceNode = new SequenceNode(name);

        if (parentNodeStack.Count > 0)
        {
            parentNodeStack.Peek().AddChild(sequenceNode);
        }

        parentNodeStack.Push(sequenceNode);
        return this;
    }

    /// <summary>
    /// Create a parallel node.
    /// </summary>
    public BehaviorTreeBuilder Parallel(string name, int numRequiredToFail, int numRequiredToSucceed)
    {
        var parallelNode = new ParallelNode(name, numRequiredToFail, numRequiredToSucceed);

        if (parentNodeStack.Count > 0)
        {
            parentNodeStack.Peek().AddChild(parallelNode);
        }

        parentNodeStack.Push(parallelNode);
        return this;
    }

    /// <summary>
    /// Create a selector node.
    /// </summary>
    public BehaviorTreeBuilder Selector(string name)
    {
        var selectorNode = new SelectorNode(name);

        if (parentNodeStack.Count > 0)
        {
            parentNodeStack.Peek().AddChild(selectorNode);
        }

        parentNodeStack.Push(selectorNode);
        return this;
    }

    /// <summary>
    /// Splice a sub tree into the parent tree.
    /// </summary>
    public BehaviorTreeBuilder Splice(IBehaviorTreeNode subTree)
    {
        _ = subTree ?? throw new ArgumentNullException(nameof(subTree));
        if (parentNodeStack.Count <= 0)
        {
            throw new ApplicationException("Can't splice an unnested sub-tree, there must be a parent-tree.");
        }

        parentNodeStack.Peek().AddChild(subTree);
        return this;
    }

    /// <summary>
    /// Build the actual tree.
    /// </summary>
    public IBehaviorTreeNode Build()
    {
        if (curNode == null)
        {
            throw new ApplicationException("Can't create a behavior tree with zero nodes");
        }
        return curNode;
    }

    /// <summary>
    /// Ends a sequence of children.
    /// </summary>
    public BehaviorTreeBuilder End()
    {
        curNode = parentNodeStack.Pop();
        return this;
    }
}
