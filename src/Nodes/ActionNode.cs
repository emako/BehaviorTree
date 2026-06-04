using System;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// A leaf node that executes a user-supplied action on each tick.
/// </summary>
public class ActionNode : IBehaviorTreeNode
{
    /// <summary>
    /// The display name of the node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Synchronous action invoked when no async delegate is configured.
    /// </summary>
    protected readonly Func<TimeData, BehaviorTreeStatus> syncFn;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Task-based action invoked on the first tick of an async action.
    /// </summary>
    protected readonly Func<TimeData, Task<BehaviorTreeStatus>> taskFn;

    /// <summary>
    /// The in-flight task started by a previous tick, if any.
    /// </summary>
    protected Task<BehaviorTreeStatus> pendingTask;
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// ValueTask-based action invoked on the first tick of an async action.
    /// </summary>
    protected readonly Func<TimeData, ValueTask<BehaviorTreeStatus>> valueTaskFn;

    /// <summary>
    /// The in-flight value task started by a previous tick, if any.
    /// </summary>
    protected ValueTask<BehaviorTreeStatus> pendingValueTask;

    /// <summary>
    /// Indicates whether <see cref="pendingValueTask"/> is active.
    /// </summary>
    protected bool valueTaskPending;
#endif

    /// <summary>
    /// Initializes a synchronous action node.
    /// </summary>
    /// <param name="name">The display name of the node.</param>
    /// <param name="fn">The action invoked on each tick.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fn"/> is <see langword="null"/>.</exception>
    public ActionNode(string name, Func<TimeData, BehaviorTreeStatus> fn)
    {
        Name = name;
        syncFn = fn ?? throw new ArgumentNullException(nameof(fn));
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Initializes a task-based action node.
    /// </summary>
    /// <param name="name">The display name of the node.</param>
    /// <param name="fn">The action invoked on the first tick; later ticks wait for completion.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fn"/> is <see langword="null"/>.</exception>
    public ActionNode(string name, Func<TimeData, Task<BehaviorTreeStatus>> fn)
    {
        Name = name;
        taskFn = fn ?? throw new ArgumentNullException(nameof(fn));
    }
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Initializes a value-task-based action node.
    /// </summary>
    /// <param name="name">The display name of the node.</param>
    /// <param name="fn">The action invoked on the first tick; later ticks wait for completion.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fn"/> is <see langword="null"/>.</exception>
    public ActionNode(string name, Func<TimeData, ValueTask<BehaviorTreeStatus>> fn)
    {
        Name = name;
        valueTaskFn = fn ?? throw new ArgumentNullException(nameof(fn));
    }
#endif

    /// <inheritdoc />
    public BehaviorTreeStatus Tick(TimeData time)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        if (valueTaskFn != null)
        {
            return TickValueTaskSync(time);
        }
#endif
#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
        if (taskFn != null)
        {
            return TickTaskSync(time);
        }
#endif
        return syncFn(time);
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public Task<BehaviorTreeStatus> TickAsync(TimeData time)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        if (valueTaskFn != null)
        {
            return TickValueTaskAsync(time).AsTask();
        }
#endif
        if (taskFn != null)
        {
            return TickTaskAsync(time);
        }

        return Task.FromResult(syncFn(time));
    }

    /// <summary>
    /// Synchronously advances a pending task-based action.
    /// </summary>
    /// <param name="time">Time information for the current tick.</param>
    /// <returns>The status of the action after this tick.</returns>
    private BehaviorTreeStatus TickTaskSync(TimeData time)
    {
        pendingTask ??= taskFn(time);

        if (!pendingTask.IsCompleted)
        {
            return BehaviorTreeStatus.Running;
        }

        try
        {
            return pendingTask.GetAwaiter().GetResult();
        }
        finally
        {
            pendingTask = null;
        }
    }

    /// <summary>
    /// Asynchronously advances a pending task-based action.
    /// </summary>
    /// <param name="time">Time information for the current tick.</param>
    /// <returns>A task that completes with the status of the action after this tick.</returns>
    private async Task<BehaviorTreeStatus> TickTaskAsync(TimeData time)
    {
        pendingTask ??= taskFn(time);

        if (!pendingTask.IsCompleted)
        {
            return BehaviorTreeStatus.Running;
        }

        try
        {
            return await pendingTask.ConfigureAwait(false);
        }
        finally
        {
            pendingTask = null;
        }
    }
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Synchronously advances a pending value-task-based action.
    /// </summary>
    /// <param name="time">Time information for the current tick.</param>
    /// <returns>The status of the action after this tick.</returns>
    private BehaviorTreeStatus TickValueTaskSync(TimeData time)
    {
        if (!valueTaskPending)
        {
            pendingValueTask = valueTaskFn.Invoke(time);
            valueTaskPending = true;
        }

        if (!pendingValueTask.IsCompleted)
        {
            return BehaviorTreeStatus.Running;
        }

        try
        {
            return pendingValueTask.GetAwaiter().GetResult();
        }
        finally
        {
            valueTaskPending = false;
        }
    }

    /// <summary>
    /// Asynchronously advances a pending value-task-based action.
    /// </summary>
    /// <param name="time">Time information for the current tick.</param>
    /// <returns>A value task that completes with the status of the action after this tick.</returns>
    private async ValueTask<BehaviorTreeStatus> TickValueTaskAsync(TimeData time)
    {
        if (!valueTaskPending)
        {
            pendingValueTask = valueTaskFn.Invoke(time);
            valueTaskPending = true;
        }

        if (!pendingValueTask.IsCompleted)
        {
            return BehaviorTreeStatus.Running;
        }

        try
        {
            return await pendingValueTask.ConfigureAwait(false);
        }
        finally
        {
            valueTaskPending = false;
        }
    }
#endif
}
