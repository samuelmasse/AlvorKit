namespace AlvorKit.Engine;

/// <summary>Stores duration metrics for one repeated runtime tick.</summary>
public sealed class TickMetric
{
    private readonly TickMetricWindow[] windows;
    private readonly Stopwatch watch = new();
    private readonly Dictionary<DateTime, double> points = [];
    private readonly Queue<DateTime> queue = [];
    private readonly TimeSpan duration;
    private long ticks;
    private double lastMilliseconds;
    private double totalMilliseconds;
    private double averageMilliseconds;
    private double maxMilliseconds;

    /// <summary>Creates the old engine stopwatch-backed metric with a rolling elapsed-time duration.</summary>
    public TickMetric(TimeSpan duration)
    {
        this.duration = duration;
        windows = [];
    }

    /// <summary>Creates a metric with caller-selected sample window sizes.</summary>
    public TickMetric(params int[] windowSizes)
    {
        windows = new TickMetricWindow[windowSizes.Length];
        for (var i = 0; i < windows.Length; i++)
            windows[i] = new(windowSizes[i]);
    }

    /// <summary>Gets the old engine rolling elapsed-time duration.</summary>
    public TimeSpan Duration => duration;

    /// <summary>Gets the old engine snapshot value.</summary>
    public TickMetricValue Value => new(ticks, lastMilliseconds, averageMilliseconds, maxMilliseconds);

    /// <summary>Gets a sampled duration window by index.</summary>
    public TickMetricValue this[int index] => windows[index].Value;

    /// <summary>Gets the total number of recorded ticks.</summary>
    public long Ticks => ticks;

    /// <summary>Gets the latest recorded duration in milliseconds.</summary>
    public double Last => lastMilliseconds;

    /// <summary>Gets the average duration across all recorded ticks in milliseconds.</summary>
    public double Average => averageMilliseconds;

    /// <summary>Gets the largest recorded duration in milliseconds.</summary>
    public double Max => maxMilliseconds;

    /// <summary>Starts measuring an old engine tick duration.</summary>
    public void Start() => watch.Restart();

    /// <summary>Stops measuring an old engine tick duration and records the elapsed milliseconds.</summary>
    public void End()
    {
        watch.Stop();
        AddMeasured(watch.Elapsed.TotalMilliseconds);
    }

    /// <summary>Records one duration sample in every window.</summary>
    public void Add(double delta)
    {
        AddSample(delta * 1000);
        foreach (var window in windows)
            window.Add(delta);
    }

    private void AddMeasured(double milliseconds)
    {
        var now = DateTime.UtcNow;
        while (queue.Count > 0 && now - queue.Peek() > duration)
            points.Remove(queue.Dequeue());

        points[now] = milliseconds;
        queue.Enqueue(now);
        ticks++;
        lastMilliseconds = milliseconds;

        var sum = 0.0;
        maxMilliseconds = 0;
        foreach (var point in points.Values)
        {
            sum += point;
            maxMilliseconds = Math.Max(maxMilliseconds, point);
        }

        averageMilliseconds = sum / points.Count;
    }

    private void AddSample(double milliseconds)
    {
        ticks++;
        lastMilliseconds = milliseconds;
        totalMilliseconds += milliseconds;
        maxMilliseconds = Math.Max(maxMilliseconds, milliseconds);
        averageMilliseconds = totalMilliseconds / ticks;
    }
}
