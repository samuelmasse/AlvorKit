namespace AlvorKit.Engine;

/// <summary>Represents a tick-count and duration snapshot in milliseconds.</summary>
public readonly record struct TickMetricValue(long Ticks, double Last, double Average, double Max)
{
    /// <summary>Gets the latest recorded duration in milliseconds.</summary>
    public double Now => Last;

    /// <summary>Gets the average recorded duration in milliseconds.</summary>
    public double Avg => Average;
}
