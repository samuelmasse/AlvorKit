namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D ray scalar types emitted by the generator.</summary>
internal static class RayCatalog
{
    /// <summary>All ray scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All ray specifications emitted by the generator.</summary>
    public static IReadOnlyList<RaySpec> Rays { get; } =
        Scalars.Select(scalar => new RaySpec(scalar)).ToArray();
}
