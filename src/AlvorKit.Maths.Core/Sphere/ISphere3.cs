namespace AlvorKit.Maths;

/// <summary>Applies to 3D sphere types, including <c>Sphere3</c> and <c>Sphere3d</c>.</summary>
/// <typeparam name="TSelf">The concrete sphere type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
public interface ISphere3<TSelf, TScalar, TVector3, TBox3> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, ISphere3<TSelf, TScalar, TVector3, TBox3>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
{
    /// <summary>Gets the number of scalar components in the sphere.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the sphere.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets an empty sphere represented by a negative radius.</summary>
    static abstract TSelf Empty { get; }

    /// <summary>Gets or sets the sphere center point.</summary>
    TVector3 Center { get; set; }

    /// <summary>Gets or sets the sphere radius. A negative radius represents an empty sphere.</summary>
    TScalar Radius { get; set; }

    /// <summary>Gets the sphere diameter.</summary>
    TScalar Diameter { get; }

    /// <summary>Gets the squared radius.</summary>
    TScalar RadiusSquared { get; }

    /// <summary>Gets whether the radius is negative.</summary>
    bool IsEmpty { get; }

    /// <summary>Creates a sphere from a center and radius. A negative radius represents an empty sphere.</summary>
    static abstract TSelf Create(TVector3 center, TScalar radius);

    /// <summary>Creates a sphere from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Creates the smallest sphere centered on <paramref name="box" /> that contains its corners.</summary>
    static abstract TSelf CreateFromBox(TBox3 box);

    /// <summary>Gets a mutable reference to a scalar component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a scalar component by zero-based index, with center components first.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies the center components followed by radius into a caller-owned span.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy the center components followed by radius into a caller-owned span.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Returns whether this sphere contains <paramref name="point" />.</summary>
    bool Contains(TVector3 point);

    /// <summary>Returns whether this sphere fully contains <paramref name="sphere" />.</summary>
    bool Contains(TSelf sphere);

    /// <summary>Returns whether this sphere intersects <paramref name="sphere" />.</summary>
    bool Intersects(TSelf sphere);

    /// <summary>Returns the closest point in this sphere to <paramref name="point" />.</summary>
    TVector3 ClosestPoint(TVector3 point);

    /// <summary>Returns the distance from this sphere to <paramref name="point" />.</summary>
    TScalar DistanceTo(TVector3 point);

    /// <summary>Returns the squared distance from this sphere to <paramref name="point" />.</summary>
    TScalar DistanceSquaredTo(TVector3 point);

    /// <summary>Returns whether two spheres intersect, counting touching surfaces as an intersection.</summary>
    static abstract bool Intersects(TSelf left, TSelf right);
}
