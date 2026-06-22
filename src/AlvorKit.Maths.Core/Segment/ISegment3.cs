namespace AlvorKit.Maths;

/// <summary>Applies to 3D line segment types, including <c>Segment3</c> and <c>Segment3d</c>.</summary>
/// <typeparam name="TSelf">The concrete segment type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
/// <typeparam name="TSphere3">The matching 3D sphere type.</typeparam>
public interface ISegment3<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, ISegment3<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3, TSphere3>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
    where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
{
    /// <summary>Gets the number of scalar components in the segment.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the segment.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets or sets the segment start point.</summary>
    TVector3 Start { get; set; }

    /// <summary>Gets or sets the segment end point.</summary>
    TVector3 End { get; set; }

    /// <summary>Gets the point halfway between <see cref="Start" /> and <see cref="End" />.</summary>
    TVector3 Center { get; }

    /// <summary>Gets the vector from <see cref="Start" /> to <see cref="End" />.</summary>
    TVector3 Direction { get; }

    /// <summary>Gets the distance between <see cref="Start" /> and <see cref="End" />.</summary>
    TScalar Length { get; }

    /// <summary>Gets the squared distance between <see cref="Start" /> and <see cref="End" />.</summary>
    TScalar LengthSquared { get; }

    /// <summary>Creates a segment from a start and end point.</summary>
    static abstract TSelf Create(TVector3 start, TVector3 end);

    /// <summary>Creates a segment from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Gets a mutable reference to a scalar component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a scalar component by zero-based index, with start components first.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies start components followed by end components into a caller-owned span.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy start components followed by end components into a caller-owned span.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Returns the point at <paramref name="amount" /> along the segment without clamping the amount.</summary>
    TVector3 PointAt(TScalar amount);

    /// <summary>Returns the closest point on this finite segment to <paramref name="point" />.</summary>
    TVector3 ClosestPoint(TVector3 point);

    /// <summary>Returns the distance from this finite segment to <paramref name="point" />.</summary>
    TScalar DistanceTo(TVector3 point);

    /// <summary>Returns the squared distance from this finite segment to <paramref name="point" />.</summary>
    TScalar DistanceSquaredTo(TVector3 point);

    /// <summary>Returns whether this finite segment intersects <paramref name="sphere" />, counting touching as an intersection.</summary>
    bool Intersects(TSphere3 sphere);

    /// <summary>Returns whether this segment intersects <paramref name="box" />.</summary>
    bool Intersects(TBox3 box);

    /// <summary>Attempts to find this segment's intersection amount with <paramref name="plane" />.</summary>
    bool TryIntersect(TPlane3 plane, out TScalar amount);
}
