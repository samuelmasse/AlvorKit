namespace AlvorKit.Script.MathsGen;

/// <summary>Defines axis-aligned box dimensions and scalar families emitted by the generator.</summary>
internal static class BoxCatalog
{
    /// <summary>All box dimensions emitted by the generator.</summary>
    public static IReadOnlyList<int> Dimensions { get; } = [2, 3];

    /// <summary>The scalar types emitted for boxes.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
        VectorCatalog.Int,
    ];

    /// <summary>All box specifications emitted by the generator.</summary>
    public static IReadOnlyList<BoxSpec> Boxes { get; } =
        Scalars.SelectMany(scalar => Dimensions.Select(dimension => new BoxSpec(dimension, scalar))).ToArray();
}
