namespace AlvorKit;

/// <summary>Associates a runnable case with its warmups, retained samples, and summary.</summary>
public record BenchCaseRun(BenchCase Benchmark, BenchMeasurementRun Measurement);
