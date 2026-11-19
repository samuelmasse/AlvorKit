namespace AlvorKit;

/// <summary>Stores rolling duration metrics for one repeated runtime tick.</summary>
public class TickMetric(TimeSpan duration)
{
    private readonly Stopwatch watch = new();
    private readonly Queue<(long End, double Milliseconds)> samples = [];
    private TickMetricValue value;

    /// <summary>Gets the rolling elapsed-time duration.</summary>
    public TimeSpan Duration => duration;

    /// <summary>Gets the lifetime tick count and rolling durations as of the latest completed tick.</summary>
    public TickMetricValue Value => value;

    /// <summary>Starts measuring a tick duration.</summary>
    public void Start() => watch.Restart();

    /// <summary>Stops measuring a tick duration and records the elapsed milliseconds.</summary>
    public void End()
    {
        watch.Stop();
        Record(Stopwatch.GetTimestamp(), watch.Elapsed.TotalMilliseconds);
    }

    /// <summary>Records a completed tick at its monotonic end time.</summary>
    internal void Record(long now, double milliseconds)
    {
        RemoveExpired(now);
        samples.Enqueue((now, milliseconds));
        value = Summarize() with { Ticks = value.Ticks + 1 };
    }

    /// <summary>Counts and summarizes ticks completed within the interval ending at the supplied time.</summary>
    internal TickMetricValue SampleWindow(long now)
    {
        RemoveExpired(now);
        return Summarize();
    }

    private void RemoveExpired(long now)
    {
        while (samples.TryPeek(out var oldest) && Stopwatch.GetElapsedTime(oldest.End, now) >= duration)
            samples.Dequeue();
    }

    private TickMetricValue Summarize()
    {
        double sum = 0;
        double max = 0;
        double last = 0;
        foreach (var sample in samples)
        {
            last = sample.Milliseconds;
            sum += last;
            max = Math.Max(max, last);
        }

        return samples.Count == 0 ? default : new(samples.Count, last, sum / samples.Count, max);
    }
}
