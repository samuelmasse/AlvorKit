namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D plane scalar types emitted by the generator.</summary>
internal static class PlaneCatalog
{
    /// <summary>All plane scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All plane specifications emitted by the generator.</summary>
    public static IReadOnlyList<PlaneSpec> Planes { get; } =
        Scalars.Select(scalar => new PlaneSpec(scalar)).ToArray();
}
