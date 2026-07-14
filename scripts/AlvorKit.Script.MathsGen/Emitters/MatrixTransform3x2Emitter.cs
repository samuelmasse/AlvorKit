namespace AlvorKit.Script.MathsGen;

/// <summary>Emits compact 2D affine transform helpers for 3x2 floating-point matrices.</summary>
internal static class MatrixTransform3x2Emitter
{
    /// <summary>Appends compact 2D affine transform helpers for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        if (matrix.Scalar.Kind is not (ScalarKind.Float or ScalarKind.Double) || matrix.Columns != 3 || matrix.Rows != 2)
            return;

        var inverseTemplate = matrix.Scalar.Kind == ScalarKind.Float
            ? "matrix-transform3x2-invert-system.csfrag.tmpl"
            : "matrix-transform3x2-invert-scalar.csfrag.tmpl";
        var inverseBody = MathsTemplate.Fragment(inverseTemplate,
            ("TypeName", matrix.TypeName),
            ("Vector2Type", matrix.Scalar.VectorName(2)),
            ("ZeroLiteral", matrix.Scalar.ZeroLiteral),
            ("OneLiteral", matrix.Scalar.OneLiteral));
        members.Append(MathsTemplate.Fragment("matrix-transform3x2-2d.csfrag.tmpl",
            ("TypeName", matrix.TypeName),
            ("ScalarType", matrix.Scalar.CSharpName),
            ("Vector2Type", matrix.Scalar.VectorName(2)),
            ("Vector3Type", matrix.Scalar.VectorName(3)),
            ("ZeroLiteral", matrix.Scalar.ZeroLiteral),
            ("OneLiteral", matrix.Scalar.OneLiteral),
            ("TryInvertBody", inverseBody),
            ("TransformPointExpression", TransformPointExpression(matrix)),
            ("TransformVectorExpression", TransformVectorExpression(matrix))));
    }

    private static string TransformPointExpression(MatrixSpec matrix) =>
        matrix.Scalar.Kind == ScalarKind.Float
            ? "Unsafe.BitCast<System.Numerics.Vector2, Vec2>(System.Numerics.Vector2.Transform(" +
                "Unsafe.BitCast<Vec2, System.Numerics.Vector2>(point), " +
                "value.packed))"
            : "(value.Column0 * point.X) + (value.Column1 * point.Y) + value.Column2";

    private static string TransformVectorExpression(MatrixSpec matrix) =>
        matrix.Scalar.Kind == ScalarKind.Float
            ? "Unsafe.BitCast<System.Numerics.Vector2, Vec2>(System.Numerics.Vector2.TransformNormal(" +
                "Unsafe.BitCast<Vec2, System.Numerics.Vector2>(vector), " +
                "value.packed))"
            : "(value.Column0 * vector.X) + (value.Column1 * vector.Y)";
}
