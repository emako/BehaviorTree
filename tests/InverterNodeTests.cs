using BehaviorTree;
using Moq;
using System;
using Xunit;

public class InverterNodeTests
{
    private InverterNode testObject;

    void Init()
    {
        testObject = new InverterNode("some-node");
    }

    [Fact]
    public void ticking_with_no_child_node_throws_exception()
    {
        Init();

        Assert.Throws<ApplicationException>(
            () => testObject.Tick(new TimeData())
        );
    }

    [Fact]
    public void inverts_success_of_child_node()
    {
        Init();

        var time = new TimeData();

        var mockChildNode = new Mock<IBehaviorTreeNode>();
        mockChildNode
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Success);

        testObject.AddChild(mockChildNode.Object);

        Assert.Equal(BehaviorTreeStatus.Failure, testObject.Tick(time));

        mockChildNode.Verify(m => m.Tick(time), Times.Once());
    }

    [Fact]
    public void inverts_failure_of_child_node()
    {
        Init();

        var time = new TimeData();

        var mockChildNode = new Mock<IBehaviorTreeNode>();
        mockChildNode
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Failure);

        testObject.AddChild(mockChildNode.Object);

        Assert.Equal(BehaviorTreeStatus.Success, testObject.Tick(time));

        mockChildNode.Verify(m => m.Tick(time), Times.Once());
    }

    [Fact]
    public void pass_through_running_of_child_node()
    {
        Init();

        var time = new TimeData();

        var mockChildNode = new Mock<IBehaviorTreeNode>();
        mockChildNode
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Running);

        testObject.AddChild(mockChildNode.Object);

        Assert.Equal(BehaviorTreeStatus.Running, testObject.Tick(time));

        mockChildNode.Verify(m => m.Tick(time), Times.Once());
    }

    [Fact]
    public void adding_more_than_a_single_child_throws_exception()
    {
        Init();

        var mockChildNode1 = new Mock<IBehaviorTreeNode>();
        testObject.AddChild(mockChildNode1.Object);

        var mockChildNode2 = new Mock<IBehaviorTreeNode>();
        Assert.Throws<ApplicationException>(() =>
            testObject.AddChild(mockChildNode2.Object)
        );
    }
}
