namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D triangle scalar types emitted by the generator.</summary>
internal static class TriangleCatalog
{
    /// <summary>All triangle scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All triangle specifications emitted by the generator.</summary>
    public static IReadOnlyList<TriangleSpec> Triangles { get; } =
        Scalars.Select(scalar => new TriangleSpec(scalar)).ToArray();
}
