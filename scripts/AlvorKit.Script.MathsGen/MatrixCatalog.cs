namespace AlvorKit.Script.MathsGen;

/// <summary>Defines matrix shapes and scalar families emitted by the generator.</summary>
internal static class MatrixCatalog
{
    /// <summary>All matrix shapes emitted as column-count and row-count pairs.</summary>
    public static IReadOnlyList<(int Columns, int Rows)> Shapes { get; } =
    [
        (2, 2),
        (2, 3),
        (2, 4),
        (3, 2),
        (3, 3),
        (3, 4),
        (4, 2),
        (4, 3),
        (4, 4),
    ];

    /// <summary>The floating-point scalar types emitted for matrices.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
        [VectorCatalog.Float, VectorCatalog.Double];

    /// <summary>All matrix specifications emitted by the generator.</summary>
    public static IReadOnlyList<MatrixSpec> Matrices { get; } =
        Scalars.SelectMany(scalar => Shapes.Select(shape => new MatrixSpec(shape.Columns, shape.Rows, scalar))).ToArray();
}
