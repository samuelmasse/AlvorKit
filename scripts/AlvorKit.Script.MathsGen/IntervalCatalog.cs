namespace AlvorKit.Script.MathsGen;

/// <summary>Defines scalar interval types emitted by the generator.</summary>
internal static class IntervalCatalog
{
    /// <summary>All interval scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All interval specifications emitted by the generator.</summary>
    public static IReadOnlyList<IntervalSpec> Intervals { get; } =
        Scalars.Select(scalar => new IntervalSpec(scalar)).ToArray();
}
