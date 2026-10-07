namespace AlvorKit;

/// <summary>Validated selection, sampling, and reporting options for one invocation.</summary>
[Bench]
public record BenchCommand(
    BenchCommandKind Kind,
    string[] IncludePatterns,
    string[] ExcludePatterns,
    int WarmupCount,
    int SampleCount,
    string? JsonPath,
    BenchDisplay Display);
