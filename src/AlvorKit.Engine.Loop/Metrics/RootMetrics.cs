namespace AlvorKit.Engine.Loop;

/// <summary>Tracks root-loop update and frame durations.</summary>
[Root]
public sealed class RootMetrics
{
    private readonly Stopwatch stopwatch = new();
    private readonly TickMetric update = new(10, 100, 1000);
    private readonly TickMetric frame = new(10, 100, 1000);
    private readonly TickMetricPeriodWindow frameWindow = new(TimeSpan.FromSeconds(1));

    /// <summary>Gets update duration samples.</summary>
    public TickMetric Update => update;

    /// <summary>Gets frame duration samples.</summary>
    public TickMetric Frame => frame;

    /// <summary>Gets the latest one-second frame snapshot used by slower FPS HUDs.</summary>
    public TickMetricPeriodWindow FrameWindow => frameWindow;

    /// <summary>Gets total elapsed loop time.</summary>
    public TimeSpan Elapsed => stopwatch.Elapsed;

    /// <summary>Starts elapsed-time tracking.</summary>
    public void Start() => stopwatch.Start();

    /// <summary>Stops elapsed-time tracking.</summary>
    public void Stop() => stopwatch.Stop();

    /// <summary>Records one update duration in seconds.</summary>
    internal void AddUpdate(double delta) => update.Add(delta);

    /// <summary>Records one frame duration in seconds and refreshes the slower frame window when its period elapses.</summary>
    internal void AddFrame(double delta)
    {
        frame.Add(delta);
        frameWindow.Add(delta);
    }
}
