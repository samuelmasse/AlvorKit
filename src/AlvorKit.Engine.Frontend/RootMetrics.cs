namespace AlvorKit;

/// <summary>Tracks root-loop update and frame durations.</summary>
[Root]
public class RootMetrics
{
    private readonly TickMetric frameMetric;
    private readonly TickMetricWindow frameMetricWindow;

    internal TickMetric FrameMetric => frameMetric;

    /// <summary>Gets the rolling frame metric.</summary>
    public TickMetricValue Frame => frameMetric.Value;

    /// <summary>Gets the latest one-second sample of completed frames.</summary>
    public TickMetricValue FrameWindow => frameMetricWindow.Value;

    /// <summary>Creates frame metrics with a one-second rolling duration.</summary>
    public RootMetrics()
    {
        frameMetric = new(TimeSpan.FromSeconds(1));
        frameMetricWindow = new(frameMetric);
    }

    /// <summary>Begins measuring the first frame and sampling frame intervals.</summary>
    internal void Start()
    {
        frameMetric.Start();
        frameMetricWindow.Start();
    }

    /// <summary>Freezes the final frame window on unload.</summary>
    internal void Stop() => frameMetricWindow.Stop();
}
