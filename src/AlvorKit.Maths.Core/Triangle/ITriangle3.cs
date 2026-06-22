namespace AlvorKit.Maths;

/// <summary>Applies to 3D triangle types, including <c>Triangle3</c> and <c>Triangle3d</c>.</summary>
/// <typeparam name="TSelf">The concrete triangle type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
/// <typeparam name="TRay3">The matching 3D ray type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
/// <typeparam name="TSphere3">The matching 3D sphere type.</typeparam>
/// <typeparam name="TFrustum3">The matching 3D frustum type.</typeparam>
/// <typeparam name="TInterval">The matching scalar interval type.</typeparam>
public interface ITriangle3<TSelf, TScalar, TVector3, TVector4, TPlane3, TRay3, TBox3, TSphere3, TFrustum3, TInterval> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, ITriangle3<TSelf, TScalar, TVector3, TVector4, TPlane3, TRay3, TBox3, TSphere3, TFrustum3, TInterval>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
    where TRay3 : struct, IRay3<TRay3, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
    where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
    where TFrustum3 : struct, IFrustum3<TFrustum3, TScalar, TVector3, TVector4, TPlane3, TBox3>
    where TInterval : struct, IInterval<TInterval, TScalar>
{
    /// <summary>Gets the number of scalar components in the triangle.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the triangle.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets or sets the first triangle vertex.</summary>
    TVector3 A { get; set; }

    /// <summary>Gets or sets the second triangle vertex.</summary>
    TVector3 B { get; set; }

    /// <summary>Gets or sets the third triangle vertex.</summary>
    TVector3 C { get; set; }

    /// <summary>Gets the edge from <see cref="A" /> to <see cref="B" />.</summary>
    TVector3 EdgeAB { get; }

    /// <summary>Gets the edge from <see cref="A" /> to <see cref="C" />.</summary>
    TVector3 EdgeAC { get; }

    /// <summary>Gets the edge from <see cref="B" /> to <see cref="C" />.</summary>
    TVector3 EdgeBC { get; }

    /// <summary>Gets the raw cross product of <see cref="EdgeAB" /> and <see cref="EdgeAC" />.</summary>
    TVector3 UnnormalizedNormal { get; }

    /// <summary>Gets the unit normal, or throws when the triangle is degenerate.</summary>
    TVector3 Normal { get; }

    /// <summary>Gets the normalized plane containing this triangle, or throws when the triangle is degenerate.</summary>
    TPlane3 Plane { get; }

    /// <summary>Gets the finite triangle area.</summary>
    TScalar Area { get; }

    /// <summary>Gets whether the triangle has zero area.</summary>
    bool IsDegenerate { get; }

    /// <summary>Creates a triangle from three vertices.</summary>
    static abstract TSelf Create(TVector3 a, TVector3 b, TVector3 c);

    /// <summary>Creates a triangle from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Gets a mutable reference to a scalar component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a scalar component by zero-based index, with vertex components in A, B, C order.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies vertex components into a caller-owned span in A, B, C order.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy vertex components into a caller-owned span in A, B, C order.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Returns the unit normal when the triangle has nonzero area.</summary>
    bool TryGetNormal(out TVector3 normal);

    /// <summary>Returns the normalized plane containing this triangle when the triangle has nonzero area.</summary>
    bool TryGetPlane(out TPlane3 plane);

    /// <summary>Returns barycentric coordinates for <paramref name="point" /> relative to this triangle.</summary>
    TVector3 Barycentric(TVector3 point);

    /// <summary>Returns whether this triangle contains <paramref name="point" />.</summary>
    bool Contains(TVector3 point);

    /// <summary>Returns whether this triangle intersects <paramref name="box" />.</summary>
    bool Intersects(TBox3 box);

    /// <summary>Returns whether this triangle intersects <paramref name="sphere" />.</summary>
    bool Intersects(TSphere3 sphere);

    /// <summary>Returns whether <paramref name="ray" /> intersects this triangle at a nonnegative distance.</summary>
    bool Intersects(TRay3 ray);

    /// <summary>Attempts to find the nearest nonnegative intersection distance from <paramref name="ray" /> to this triangle.</summary>
    bool TryIntersect(TRay3 ray, out TScalar distance);

    /// <summary>Returns the closest point on this finite triangle to <paramref name="point" />.</summary>
    TVector3 ClosestPoint(TVector3 point);

    /// <summary>Returns the distance from this finite triangle to <paramref name="point" />.</summary>
    TScalar DistanceTo(TVector3 point);

    /// <summary>Returns the squared distance from this finite triangle to <paramref name="point" />.</summary>
    TScalar DistanceSquaredTo(TVector3 point);
}
