namespace BehaviorTree;

/// <summary>
/// Represents time. Used to pass time values to behavior tree nodes.
/// </summary>
public readonly struct TimeData(float deltaTime)
{
    public readonly float DeltaTime = deltaTime;
}
