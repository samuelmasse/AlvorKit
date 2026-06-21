namespace AlvorKit.Script.MathsGen;

/// <summary>Emits quaternion rotation helpers for 3D transform matrices.</summary>
internal static class MatrixQuaternionEmitter
{
    /// <summary>Appends quaternion rotation helpers for <paramref name="matrix"/>.</summary>
    public static void Emit(MatrixSpec matrix, MemberBlock members)
    {
        if (matrix.Scalar.Kind is not (ScalarKind.Float or ScalarKind.Double))
            return;

        if (matrix.Columns == 3 && matrix.Rows == 3)
        {
            members.Append(MathsTemplate.Fragment("matrix-quaternion3.csfrag.tmpl",
                ("TypeName", matrix.TypeName),
                ("QuaternionType", matrix.Scalar.QuaternionName())));
            return;
        }

        if (matrix.Columns == 4 && matrix.Rows == 4)
        {
            members.Append(MathsTemplate.Fragment("matrix-quaternion4.csfrag.tmpl",
                ("TypeName", matrix.TypeName),
                ("Vector3Type", matrix.Scalar.VectorName(3)),
                ("QuaternionType", matrix.Scalar.QuaternionName())));
        }
    }
}
