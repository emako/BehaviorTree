using BehaviorTree;
using Moq;
using Xunit;

public class SelectorNodeTests
{
    private SelectorNode testObject;

    void Init()
    {
        testObject = new SelectorNode("some-selector");
    }

    [Fact]
    public void runs_the_first_node_if_it_succeeds()
    {
        Init();

        var time = new TimeData();

        var mockChild1 = new Mock<IBehaviorTreeNode>();
        mockChild1
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Success);

        var mockChild2 = new Mock<IBehaviorTreeNode>();

        testObject.AddChild(mockChild1.Object);
        testObject.AddChild(mockChild2.Object);

        Assert.Equal(BehaviorTreeStatus.Success, testObject.Tick(time));

        mockChild1.Verify(m => m.Tick(time), Times.Once());
        mockChild2.Verify(m => m.Tick(time), Times.Never());
    }

    [Fact]
    public void stops_on_the_first_node_when_it_is_running()
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
    public void runs_the_second_node_if_the_first_fails()
    {
        Init();

        var time = new TimeData();

        var mockChild1 = new Mock<IBehaviorTreeNode>();
        mockChild1
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Failure);

        var mockChild2 = new Mock<IBehaviorTreeNode>();
        mockChild2
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Success);

        testObject.AddChild(mockChild1.Object);
        testObject.AddChild(mockChild2.Object);

        Assert.Equal(BehaviorTreeStatus.Success, testObject.Tick(time));

        mockChild1.Verify(m => m.Tick(time), Times.Once());
        mockChild2.Verify(m => m.Tick(time), Times.Once());
    }

    [Fact]
    public void fails_when_all_children_fail()
    {
        Init();

        var time = new TimeData();

        var mockChild1 = new Mock<IBehaviorTreeNode>();
        mockChild1
            .Setup(m => m.Tick(time))
            .Returns(BehaviorTreeStatus.Failure);

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
