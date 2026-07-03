namespace AlvorKit.Engine;

/// <summary>Samples a source metric at that metric's rolling duration.</summary>
public class TickMetricWindow(TickMetric metric)
{
    private Timer? timer;
    private long prevTicks;
    private TickMetricValue value;

    /// <summary>Gets the last sampled metric snapshot.</summary>
    public TickMetricValue Value => value;

    /// <summary>Starts timer-backed sampling.</summary>
    public void Start() => timer = new(_ => Timer(), null, TimeSpan.Zero, metric.Duration);

    /// <summary>Stops timer-backed sampling.</summary>
    public void Stop() => timer?.Dispose();

    private void Timer()
    {
        long currentTicks = metric.Value.Ticks;
        long delta = currentTicks - prevTicks;
        value = metric.Value with { Ticks = delta };
        prevTicks = currentTicks;
    }
}
