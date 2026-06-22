namespace AlvorKit.Engine;

/// <summary>Publishes a metric snapshot after each fixed elapsed-time period.</summary>
public sealed class TickMetricPeriodWindow(TimeSpan period)
{
    private readonly double periodSeconds = period.TotalSeconds > 0 ? period.TotalSeconds : throw new ArgumentOutOfRangeException(nameof(period));
    private double elapsedSeconds;
    private double pendingSeconds;
    private double pendingMaxMilliseconds;
    private long pendingTicks;
    private long ticks;
    private double lastMilliseconds;
    private double averageMilliseconds;
    private double maxMilliseconds;

    /// <summary>Gets the number of ticks captured by the last completed period.</summary>
    public long Ticks => ticks;

    /// <summary>Gets the latest tick duration captured by the last completed period in milliseconds.</summary>
    public double Last => lastMilliseconds;

    /// <summary>Gets the average tick duration captured by the last completed period in milliseconds.</summary>
    public double Average => averageMilliseconds;

    /// <summary>Gets the largest tick duration captured by the last completed period in milliseconds.</summary>
    public double Max => maxMilliseconds;

    /// <summary>Records one duration sample in seconds and refreshes the published snapshot after the period elapses.</summary>
    public void Add(double delta)
    {
        var milliseconds = delta * 1000;
        elapsedSeconds += delta;
        pendingSeconds += delta;
        pendingTicks++;
        pendingMaxMilliseconds = Math.Max(pendingMaxMilliseconds, milliseconds);

        if (elapsedSeconds < periodSeconds)
            return;

        ticks = pendingTicks;
        lastMilliseconds = milliseconds;
        averageMilliseconds = pendingSeconds / pendingTicks * 1000;
        maxMilliseconds = pendingMaxMilliseconds;
        pendingTicks = 0;
        pendingSeconds = 0;
        pendingMaxMilliseconds = 0;
        elapsedSeconds %= periodSeconds;
    }
}
