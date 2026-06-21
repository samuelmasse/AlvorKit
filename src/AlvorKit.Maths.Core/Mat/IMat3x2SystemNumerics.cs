namespace AlvorKit.Maths;

/// <summary>Applies to the single-precision 3x2 matrix type with <see cref="System.Numerics.Matrix3x2" /> conversions.</summary>
/// <typeparam name="TSelf">The single-precision 3x2 matrix type, such as <c>Mat3x2</c>.</typeparam>
public interface IMat3x2SystemNumerics<TSelf>
    where TSelf : struct, IMat3x2SystemNumerics<TSelf>
{
    /// <summary>Creates a matrix from a System.Numerics matrix.</summary>
    static abstract explicit operator TSelf(System.Numerics.Matrix3x2 value);

    /// <summary>Returns this matrix as a System.Numerics matrix.</summary>
    static abstract explicit operator System.Numerics.Matrix3x2(TSelf value);
}
