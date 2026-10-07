namespace AlvorKit;

/// <summary>Allocation totals for a measurement and its timed workload; divide by operation count for per-operation costs.</summary>
public record BenchJsonAllocations(double MeasurementBytes, double? MeasurementObjects, double? WorkloadBytes);
