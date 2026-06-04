namespace BehaviorTree;

/// <summary>
/// The status returned when a behavior tree node is ticked.
/// </summary>
public enum BehaviorTreeStatus
{
    /// <summary>
    /// The node finished successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The node finished with a failure.
    /// </summary>
    Failure,

    /// <summary>
    /// The node is still in progress and should be ticked again.
    /// </summary>
    Running,
}
