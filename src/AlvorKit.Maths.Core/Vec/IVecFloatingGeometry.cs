namespace AlvorKit.Maths;

/// <summary>
/// Applies to all floating-point vector types with geometric helpers, including <c>Vec2</c>, <c>Vec3h</c>, and
/// <c>Vec4d</c>.
/// </summary>
/// <typeparam name="TSelf">The floating-point vector type, such as <c>Vec3</c> or <c>Vec4d</c>.</typeparam>
/// <typeparam name="TScalar">The floating-point component type, such as <see cref="float" />, <see cref="double" />, or <see cref="Half" />.</typeparam>
public interface IVecFloatingGeometry<TSelf, TScalar> : IVecMetric<TSelf, TScalar, TScalar>
    where TSelf : struct, IVecFloatingGeometry<TSelf, TScalar>
{
    /// <summary>Gets this vector divided by its length.</summary>
    TSelf Normalized { get; }

    /// <summary>Gets this vector divided by its length, or zero when its length is zero.</summary>
    TSelf NormalizedOrZero { get; }

    /// <summary>Returns this vector divided by its length, or fallback when its length is zero.</summary>
    TSelf NormalizedOr(TSelf fallback);

    /// <summary>Returns this vector divided by its length when possible.</summary>
    bool TryNormalize(out TSelf result);

    /// <summary>Returns value divided by its length.</summary>
    static abstract TSelf Normalize(TSelf value);

    /// <summary>Linearly interpolates between two vectors without clamping amount.</summary>
    static abstract TSelf Lerp(TSelf from, TSelf to, TScalar amount);

    /// <summary>Returns the barycentric blend of three vectors.</summary>
    static abstract TSelf Barycentric(TSelf a, TSelf b, TSelf c, TScalar u, TScalar v);

    /// <summary>Returns incident reflected around normal.</summary>
    static abstract TSelf Reflect(TSelf incident, TSelf normal);

    /// <summary>Returns a vector facing away from incident according to referenceNormal.</summary>
    static abstract TSelf FaceForward(TSelf normal, TSelf incident, TSelf referenceNormal);

    /// <summary>Returns the refraction vector for an incident vector, normal, and index ratio.</summary>
    static abstract TSelf Refract(TSelf incident, TSelf normal, TScalar indexRatio);
}
