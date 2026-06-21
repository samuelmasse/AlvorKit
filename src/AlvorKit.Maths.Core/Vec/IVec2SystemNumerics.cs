namespace AlvorKit.Maths;

/// <summary>Applies to the two-component single-precision vector type with <see cref="System.Numerics.Vector2" /> conversions, including <c>Vec2</c>.</summary>
/// <typeparam name="TSelf">The two-component single-precision vector type, such as <c>Vec2</c>.</typeparam>
public interface IVec2SystemNumerics<TSelf>
    where TSelf : struct, IVec2SystemNumerics<TSelf>
{
    /// <summary>Creates a vector from a System.Numerics vector.</summary>
    static abstract implicit operator TSelf(System.Numerics.Vector2 value);

    /// <summary>Returns this vector as a System.Numerics vector.</summary>
    static abstract implicit operator System.Numerics.Vector2(TSelf value);
}
