namespace AlvorKit.Script.MathsGen;

/// <summary>Emits 2D transform helpers for 3x3 floating-point matrices.</summary>
internal static class MatrixTransform2DEmitter
{
    /// <summary>Appends 2D transform helpers for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        if (matrix.Scalar.Kind is not (ScalarKind.Float or ScalarKind.Double) || matrix.Columns != 3 || matrix.Rows != 3)
            return;

        members.Append(MathsTemplate.Fragment("matrix-transform3-2d.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("Vector2Type", matrix.Scalar.VectorName(2)),
            ("Vector3Type", matrix.Scalar.VectorName(3)),
            ("ZeroLiteral", matrix.Scalar.ZeroLiteral),
            ("OneLiteral", matrix.Scalar.OneLiteral)));
    }
}
