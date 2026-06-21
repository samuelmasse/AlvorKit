namespace AlvorKit.Maths;

/// <summary>Applies to the single-precision 4x4 matrix type with <see cref="System.Numerics.Matrix4x4" /> conversions.</summary>
/// <typeparam name="TSelf">The single-precision 4x4 matrix type, such as <c>Mat4</c>.</typeparam>
public interface IMat4SystemNumerics<TSelf>
    where TSelf : struct, IMat4SystemNumerics<TSelf>
{
    /// <summary>Creates a matrix from a System.Numerics matrix.</summary>
    static abstract explicit operator TSelf(System.Numerics.Matrix4x4 value);

    /// <summary>Returns this matrix as a System.Numerics matrix.</summary>
    static abstract explicit operator System.Numerics.Matrix4x4(TSelf value);
}
