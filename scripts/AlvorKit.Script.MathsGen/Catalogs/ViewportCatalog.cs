namespace AlvorKit.Script.MathsGen;

/// <summary>Defines viewport scalar types emitted by the generator.</summary>
internal static class ViewportCatalog
{
    /// <summary>All viewport scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All viewport specifications emitted by the generator.</summary>
    public static IReadOnlyList<ViewportSpec> Viewports { get; } =
        Scalars.Select(scalar => new ViewportSpec(scalar)).ToArray();
}
