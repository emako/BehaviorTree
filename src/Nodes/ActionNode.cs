using System;
#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace BehaviorTree;

/// <summary>
/// A behavior tree leaf node for running an action.
/// </summary>
public class ActionNode : IBehaviorTreeNode
{
    /// <summary>
    /// The name of the node.
    /// </summary>
    private string name;

    /// <summary>
    /// Function to invoke for the action.
    /// </summary>
    private Func<TimeData, BehaviorTreeStatus> syncFn;

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Async function to invoke for the action.
    /// </summary>
    private Func<TimeData, Task<BehaviorTreeStatus>> taskFn;

    /// <summary>
    /// Pending task started by the previous tick.
    /// </summary>
    private Task<BehaviorTreeStatus> pendingTask;
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// ValueTask function to invoke for the action.
    /// </summary>
    private Func<TimeData, ValueTask<BehaviorTreeStatus>> valueTaskFn;

    /// <summary>
    /// Pending value task started by the previous tick.
    /// </summary>
    private ValueTask<BehaviorTreeStatus> pendingValueTask;

    /// <summary>
    /// Whether a value task is currently pending.
    /// </summary>
    private bool valueTaskPending;
#endif

    public ActionNode(string name, Func<TimeData, BehaviorTreeStatus> fn)
    {
        this.name = name;
        syncFn = fn ?? throw new ArgumentNullException(nameof(fn));
    }

#if NET452_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    public ActionNode(string name, Func<TimeData, Task<BehaviorTreeStatus>> fn)
    {
        this.name = name;
        taskFn = fn ?? throw new ArgumentNullException(nameof(fn));
    }
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    public ActionNode(string name, Func<TimeData, ValueTask<BehaviorTreeStatus>> fn)
    {
        this.name = name;
        valueTaskFn = fn ?? throw new ArgumentNullException(nameof(fn));
    }
#endif

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

    private BehaviorTreeStatus TickTaskSync(TimeData time)
    {
        if (pendingTask == null)
        {
            pendingTask = taskFn(time);
        }

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

    private async Task<BehaviorTreeStatus> TickTaskAsync(TimeData time)
    {
        if (pendingTask == null)
        {
            pendingTask = taskFn(time);
        }

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
    private BehaviorTreeStatus TickValueTaskSync(TimeData time)
    {
        if (!valueTaskPending)
        {
            pendingValueTask = valueTaskFn(time);
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

    private async ValueTask<BehaviorTreeStatus> TickValueTaskAsync(TimeData time)
    {
        if (!valueTaskPending)
        {
            pendingValueTask = valueTaskFn(time);
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
