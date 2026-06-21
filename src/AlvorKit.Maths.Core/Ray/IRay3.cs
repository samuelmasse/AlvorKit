namespace AlvorKit.Maths;

/// <summary>Applies to 3D ray types, including <c>Ray3</c> and <c>Ray3d</c>.</summary>
/// <typeparam name="TSelf">The concrete ray type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
/// <typeparam name="TSphere3">The matching 3D sphere type.</typeparam>
/// <typeparam name="TFrustum3">The matching 3D frustum type.</typeparam>
/// <typeparam name="TInterval">The matching scalar interval type.</typeparam>
public interface IRay3<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, IRay3<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
    where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
    where TFrustum3 : struct, IFrustum3<TFrustum3, TScalar, TVector3, TVector4, TPlane3, TBox3>
    where TInterval : struct, IInterval<TInterval, TScalar>
{
    /// <summary>Gets the number of scalar components in the ray.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the ray.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets or sets the ray origin point.</summary>
    TVector3 Origin { get; set; }

    /// <summary>Gets or sets the ray direction. The direction is not normalized automatically.</summary>
    TVector3 Direction { get; set; }

    /// <summary>Creates a ray from an origin and direction.</summary>
    static abstract TSelf Create(TVector3 origin, TVector3 direction);

    /// <summary>Creates a ray from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Gets a mutable reference to a scalar component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a scalar component by zero-based index, with origin components first.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies the origin components followed by direction components into a caller-owned span.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy the origin components followed by direction components into a caller-owned span.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Returns the point at <paramref name="distance" /> along the stored direction.</summary>
    TVector3 PointAt(TScalar distance);

    /// <summary>Returns this ray translated by <paramref name="offset" />.</summary>
    TSelf Translated(TVector3 offset);

    /// <summary>Returns this ray with a unit-length direction.</summary>
    TSelf Normalized();

    /// <summary>Returns this ray with a unit-length direction when the direction is nonzero.</summary>
    bool TryNormalize(out TSelf result);

    /// <summary>Returns whether this ray intersects <paramref name="plane" /> at a nonnegative distance.</summary>
    bool Intersects(TPlane3 plane);

    /// <summary>Returns whether this ray intersects <paramref name="box" /> at a nonnegative distance.</summary>
    bool Intersects(TBox3 box);

    /// <summary>Returns whether this ray intersects <paramref name="sphere" /> at a nonnegative distance.</summary>
    bool Intersects(TSphere3 sphere);

    /// <summary>Returns whether this ray intersects <paramref name="frustum" /> at a nonnegative distance.</summary>
    bool Intersects(TFrustum3 frustum);

    /// <summary>Attempts to find this ray's nonnegative intersection distance with <paramref name="plane" />.</summary>
    bool TryIntersect(TPlane3 plane, out TScalar distance);

    /// <summary>Attempts to find this ray's nearest nonnegative intersection distance with <paramref name="box" />.</summary>
    bool TryIntersect(TBox3 box, out TScalar distance);

    /// <summary>Attempts to find this ray's nonnegative intersection distance interval with <paramref name="box" />.</summary>
    bool TryIntersect(TBox3 box, out TInterval distances);

    /// <summary>Attempts to find this ray's nearest nonnegative intersection distance with <paramref name="sphere" />.</summary>
    bool TryIntersect(TSphere3 sphere, out TScalar distance);

    /// <summary>Attempts to find this ray's nonnegative intersection distance interval with <paramref name="frustum" />.</summary>
    bool TryIntersect(TFrustum3 frustum, out TInterval distances);
}
