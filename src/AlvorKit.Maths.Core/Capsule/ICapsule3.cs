namespace AlvorKit.Maths;

/// <summary>Applies to 3D capsule types, including <c>Capsule3</c> and <c>Capsule3d</c>.</summary>
/// <typeparam name="TSelf">The concrete capsule type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TSegment3">The matching 3D segment type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
/// <typeparam name="TRay3">The matching 3D ray type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
/// <typeparam name="TSphere3">The matching 3D sphere type.</typeparam>
/// <typeparam name="TFrustum3">The matching 3D frustum type.</typeparam>
/// <typeparam name="TInterval">The matching scalar interval type.</typeparam>
public interface ICapsule3<TSelf, TScalar, TVector3, TVector4, TSegment3, TPlane3, TRay3, TBox3, TSphere3, TFrustum3, TInterval> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, ICapsule3<TSelf, TScalar, TVector3, TVector4, TSegment3, TPlane3, TRay3, TBox3, TSphere3, TFrustum3, TInterval>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TSegment3 : struct, ISegment3<TSegment3, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3>
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
    where TRay3 : struct, IRay3<TRay3, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3, TFrustum3, TInterval>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
    where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
    where TFrustum3 : struct, IFrustum3<TFrustum3, TScalar, TVector3, TVector4, TPlane3, TBox3>
    where TInterval : struct, IInterval<TInterval, TScalar>
{
    /// <summary>Gets the number of scalar components in the capsule.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the capsule.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets an empty capsule represented by a negative radius.</summary>
    static abstract TSelf Empty { get; }

    /// <summary>Gets or sets the finite centerline segment swept by the capsule radius.</summary>
    TSegment3 Segment { get; set; }

    /// <summary>Gets or sets the capsule radius. A negative value represents an empty capsule.</summary>
    TScalar Radius { get; set; }

    /// <summary>Gets or sets the capsule start point.</summary>
    TVector3 Start { get; set; }

    /// <summary>Gets or sets the capsule end point.</summary>
    TVector3 End { get; set; }

    /// <summary>Gets the point halfway between <see cref="Start" /> and <see cref="End" />.</summary>
    TVector3 Center { get; }

    /// <summary>Gets the vector from <see cref="Start" /> to <see cref="End" />.</summary>
    TVector3 Direction { get; }

    /// <summary>Gets the distance between <see cref="Start" /> and <see cref="End" />.</summary>
    TScalar Length { get; }

    /// <summary>Gets the squared distance between <see cref="Start" /> and <see cref="End" />.</summary>
    TScalar LengthSquared { get; }

    /// <summary>Gets the squared capsule radius.</summary>
    TScalar RadiusSquared { get; }

    /// <summary>Gets whether the radius is negative.</summary>
    bool IsEmpty { get; }

    /// <summary>Creates a capsule from a finite centerline segment and radius.</summary>
    static abstract TSelf Create(TSegment3 segment, TScalar radius);

    /// <summary>Creates a capsule from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Gets a mutable reference to a scalar component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a scalar component by zero-based index, with segment components followed by radius.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies the segment components followed by radius into a caller-owned span.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy the segment components followed by radius into a caller-owned span.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Returns the point at <paramref name="amount" /> along the segment without clamping the amount.</summary>
    TVector3 PointAt(TScalar amount);

    /// <summary>Returns whether this capsule contains <paramref name="point" />.</summary>
    bool Contains(TVector3 point);

    /// <summary>Returns whether this capsule fully contains <paramref name="sphere" />.</summary>
    bool Contains(TSphere3 sphere);

    /// <summary>Returns whether this capsule intersects <paramref name="box" />.</summary>
    bool Intersects(TBox3 box);

    /// <summary>Returns whether this capsule intersects <paramref name="sphere" />.</summary>
    bool Intersects(TSphere3 sphere);

    /// <summary>Returns whether this capsule intersects <paramref name="capsule" />.</summary>
    bool Intersects(TSelf capsule);

    /// <summary>Returns whether this capsule intersects <paramref name="plane" />.</summary>
    bool Intersects(TPlane3 plane);

    /// <summary>Returns whether this capsule intersects <paramref name="frustum" />.</summary>
    bool Intersects(TFrustum3 frustum);

    /// <summary>Returns whether <paramref name="ray" /> intersects this capsule at a nonnegative distance.</summary>
    bool Intersects(TRay3 ray);

    /// <summary>Classifies how this capsule relates to <paramref name="frustum" />.</summary>
    ContainmentKind Classify(TFrustum3 frustum);

    /// <summary>Attempts to find the nearest nonnegative intersection distance from <paramref name="ray" /> to this capsule.</summary>
    bool TryIntersect(TRay3 ray, out TScalar distance);

    /// <summary>Returns the closest point in this capsule to <paramref name="point" />.</summary>
    TVector3 ClosestPoint(TVector3 point);

    /// <summary>Returns the distance from this capsule to <paramref name="point" />.</summary>
    TScalar DistanceTo(TVector3 point);

    /// <summary>Returns the squared distance from this capsule to <paramref name="point" />.</summary>
    TScalar DistanceSquaredTo(TVector3 point);

    /// <summary>Returns whether two capsules intersect, counting touching surfaces as an intersection.</summary>
    static abstract bool Intersects(TSelf left, TSelf right);
}
