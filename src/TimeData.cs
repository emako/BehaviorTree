namespace BehaviorTree;

/// <summary>
/// Time information passed to behavior tree nodes on each tick.
/// </summary>
/// <param name="deltaTime">Elapsed time in seconds since the previous tick.</param>
public readonly struct TimeData(float deltaTime)
{
    /// <summary>
    /// Elapsed time in seconds since the previous tick.
    /// </summary>
    public readonly float DeltaTime = deltaTime;
}
