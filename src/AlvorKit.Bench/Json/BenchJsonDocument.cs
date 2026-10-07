namespace AlvorKit;

/// <summary>Versioned benchmark result document with environment and selection provenance.</summary>
public record BenchJsonDocument(
    int SchemaVersion,
    string Suite,
    DateTimeOffset StartedAtUtc,
    double DurationMilliseconds,
    bool ObjectTrackingEnabled,
    BenchJsonSelection Selection,
    BenchJsonEnvironment Environment,
    BenchJsonBenchmark[] Benchmarks);
