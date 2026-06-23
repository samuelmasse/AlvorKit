namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D oriented bounding box scalar types emitted by the generator.</summary>
internal static class ObbCatalog
{
    /// <summary>All oriented bounding box scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All oriented bounding box specifications emitted by the generator.</summary>
    public static IReadOnlyList<ObbSpec> Obbs { get; } =
        Scalars.Select(scalar => new ObbSpec(scalar)).ToArray();
}
