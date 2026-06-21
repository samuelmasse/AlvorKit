namespace AlvorKit.Script.MathsGen;

/// <summary>Emits compact 2D affine transform helpers for 3x2 floating-point matrices.</summary>
internal static class MatrixTransform3x2Emitter
{
    /// <summary>Appends compact 2D affine transform helpers for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        if (matrix.Scalar.Kind is not (ScalarKind.Float or ScalarKind.Double) || matrix.Columns != 3 || matrix.Rows != 2)
            return;

        members.Append(MathsTemplate.Fragment("matrix-transform3x2-2d.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("Vector2Type", matrix.Scalar.VectorName(2)),
            ("Vector3Type", matrix.Scalar.VectorName(3)),
            ("ZeroLiteral", matrix.Scalar.ZeroLiteral),
            ("OneLiteral", matrix.Scalar.OneLiteral)));
    }
}
