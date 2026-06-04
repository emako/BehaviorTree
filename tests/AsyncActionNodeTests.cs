using BehaviorTree;
using System;
using System.Threading.Tasks;
using Xunit;

public class AsyncActionNodeTests
{
    [Fact]
    public async Task task_action_stays_running_until_complete()
    {
        var time = new TimeData();
        var invokeCount = 0;
        var completionSource = new TaskCompletionSource<BehaviorTreeStatus>();

        var node = new ActionNode(
            "async-action",
            t =>
            {
                ++invokeCount;
                return completionSource.Task;
            });

        Assert.Equal(BehaviorTreeStatus.Running, await node.TickAsync(time));
        Assert.Equal(1, invokeCount);

        Assert.Equal(BehaviorTreeStatus.Running, await node.TickAsync(time));
        Assert.Equal(1, invokeCount);

        completionSource.SetResult(BehaviorTreeStatus.Success);
        Assert.Equal(BehaviorTreeStatus.Success, await node.TickAsync(time));
        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public async Task value_task_action_stays_running_until_complete()
    {
        var time = new TimeData();
        var invokeCount = 0;
        var completionSource = new TaskCompletionSource<BehaviorTreeStatus>();

        var node = new ActionNode(
            "value-task-action",
            t =>
            {
                ++invokeCount;
                return new ValueTask<BehaviorTreeStatus>(completionSource.Task);
            });

        Assert.Equal(BehaviorTreeStatus.Running, await node.TickAsync(time));
        Assert.Equal(1, invokeCount);

        completionSource.SetResult(BehaviorTreeStatus.Failure);
        Assert.Equal(BehaviorTreeStatus.Failure, await node.TickAsync(time));
        Assert.Equal(1, invokeCount);
    }

    [Fact]
    public async Task builder_can_create_task_action()
    {
        var invokeCount = 0;
        var completionSource = new TaskCompletionSource<BehaviorTreeStatus>();

        Func<TimeData, Task<BehaviorTreeStatus>> action = t =>
        {
            ++invokeCount;
            return completionSource.Task;
        };

        var tree = new BehaviorTreeBuilder()
            .Sequence("sequence")
                .Do("async-action", action)
            .End()
            .Build();

        Assert.Equal(BehaviorTreeStatus.Running, await tree.TickAsync(new TimeData()));
        Assert.Equal(1, invokeCount);

        completionSource.SetResult(BehaviorTreeStatus.Success);
        Assert.Equal(BehaviorTreeStatus.Success, await tree.TickAsync(new TimeData()));
        Assert.Equal(1, invokeCount);
    }
}
