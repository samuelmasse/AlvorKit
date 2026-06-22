namespace AlvorKit.Engine;

/// <summary>Stores several sample windows for one repeated runtime tick.</summary>
public sealed class TickMetric
{
    private readonly TickMetricWindow[] windows;

    /// <summary>Creates a metric with caller-selected sample window sizes.</summary>
    public TickMetric(params int[] windowSizes)
    {
        windows = new TickMetricWindow[windowSizes.Length];
        for (var i = 0; i < windows.Length; i++)
            windows[i] = new(windowSizes[i]);
    }

    /// <summary>Gets a sampled duration window by index.</summary>
    public TickMetricValue this[int index] => windows[index].Value;

    /// <summary>Records one duration sample in every window.</summary>
    public void Add(double delta)
    {
        foreach (var window in windows)
            window.Add(delta);
    }
}
