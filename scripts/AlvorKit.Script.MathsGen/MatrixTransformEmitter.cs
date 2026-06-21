namespace AlvorKit.Script.MathsGen;

/// <summary>Emits graphics-oriented transform helpers for 4x4 floating-point matrices.</summary>
internal static class MatrixTransformEmitter
{
    /// <summary>Appends transform helpers for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        if (matrix.Scalar.Kind is not (ScalarKind.Float or ScalarKind.Double) || matrix.Columns != 4 || matrix.Rows != 4)
            return;

        var values = Values(matrix);
        members.Append(MathsTemplate.Fragment("matrix-transform4-core.csfrag.tmpl", values));
        members.Append(MathsTemplate.Fragment("matrix-transform4-projection.csfrag.tmpl", values));
    }

    private static (string Name, string Value)[] Values(MatrixSpec matrix) =>
    [
        ("TypeName", matrix.TypeName),
        ("ScalarType", matrix.Scalar.CSharpName),
        ("Suffix", matrix.Scalar.Suffix),
        ("Vector2Type", matrix.Scalar.VectorName(2)),
        ("Vector3Type", matrix.Scalar.VectorName(3)),
        ("Vector4Type", matrix.Scalar.VectorName(4)),
        ("ZeroLiteral", matrix.Scalar.ZeroLiteral),
        ("OneLiteral", matrix.Scalar.OneLiteral),
        ("TwoLiteral", matrix.Scalar.TwoLiteral),
        ("EpsilonLiteral", $"{matrix.Scalar.CSharpName}.Epsilon"),
    ];
}
