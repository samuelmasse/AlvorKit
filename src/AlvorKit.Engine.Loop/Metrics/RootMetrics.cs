namespace AlvorKit.Engine.Loop;

/// <summary>Tracks root-loop update and frame durations.</summary>
[Root]
public sealed class RootMetrics
{
    private readonly Stopwatch stopwatch = new();

    /// <summary>Gets update duration samples.</summary>
    public TickMetric Update { get; } = new(10, 100, 1000);

    /// <summary>Gets frame duration samples.</summary>
    public TickMetric Frame { get; } = new(10, 100, 1000);

    /// <summary>Gets total elapsed loop time.</summary>
    public TimeSpan Elapsed => stopwatch.Elapsed;

    /// <summary>Starts elapsed-time tracking.</summary>
    public void Start() => stopwatch.Start();

    /// <summary>Stops elapsed-time tracking.</summary>
    public void Stop() => stopwatch.Stop();
}
