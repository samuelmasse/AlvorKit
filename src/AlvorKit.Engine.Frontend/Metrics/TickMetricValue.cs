namespace AlvorKit;

/// <summary>Represents a tick-count and duration snapshot in milliseconds.</summary>
public record struct TickMetricValue(long Ticks, double Last, double Average, double Max);
