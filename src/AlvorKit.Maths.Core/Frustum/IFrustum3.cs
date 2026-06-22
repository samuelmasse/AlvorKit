namespace AlvorKit.Maths;

/// <summary>Applies to 3D frustum types, including <c>Frustum3</c> and <c>Frustum3d</c>.</summary>
/// <typeparam name="TSelf">The concrete frustum type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TVector4">The matching four-component vector type.</typeparam>
/// <typeparam name="TPlane3">The matching 3D plane type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
public interface IFrustum3<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, IFrustum3<TSelf, TScalar, TVector3, TVector4, TPlane3, TBox3>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TVector4 : struct, IVec4<TVector4, TScalar>
    where TPlane3 : struct, IPlane3<TPlane3, TScalar, TVector3, TVector4>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
{
    /// <summary>Gets the number of planes bounding the frustum.</summary>
    static abstract int PlaneCount { get; }

    /// <summary>Gets the number of finite corners in a closed frustum.</summary>
    static abstract int CornerCount { get; }

    /// <summary>Gets the number of scalar components stored by the six planes.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the left clipping plane.</summary>
    TPlane3 Left { get; set; }

    /// <summary>Gets the right clipping plane.</summary>
    TPlane3 Right { get; set; }

    /// <summary>Gets the bottom clipping plane.</summary>
    TPlane3 Bottom { get; set; }

    /// <summary>Gets the top clipping plane.</summary>
    TPlane3 Top { get; set; }

    /// <summary>Gets the near clipping plane.</summary>
    TPlane3 Near { get; set; }

    /// <summary>Gets the far clipping plane.</summary>
    TPlane3 Far { get; set; }

    /// <summary>Creates a frustum from six inward-facing planes in Left, Right, Bottom, Top, Near, Far order.</summary>
    static abstract TSelf Create(TPlane3 left, TPlane3 right, TPlane3 bottom, TPlane3 top, TPlane3 near, TPlane3 far);

    /// <summary>Creates a frustum from the first six inward-facing planes in Left, Right, Bottom, Top, Near, Far order.</summary>
    static abstract TSelf CreateFromPlanes(ReadOnlySpan<TPlane3> planes);

    /// <summary>Attempts to create a frustum from the first six inward-facing planes in Left, Right, Bottom, Top, Near, Far order.</summary>
    static abstract bool TryCreateFromPlanes(ReadOnlySpan<TPlane3> planes, out TSelf result);

    /// <summary>Creates a frustum from the first <see cref="ComponentCount" /> scalar values in Left, Right, Bottom, Top, Near, Far plane order.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Gets a mutable reference to a scalar component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a scalar component by zero-based index.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies scalar components into a caller-owned span in Left, Right, Bottom, Top, Near, Far plane order.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy scalar components into a caller-owned span in Left, Right, Bottom, Top, Near, Far plane order.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Copies the six planes into a caller-owned span in Left, Right, Bottom, Top, Near, Far order.</summary>
    void CopyPlanesTo(Span<TPlane3> destination);

    /// <summary>Attempts to copy the six planes into a caller-owned span in Left, Right, Bottom, Top, Near, Far order.</summary>
    bool TryCopyPlanesTo(Span<TPlane3> destination);

    /// <summary>
    /// Copies finite corners into a caller-owned span in Near bottom-left, Near bottom-right, Near top-left, Near top-right,
    /// Far bottom-left, Far bottom-right, Far top-left, Far top-right order.
    /// </summary>
    void CopyCornersTo(Span<TVector3> destination);

    /// <summary>
    /// Attempts to copy finite corners into a caller-owned span in Near bottom-left, Near bottom-right, Near top-left,
    /// Near top-right, Far bottom-left, Far bottom-right, Far top-left, Far top-right order.
    /// </summary>
    bool TryCopyCornersTo(Span<TVector3> destination);

    /// <summary>Gets whether the frustum planes define eight finite corners.</summary>
    bool HasFiniteCorners { get; }

    /// <summary>Attempts to copy normalized planes into a caller-owned span in Left, Right, Bottom, Top, Near, Far order.</summary>
    bool TryCopyNormalizedPlanesTo(Span<TPlane3> destination);

    /// <summary>Attempts to create an axis-aligned bounding box containing all finite frustum corners.</summary>
    bool TryCreateBoundingBox(out TBox3 box);

    /// <summary>Returns whether the frustum contains <paramref name="point" />.</summary>
    bool Contains(TVector3 point);

    /// <summary>Returns whether the frustum fully contains <paramref name="box" />.</summary>
    bool Contains(TBox3 box);

    /// <summary>Returns whether the frustum may intersect <paramref name="box" /> using a conservative culling test.</summary>
    bool Intersects(TBox3 box);

    /// <summary>
    /// Classifies how the frustum relates to <paramref name="box" />; <see cref="ContainmentKind.Intersects" /> is conservative
    /// and can be returned for boxes outside the finite frustum.
    /// </summary>
    ContainmentKind Classify(TBox3 box);

    /// <summary>Returns whether the frustum intersects <paramref name="box" /> after finite-corner refinement.</summary>
    bool IntersectsPrecise(TBox3 box);

    /// <summary>Classifies how the frustum relates to <paramref name="box" /> after finite-corner refinement.</summary>
    ContainmentKind ClassifyPrecise(TBox3 box);

    /// <summary>Returns whether the frustum fully contains <paramref name="other" />.</summary>
    bool Contains(TSelf other);

    /// <summary>Returns whether the frustum may intersect <paramref name="other" /> using a conservative culling test.</summary>
    bool Intersects(TSelf other);

    /// <summary>Classifies how the frustum relates to <paramref name="other" /> using finite corners when available.</summary>
    ContainmentKind Classify(TSelf other);

    /// <summary>Attempts to classify how the frustum relates to <paramref name="other" />.</summary>
    bool TryClassify(TSelf other, out ContainmentKind result);
}
