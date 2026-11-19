namespace AlvorKit;

/// <summary>Samples a trailing interval on the metric owner's thread, at most once per duration.</summary>
public class TickMetricWindow(TickMetric metric)
{
    private long? sampledAt;
    private TickMetricValue value;

    /// <summary>Gets the interval snapshot, refreshing it when due.</summary>
    public TickMetricValue Value => Read(Stopwatch.GetTimestamp());

    /// <summary>Samples the current interval and starts periodic refreshes on read.</summary>
    public void Start() => Sample(Stopwatch.GetTimestamp());

    /// <summary>Freezes the latest interval snapshot.</summary>
    public void Stop()
    {
        Read(Stopwatch.GetTimestamp());
        sampledAt = null;
    }

    /// <summary>Returns the snapshot due at the given monotonic time.</summary>
    internal TickMetricValue Read(long now)
    {
        if (sampledAt is long previous && Stopwatch.GetElapsedTime(previous, now) >= metric.Duration)
            Sample(now);
        return value;
    }

    private void Sample(long now)
    {
        value = metric.SampleWindow(now);
        sampledAt = now;
    }
}
