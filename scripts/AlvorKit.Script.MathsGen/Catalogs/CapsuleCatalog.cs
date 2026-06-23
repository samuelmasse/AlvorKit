namespace AlvorKit.Script.MathsGen;

/// <summary>Defines 3D capsule scalar types emitted by the generator.</summary>
internal static class CapsuleCatalog
{
    /// <summary>All capsule scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        VectorCatalog.Float,
        VectorCatalog.Double,
    ];

    /// <summary>All capsule specifications emitted by the generator.</summary>
    public static IReadOnlyList<CapsuleSpec> Capsules { get; } =
        Scalars.Select(scalar => new CapsuleSpec(scalar)).ToArray();
}
