using BehaviorTree;
using Moq;
using Xunit;

namespace tests;

public class SequenceNodeTests
{
    private SequenceNode testObject;

    void Init()
    {
        testObject = new SequenceNode("some-sequence");
    }

    [Fact]
    public void can_run_all_children_in_order()
    {
        Init();

        var time = new TimeData();

        var callOrder = 0;

        var mockChild1 = new Mock<IBehaviorTreeNode>();
        mockChild1
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Success)
            .Callback(() =>
             {
                 Assert.Equal(1, ++callOrder);
             });

        var mockChild2 = new Mock<IBehaviorTreeNode>();
        mockChild2
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Success)
            .Callback(() =>
            {
                Assert.Equal(2, ++callOrder);
            });

        testObject.AddChild(mockChild1.Object);
        testObject.AddChild(mockChild2.Object);

        Assert.Equal(BehaviorTreeStatus.Success, testObject.Tick(time));

        Assert.Equal(2, callOrder);

        mockChild1.Verify(m => m.Tick(time), Times.Once());
        mockChild2.Verify(m => m.Tick(time), Times.Once());
    }

    [Fact]
    public void when_first_child_is_running_second_child_is_supressed()
    {
        Init();

        var time = new TimeData();

        var mockChild1 = new Mock<IBehaviorTreeNode>();
        mockChild1
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Running);

        var mockChild2 = new Mock<IBehaviorTreeNode>();

        testObject.AddChild(mockChild1.Object);
        testObject.AddChild(mockChild2.Object);

        Assert.Equal(BehaviorTreeStatus.Running, testObject.Tick(time));

        mockChild1.Verify(m => m.Tick(time), Times.Once());
        mockChild2.Verify(m => m.Tick(time), Times.Never());
    }

    [Fact]
    public void when_first_child_fails_then_entire_sequence_fails()
    {
        Init();

        var time = new TimeData();

        var mockChild1 = new Mock<IBehaviorTreeNode>();
        mockChild1
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Failure);

        var mockChild2 = new Mock<IBehaviorTreeNode>();

        testObject.AddChild(mockChild1.Object);
        testObject.AddChild(mockChild2.Object);

        Assert.Equal(BehaviorTreeStatus.Failure, testObject.Tick(time));

        mockChild1.Verify(m => m.Tick(time), Times.Once());
        mockChild2.Verify(m => m.Tick(time), Times.Never());
    }

    [Fact]
    public void when_second_child_fails_then_entire_sequence_fails()
    {
        Init();

        var time = new TimeData();

        var mockChild1 = new Mock<IBehaviorTreeNode>();
        mockChild1
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Success);

        var mockChild2 = new Mock<IBehaviorTreeNode>();
        mockChild2
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Failure);

        testObject.AddChild(mockChild1.Object);
        testObject.AddChild(mockChild2.Object);

        Assert.Equal(BehaviorTreeStatus.Failure, testObject.Tick(time));

        mockChild1.Verify(m => m.Tick(time), Times.Once());
        mockChild2.Verify(m => m.Tick(time), Times.Once());
    }
}
