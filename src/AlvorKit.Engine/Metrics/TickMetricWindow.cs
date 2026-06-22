namespace AlvorKit.Engine;

/// <summary>Tracks min, average, max, and latest tick durations over a bounded sample window.</summary>
public sealed class TickMetricWindow
{
    private readonly double[] values;
    private int count;
    private int index;

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
            if (count == 0)
                return default;

            var min = double.PositiveInfinity;
            var max = double.NegativeInfinity;
            var sum = 0.0;
            for (var i = 0; i < count; i++)
            {
                var value = values[i];
                min = Math.Min(min, value);
                max = Math.Max(max, value);
                sum += value;
            }

            var nowIndex = (index + values.Length - 1) % values.Length;
            return new(min, sum / count, max, values[nowIndex]);
        }
    }

    /// <summary>Records a duration sample in seconds.</summary>
    public void Add(double delta)
    {
        values[index] = delta;
        index = (index + 1) % values.Length;
        count = Math.Min(count + 1, values.Length);
    }
}
