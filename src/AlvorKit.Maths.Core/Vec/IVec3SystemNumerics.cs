namespace AlvorKit.Maths;

/// <summary>Applies to the three-component single-precision vector type with <see cref="System.Numerics.Vector3" /> conversions, including <c>Vec3</c>.</summary>
/// <typeparam name="TSelf">The three-component single-precision vector type, such as <c>Vec3</c>.</typeparam>
public interface IVec3SystemNumerics<TSelf>
    where TSelf : struct, IVec3SystemNumerics<TSelf>
{
    /// <summary>Creates a vector from a System.Numerics vector.</summary>
    static abstract implicit operator TSelf(System.Numerics.Vector3 value);

    /// <summary>Returns this vector as a System.Numerics vector.</summary>
    static abstract implicit operator System.Numerics.Vector3(TSelf value);
}
