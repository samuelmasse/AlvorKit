namespace AlvorKit.Maths;

/// <summary>Applies to all 3D plane types, including <c>Plane3</c> and <c>Plane3d</c>.</summary>
/// <typeparam name="TSelf">The concrete plane type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component coefficient vector type.</typeparam>
public interface IPlane3<TSelf, TScalar, TVector3, TVector4> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    IUnaryNegationOperators<TSelf, TSelf>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, IPlane3<TSelf, TScalar, TVector3, TVector4>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
{
    /// <summary>Gets the number of scalar components in the plane equation.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the plane.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets the degenerate zero plane.</summary>
    static abstract TSelf Zero { get; }

    /// <summary>Gets or sets the normal vector.</summary>
    TVector3 Normal { get; set; }

    /// <summary>Gets or sets the offset from the origin in <c>dot(Normal, point) + Offset</c> form.</summary>
    TScalar Offset { get; set; }

    /// <summary>Gets or sets the plane equation coefficients as <c>(Normal.X, Normal.Y, Normal.Z, Offset)</c>.</summary>
    TVector4 Coefficients { get; set; }

    /// <summary>Gets the squared normal length.</summary>
    TScalar NormalLengthSquared { get; }

    /// <summary>Gets the normal length.</summary>
    TScalar NormalLength { get; }

    /// <summary>Gets this plane divided by its normal length.</summary>
    TSelf Normalized { get; }

    /// <summary>Returns this plane divided by its normal length, or fallback when its normal is zero.</summary>
    TSelf NormalizedOr(TSelf fallback);

    /// <summary>Returns this plane divided by its normal length when possible.</summary>
    bool TryNormalize(out TSelf result);

    /// <summary>Gets the plane with the opposite normal and offset.</summary>
    TSelf Flipped { get; }

    /// <summary>Creates a plane from a normal and offset.</summary>
    static abstract TSelf Create(TVector3 normal, TScalar offset);

    /// <summary>Creates a plane from equation coefficients.</summary>
    static abstract TSelf Create(TVector4 coefficients);

    /// <summary>Creates a plane from the first four values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Creates a plane that contains <paramref name="point" /> with the supplied normal.</summary>
    static abstract TSelf CreateFromPointNormal(TVector3 point, TVector3 normal);

    /// <summary>Creates a normalized plane that contains three non-collinear points.</summary>
    static abstract TSelf CreateFromPoints(TVector3 point0, TVector3 point1, TVector3 point2);

    /// <summary>Attempts to create a normalized plane that contains three non-collinear points.</summary>
    static abstract bool TryCreateFromPoints(TVector3 point0, TVector3 point1, TVector3 point2, out TSelf result);

    /// <summary>Gets a mutable reference to a component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a component by zero-based index.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies this plane into an array.</summary>
    void CopyTo(TScalar[] array);

    /// <summary>Copies this plane into an array starting at <paramref name="index" />.</summary>
    void CopyTo(TScalar[] array, int index);

    /// <summary>Copies this plane into a destination span.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy this plane into a destination span.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Evaluates <c>dot(Normal, point) + Offset</c>.</summary>
    TScalar Evaluate(TVector3 point);

    /// <summary>Returns the dot product of the plane coefficients and a four-component vector.</summary>
    TScalar Dot(TVector4 value);

    /// <summary>Returns the dot product of the plane normal and a vector.</summary>
    TScalar DotNormal(TVector3 value);

    /// <summary>Returns the signed distance from the plane to a point.</summary>
    TScalar SignedDistanceTo(TVector3 point);

    /// <summary>Returns the absolute distance from the plane to a point.</summary>
    TScalar DistanceTo(TVector3 point);

    /// <summary>Returns the closest point on this plane to <paramref name="point" />.</summary>
    TVector3 ClosestPoint(TVector3 point);

    /// <summary>Returns the projection of <paramref name="point" /> onto this plane.</summary>
    TVector3 ProjectPoint(TVector3 point);

    /// <summary>Returns <paramref name="point" /> reflected across this plane.</summary>
    TVector3 ReflectPoint(TVector3 point);

    /// <summary>Returns value divided by its normal length.</summary>
    static abstract TSelf Normalize(TSelf value);

    /// <summary>Returns value divided by its normal length when possible.</summary>
    static abstract bool TryNormalize(TSelf value, out TSelf result);

    /// <summary>Returns value with the opposite normal and offset.</summary>
    static abstract TSelf Flip(TSelf value);

    /// <summary>Evaluates <c>dot(plane.Normal, point) + plane.Offset</c>.</summary>
    static abstract TScalar Evaluate(TSelf plane, TVector3 point);

    /// <summary>Returns the dot product of the plane coefficients and a four-component vector.</summary>
    static abstract TScalar Dot(TSelf plane, TVector4 value);

    /// <summary>Returns the dot product of the plane normal and a vector.</summary>
    static abstract TScalar DotNormal(TSelf plane, TVector3 value);

    /// <summary>Returns whether value has a unit-length normal.</summary>
    static abstract bool IsNormalized(TSelf value, TScalar epsilon);

    /// <summary>Returns whether this plane has a unit-length normal.</summary>
    bool IsNormalized(TScalar epsilon);
}
