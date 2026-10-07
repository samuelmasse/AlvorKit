namespace AlvorKit;

/// <summary>Captures the start, total duration, and ordered results of one suite execution.</summary>
public record BenchExecution(DateTimeOffset StartedAtUtc, TimeSpan Duration, BenchCaseRun[] Benchmarks);
