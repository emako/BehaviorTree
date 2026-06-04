using System;
using System.Collections.Generic;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// Fluent API for constructing behavior trees.
/// </summary>
public class BehaviorTreeBuilder
{
    /// <summary>
    /// The root node produced by the most recent <see cref="End"/> call.
    /// </summary>
    private IBehaviorTreeNode curNode = null;

    /// <summary>
    /// Parent nodes currently open in the fluent build sequence.
    /// </summary>
    private readonly Stack<IParentBehaviorTreeNode> parentNodeStack = new();

    /// <summary>
    /// Creates a synchronous leaf action node.
    /// </summary>
    /// <param name="name">The display name of the action.</param>
    /// <param name="fn">The action invoked on each tick.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ApplicationException">Thrown when the action is not nested inside a parent node.</exception>
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
    /// Creates a leaf action node backed by a <see cref="Task{TResult}"/>.
    /// </summary>
    /// <param name="name">The display name of the action.</param>
    /// <param name="fn">The action invoked on the first tick; subsequent ticks wait for completion.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ApplicationException">Thrown when the action is not nested inside a parent node.</exception>
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
    /// Creates a leaf action node backed by a <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="name">The display name of the action.</param>
    /// <param name="fn">The action invoked on the first tick; subsequent ticks wait for completion.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ApplicationException">Thrown when the action is not nested inside a parent node.</exception>
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
    /// Creates a synchronous condition node that maps <see langword="true"/> to
    /// <see cref="BehaviorTreeStatus.Success"/> and <see langword="false"/> to
    /// <see cref="BehaviorTreeStatus.Failure"/>.
    /// </summary>
    /// <param name="name">The display name of the condition.</param>
    /// <param name="fn">The predicate evaluated on each tick.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public BehaviorTreeBuilder Condition(string name, Func<TimeData, bool> fn)
    {
        return Do(name, t => fn(t) ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Failure);
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Creates an asynchronous condition node backed by a <see cref="Task{TResult}"/> of
    /// <see cref="bool"/>.
    /// </summary>
    /// <param name="name">The display name of the condition.</param>
    /// <param name="fn">The predicate evaluated on each tick.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
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
    /// Creates an asynchronous condition node backed by a <see cref="ValueTask{TResult}"/> of
    /// <see cref="bool"/>.
    /// </summary>
    /// <param name="name">The display name of the condition.</param>
    /// <param name="fn">The predicate evaluated on each tick.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
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
    /// Opens an inverter decorator that swaps success and failure of its single child.
    /// </summary>
    /// <param name="name">The display name of the inverter node.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
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
    /// Opens a sequence composite that runs children in order until one fails or is running.
    /// </summary>
    /// <param name="name">The display name of the sequence node.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
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
    /// Opens a parallel composite that ticks every child each frame.
    /// </summary>
    /// <param name="name">The display name of the parallel node.</param>
    /// <param name="numRequiredToFail">Number of child failures required to terminate with failure.</param>
    /// <param name="numRequiredToSucceed">Number of child successes required to terminate with success.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
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
    /// Opens a selector composite that tries children in order until one succeeds or is running.
    /// </summary>
    /// <param name="name">The display name of the selector node.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
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
    /// Attaches a pre-built sub-tree as a child of the current parent node.
    /// </summary>
    /// <param name="subTree">The sub-tree to attach.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="subTree"/> is <see langword="null"/>.</exception>
    /// <exception cref="ApplicationException">Thrown when there is no open parent node.</exception>
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
    /// Returns the completed behavior tree.
    /// </summary>
    /// <returns>The root node of the built tree.</returns>
    /// <exception cref="ApplicationException">Thrown when no nodes have been created.</exception>
    public IBehaviorTreeNode Build()
    {
        if (curNode == null)
        {
            throw new ApplicationException("Can't create a behavior tree with zero nodes");
        }
        return curNode;
    }

    /// <summary>
    /// Closes the most recently opened composite node and continues building at its parent.
    /// </summary>
    /// <returns>This builder instance for fluent chaining.</returns>
    /// <exception cref="ApplicationException">Thrown when the builder stack is empty.</exception>
    public BehaviorTreeBuilder End()
    {
        curNode = parentNodeStack.Pop();
        return this;
    }
}
