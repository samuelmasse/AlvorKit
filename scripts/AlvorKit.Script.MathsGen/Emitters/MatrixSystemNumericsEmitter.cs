namespace AlvorKit.Script.MathsGen;

/// <summary>Emits System.Numerics interop for matching matrix shapes.</summary>
internal static class MatrixSystemNumericsEmitter
{
    /// <summary>Returns whether a matrix has an equal-size System.Numerics storage view.</summary>
    public static bool SupportsPackedStorage(MatrixSpec matrix) =>
        matrix.Scalar.Kind == ScalarKind.Float &&
        ((matrix.Columns == 3 && matrix.Rows == 2) || (matrix.Columns == 4 && matrix.Rows == 4));

    /// <summary>Returns the equal-size System.Numerics matrix type.</summary>
    public static string PackedType(MatrixSpec matrix) =>
        matrix.Columns == 3 ? "System.Numerics.Matrix3x2" : "System.Numerics.Matrix4x4";

    /// <summary>Appends conversion operators for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        if (!SupportsPackedStorage(matrix))
            return;

        if (matrix.Columns == 3 && matrix.Rows == 2)
            members.Append(MathsTemplate.Fragment("matrix-system-numerics3x2.csfrag.tmpl"));
        else if (matrix.Columns == 4 && matrix.Rows == 4)
            members.Append(MathsTemplate.Fragment("matrix-system-numerics4.csfrag.tmpl"));
    }
}
