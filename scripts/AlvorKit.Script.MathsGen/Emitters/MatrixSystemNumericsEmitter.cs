namespace AlvorKit.Script.MathsGen;

/// <summary>Emits System.Numerics interop for matching matrix shapes.</summary>
internal static class MatrixSystemNumericsEmitter
{
    /// <summary>Appends conversion operators for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        if (matrix.Scalar.Kind != ScalarKind.Float)
            return;

        if (matrix.Columns == 3 && matrix.Rows == 2)
            members.Append(MathsTemplate.Fragment("matrix-system-numerics3x2.csfrag.tmpl"));
        else if (matrix.Columns == 4 && matrix.Rows == 4)
            members.Append(MathsTemplate.Fragment("matrix-system-numerics4.csfrag.tmpl"));
    }
}
