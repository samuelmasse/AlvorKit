namespace AlvorKit.Maths;

/// <summary>
/// Applies to all numeric vector types with length and distance helpers,
/// including <c>Vec2</c>, <c>Vec3i</c>, and <c>Vec4u64</c>.
/// </summary>
/// <typeparam name="TSelf">The numeric vector type, such as <c>Vec3</c> or <c>Vec4i</c>.</typeparam>
/// <typeparam name="TScalar">The component type and squared-distance type, such as <see cref="float" /> or <see cref="int" />.</typeparam>
/// <typeparam name="TLength">The length and distance type, such as <see cref="float" /> for <c>Vec3i</c>.</typeparam>
public interface IVecMetric<TSelf, TScalar, TLength>
    where TSelf : struct, IVecMetric<TSelf, TScalar, TLength>
{
    /// <summary>Gets the squared Euclidean length.</summary>
    TScalar LengthSquared { get; }

    /// <summary>Gets the Euclidean length.</summary>
    TLength Length { get; }

    /// <summary>Returns the dot product of two vectors.</summary>
    static abstract TScalar Dot(TSelf left, TSelf right);

    /// <summary>Returns the squared distance between two points.</summary>
    static abstract TScalar DistanceSquared(TSelf left, TSelf right);

    /// <summary>Returns the distance between two points.</summary>
    static abstract TLength Distance(TSelf left, TSelf right);
}
