namespace AlvorKit.Maths;

/// <summary>Applies to 3D oriented bounding box types, including <c>Obb3</c> and <c>Obb3d</c>.</summary>
/// <typeparam name="TSelf">The concrete oriented bounding box type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TQuat">The matching quaternion type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
/// <typeparam name="TSphere3">The matching 3D sphere type.</typeparam>
/// <typeparam name="TFrustum3">The matching 3D frustum type.</typeparam>
public interface IObb3<TSelf, TScalar, TVector3, TVector4, TQuat, TPlane3, TBox3, TSphere3, TFrustum3> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, IObb3<TSelf, TScalar, TVector3, TVector4, TQuat, TPlane3, TBox3, TSphere3, TFrustum3>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TQuat : struct
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
    where TSphere3 : struct, ISphere3<TSphere3, TScalar, TVector3, TBox3>
    where TFrustum3 : struct, IFrustum3<TFrustum3, TScalar, TVector3, TVector4, TPlane3, TBox3>
{
    /// <summary>Gets the number of finite corners in the oriented bounding box.</summary>
    static abstract int CornerCount { get; }

    /// <summary>Gets the number of scalar components in the oriented bounding box.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the oriented bounding box.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets an empty oriented bounding box represented by a negative half size.</summary>
    static abstract TSelf Empty { get; }

    /// <summary>Gets or sets the oriented bounding box center point.</summary>
    TVector3 Center { get; set; }

    /// <summary>Gets or sets the oriented bounding box half size. Any negative component represents an empty box.</summary>
    TVector3 HalfSize { get; set; }

    /// <summary>Gets or sets the oriented bounding box orientation. The orientation is not normalized automatically.</summary>
    TQuat Orientation { get; set; }

    /// <summary>Gets or sets the full oriented bounding box size.</summary>
    TVector3 Size { get; set; }

    /// <summary>Gets whether any half-size component is negative.</summary>
    bool IsEmpty { get; }

    /// <summary>Creates an oriented bounding box from a center, half size, and orientation.</summary>
    static abstract TSelf Create(TVector3 center, TVector3 halfSize, TQuat orientation);

    /// <summary>Creates an oriented bounding box from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Creates an identity-oriented bounding box from an axis-aligned box.</summary>
    static abstract TSelf CreateFromBox(TBox3 box);

    /// <summary>Gets a mutable reference to a scalar component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a scalar component by zero-based index, with center, half-size, then orientation components.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies the center, half-size, and orientation components into a caller-owned span.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy the center, half-size, and orientation components into a caller-owned span.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Copies finite corners into a caller-owned span.</summary>
    void CopyCornersTo(Span<TVector3> destination);

    /// <summary>Attempts to copy finite corners into a caller-owned span.</summary>
    bool TryCopyCornersTo(Span<TVector3> destination);

    /// <summary>Returns whether this oriented bounding box contains <paramref name="point" />.</summary>
    bool Contains(TVector3 point);

    /// <summary>Returns whether this oriented bounding box fully contains <paramref name="sphere" />.</summary>
    bool Contains(TSphere3 sphere);

    /// <summary>Returns whether this oriented bounding box fully contains <paramref name="other" />.</summary>
    bool Contains(TSelf other);

    /// <summary>Returns whether this oriented bounding box intersects <paramref name="box" />.</summary>
    bool Intersects(TBox3 box);

    /// <summary>Returns whether this oriented bounding box intersects <paramref name="sphere" />.</summary>
    bool Intersects(TSphere3 sphere);

    /// <summary>Returns whether this oriented bounding box intersects <paramref name="other" />.</summary>
    bool Intersects(TSelf other);

    /// <summary>Returns whether this oriented bounding box intersects <paramref name="plane" />.</summary>
    bool Intersects(TPlane3 plane);

    /// <summary>Returns whether this oriented bounding box intersects <paramref name="frustum" />.</summary>
    bool Intersects(TFrustum3 frustum);

    /// <summary>Returns the closest point in this oriented bounding box to <paramref name="point" />.</summary>
    TVector3 ClosestPoint(TVector3 point);

    /// <summary>Returns the distance from this oriented bounding box to <paramref name="point" />.</summary>
    TScalar DistanceTo(TVector3 point);

    /// <summary>Returns the squared distance from this oriented bounding box to <paramref name="point" />.</summary>
    TScalar DistanceSquaredTo(TVector3 point);

    /// <summary>Returns whether two oriented bounding boxes intersect, counting touching surfaces as an intersection.</summary>
    static abstract bool Intersects(TSelf left, TSelf right);
}
