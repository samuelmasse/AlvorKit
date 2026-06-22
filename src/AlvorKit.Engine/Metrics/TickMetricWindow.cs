namespace AlvorKit.Engine;

/// <summary>Tracks min, average, max, and latest tick durations over a bounded sample window.</summary>
public sealed class TickMetricWindow
{
    private readonly TickMetric? metric;
    private readonly double[] values;
    private Timer? timer;
    private long previousTicks;
    private TickMetricValue timerValue;
    private int count;
    private int index;

    /// <summary>Creates an old engine timer-backed metric window that samples the metric at its duration.</summary>
    public TickMetricWindow(TickMetric metric)
    {
        this.metric = metric;
        values = [];
    }

    /// <summary>Creates a metric window with the requested sample capacity.</summary>
    public TickMetricWindow(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        values = new double[capacity];
    }

    /// <summary>Gets the current metric value computed from recorded samples.</summary>
    public TickMetricValue Value
    {
        get
        {
            if (metric is not null)
                return timerValue;

            if (count == 0)
                return default;

            var max = double.NegativeInfinity;
            var sum = 0.0;
            for (var i = 0; i < count; i++)
            {
                var value = values[i];
                max = Math.Max(max, value);
                sum += value;
            }

            var nowIndex = (index + values.Length - 1) % values.Length;
            return new(count, values[nowIndex] * 1000, sum / count * 1000, max * 1000);
        }
    }

    /// <summary>Starts timer-backed sampling for an old engine metric window.</summary>
    public void Start() => timer = metric is null ? null : new(_ => Capture(), null, TimeSpan.Zero, metric.Duration);

    /// <summary>Stops timer-backed sampling.</summary>
    public void Stop() => timer?.Dispose();

    /// <summary>Records a duration sample in seconds.</summary>
    public void Add(double delta)
    {
        if (metric is not null)
            throw new InvalidOperationException("Timer-backed metric windows are updated from the source metric.");

        values[index] = delta;
        index = (index + 1) % values.Length;
        count = Math.Min(count + 1, values.Length);
    }

    private void Capture()
    {
        var snapshot = metric!.Value;
        var currentTicks = snapshot.Ticks;
        timerValue = snapshot with { Ticks = currentTicks - previousTicks };
        previousTicks = currentTicks;
    }
}
