namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D quad scalar types emitted by the generator.</summary>
internal static class QuadCatalog
{
    /// <summary>All quad scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All quad specifications emitted by the generator.</summary>
    public static IReadOnlyList<QuadSpec> Quads { get; } =
        Scalars.Select(scalar => new QuadSpec(scalar)).ToArray();
}
