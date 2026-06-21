namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D frustum scalar types emitted by the generator.</summary>
internal static class FrustumCatalog
{
    /// <summary>All frustum scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All frustum specifications emitted by the generator.</summary>
    public static IReadOnlyList<FrustumSpec> Frustums { get; } =
        Scalars.Select(scalar => new FrustumSpec(scalar)).ToArray();
}
