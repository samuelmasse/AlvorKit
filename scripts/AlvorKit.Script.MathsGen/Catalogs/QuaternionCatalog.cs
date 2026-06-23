namespace AlvorKit.Script.MathsGen;

/// <summary>Defines quaternion scalar types emitted by the generator.</summary>
internal static class QuaternionCatalog
{
    /// <summary>All quaternion scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All quaternion specifications emitted by the generator.</summary>
    public static IReadOnlyList<QuaternionSpec> Quaternions { get; } =
        Scalars.Select(scalar => new QuaternionSpec(scalar)).ToArray();
}
