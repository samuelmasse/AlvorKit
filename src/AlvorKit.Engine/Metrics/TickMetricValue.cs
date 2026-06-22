namespace AlvorKit.Engine;

/// <summary>Represents a sampled duration window in seconds.</summary>
public readonly record struct TickMetricValue(double Min, double Avg, double Max, double Now);
