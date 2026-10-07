namespace AlvorKit;

/// <summary>Describes a group, standalone measurement, or comparison in the suite hierarchy.</summary>
public record BenchNode(
    string Name,
    string Description,
    BenchNode[] Children,
    Func<BenchResult>? Measurement,
    BenchComparison? Comparison)
{
    /// <summary>Groups child nodes without adding a measurement.</summary>
    public static BenchNode Group(string name, string description, BenchNode[] children) =>
        new(name, description, children, null, null);

    /// <summary>Declares a baseline and candidates beneath one path.</summary>
    public static BenchNode Compare(string name, string description, BenchComparison comparison) =>
        new(name, description, [], null, comparison);

    /// <summary>Declares one independently runnable measurement.</summary>
    public static BenchNode Measure(string name, string description, Func<BenchResult> measurement) =>
        new(name, description, [], measurement, null);
}
