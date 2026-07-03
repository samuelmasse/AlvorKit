namespace AlvorKit.Engine;

/// <summary>Stores rolling duration metrics for one repeated runtime tick.</summary>
public class TickMetric(TimeSpan duration)
{
    private readonly Stopwatch watch = new();
    private readonly Dictionary<DateTime, double> points = [];
    private readonly Queue<DateTime> queue = [];
    private long ticks;
    private double last;
    private double average;
    private double max;

    /// <summary>Gets the rolling elapsed-time duration.</summary>
    public TimeSpan Duration => duration;

    /// <summary>Gets the latest metric snapshot.</summary>
    public TickMetricValue Value => new(ticks, last, average, max);

    /// <summary>Starts measuring a tick duration.</summary>
    public void Start() => watch.Restart();

    /// <summary>Stops measuring a tick duration and records the elapsed milliseconds.</summary>
    public void End()
    {
        var now = DateTime.UtcNow;

        while (queue.Count > 0 && (now - queue.Peek()).TotalSeconds > duration.TotalSeconds)
            points.Remove(queue.Dequeue());

        double sum = 0;
        max = 0;
        foreach (var point in points)
        {
            if (point.Value > max)
                max = point.Value;

            sum += point.Value;
        }

        average = sum / points.Count;

        watch.Stop();
        last = watch.Elapsed.TotalMilliseconds;
        points.Add(now, last);
        queue.Enqueue(now);
        ticks++;
    }
}
