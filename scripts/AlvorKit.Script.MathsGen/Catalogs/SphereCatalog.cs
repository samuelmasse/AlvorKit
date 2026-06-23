namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D sphere scalar types emitted by the generator.</summary>
internal static class SphereCatalog
{
    /// <summary>All sphere scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All sphere specifications emitted by the generator.</summary>
    public static IReadOnlyList<SphereSpec> Spheres { get; } =
        Scalars.Select(scalar => new SphereSpec(scalar)).ToArray();
}
