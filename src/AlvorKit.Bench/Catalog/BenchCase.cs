namespace AlvorKit;

/// <summary>A runnable catalog leaf with its stable path, inherited description, and optional baseline.</summary>
public record BenchCase(
    string Id,
    string Description,
    Func<BenchResult> Measurement,
    BenchCaseRole Role,
    string? BaselineId)
{
    /// <summary>Descriptions aligned with the case path for hierarchy rendering.</summary>
    public string[] SegmentDescriptions { get; init; } = [];
}
