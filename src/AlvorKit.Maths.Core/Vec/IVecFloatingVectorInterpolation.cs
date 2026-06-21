namespace AlvorKit.Maths;

/// <summary>
/// Applies to floating-point vector types with component-wise interpolation amounts, including <c>Vec2</c>, <c>Vec3h</c>,
/// and <c>Vec4d</c>.
/// </summary>
/// <typeparam name="TSelf">The floating-point vector type, such as <c>Vec3</c> or <c>Vec4d</c>.</typeparam>
public interface IVecFloatingVectorInterpolation<TSelf>
    where TSelf : struct, IVecFloatingVectorInterpolation<TSelf>
{
    /// <summary>Linearly interpolates between two vectors with component-wise amounts.</summary>
    static abstract TSelf Lerp(TSelf from, TSelf to, TSelf amount);
}
