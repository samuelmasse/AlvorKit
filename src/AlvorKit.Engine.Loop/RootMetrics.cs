namespace AlvorKit.Engine.Loop;

/// <summary>Tracks root-loop update and frame durations.</summary>
[Root]
public class RootMetrics
{
    private readonly TickMetric frameMetric;
    private readonly TickMetricWindow frameMetricWindow;

    internal TickMetric FrameMetric => frameMetric;

    internal TickMetricWindow FrameMetricWindow => frameMetricWindow;

    /// <summary>Gets the rolling frame metric.</summary>
    public TickMetricValue Frame => frameMetric.Value;

    /// <summary>Gets the latest timer-backed frame metric window.</summary>
    public TickMetricValue FrameWindow => frameMetricWindow.Value;

    /// <summary>Creates frame metrics with a one-second rolling duration.</summary>
    public RootMetrics()
    {
        frameMetric = new(TimeSpan.FromSeconds(1));
        frameMetricWindow = new(frameMetric);
    }

    /// <summary>Starts timer-backed metric sampling.</summary>
    internal void Start() => frameMetricWindow.Start();

    /// <summary>Stops timer-backed metric sampling.</summary>
    internal void Stop() => frameMetricWindow.Stop();
}
