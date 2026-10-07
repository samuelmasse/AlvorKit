namespace AlvorKit;

/// <summary>Exported case metadata, samples, and summary; allocation scopes remain distinct.</summary>
public record BenchJsonBenchmark(
    string Id,
    string Description,
    string Unit,
    BenchJsonComparison? Comparison,
    BenchJsonSample[] Warmups,
    BenchJsonSample[] Samples,
    BenchJsonSummary Summary);
