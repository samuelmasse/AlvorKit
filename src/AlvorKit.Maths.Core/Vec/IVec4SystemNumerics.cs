namespace AlvorKit.Maths;

/// <summary>Applies to the four-component single-precision vector type with <see cref="System.Numerics.Vector4" /> conversions, including <c>Vec4</c>.</summary>
/// <typeparam name="TSelf">The four-component single-precision vector type, such as <c>Vec4</c>.</typeparam>
public interface IVec4SystemNumerics<TSelf>
    where TSelf : struct, IVec4SystemNumerics<TSelf>
{
    /// <summary>Creates a vector from a System.Numerics vector.</summary>
    static abstract implicit operator TSelf(System.Numerics.Vector4 value);

    /// <summary>Returns this vector as a System.Numerics vector.</summary>
    static abstract implicit operator System.Numerics.Vector4(TSelf value);
}
