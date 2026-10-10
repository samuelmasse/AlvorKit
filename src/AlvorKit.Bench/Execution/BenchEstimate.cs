namespace AlvorKit;

/// <summary>Mean of independent launch estimates and a two-sided Student t interval.</summary>
public record BenchEstimate(int Launches, double Mean, double StandardDeviation, double? Lower95, double? Upper95);
