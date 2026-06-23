namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D segment scalar types emitted by the generator.</summary>
internal static class SegmentCatalog
{
    /// <summary>All segment scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All segment specifications emitted by the generator.</summary>
    public static IReadOnlyList<SegmentSpec> Segments { get; } =
        Scalars.Select(scalar => new SegmentSpec(scalar)).ToArray();
}
