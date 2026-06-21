namespace AlvorKit.Maths;

/// <summary>
/// Applies to all axis-aligned box types, such as <c>Box2</c>, <c>Box2i</c>, and <c>Box3d</c>.
/// </summary>
public interface IBox<TSelf, TScalar, TVector> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, IBox<TSelf, TScalar, TVector>
{
    /// <summary>Gets the number of spatial dimensions in the box.</summary>
    static abstract int Dimension { get; }

    /// <summary>Gets the number of scalar components used by the two corners.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets an empty box whose minimum corner is greater than its maximum corner.</summary>
    static abstract TSelf Empty { get; }

    /// <summary>Gets or sets the minimum corner.</summary>
    TVector Min { get; set; }

    /// <summary>Gets or sets the maximum corner.</summary>
    TVector Max { get; set; }

    /// <summary>Gets or sets the box size, preserving the minimum corner when assigned.</summary>
    TVector Size { get; set; }

    /// <summary>Gets or sets the box center, preserving the current half size when assigned.</summary>
    TVector Center { get; set; }

    /// <summary>Gets or sets the half size around the current center.</summary>
    TVector HalfSize { get; set; }

    /// <summary>Gets whether any minimum component is greater than the matching maximum component.</summary>
    bool IsEmpty { get; }

    /// <summary>Creates a box from minimum and maximum corners.</summary>
    static abstract TSelf Create(TVector min, TVector max);

    /// <summary>Creates a box from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Creates a box that contains both points regardless of their order.</summary>
    static abstract TSelf CreateFromCorners(TVector first, TVector second);

    /// <summary>Creates a box from a center and half size.</summary>
    static abstract TSelf CreateFromCenterHalfSize(TVector center, TVector halfSize);

    /// <summary>Creates a box from a center and full size.</summary>
    static abstract TSelf CreateFromCenterSize(TVector center, TVector size);

    /// <summary>Gets a mutable reference to a corner component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a corner component by zero-based index, with minimum components first.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies the corner components into a caller-owned span, with minimum components first.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy the corner components into a caller-owned span, with minimum components first.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Returns this box with minimum and maximum corners ordered component by component.</summary>
    TSelf Normalized { get; }

    /// <summary>Returns whether the box inclusively contains <paramref name="point" />.</summary>
    bool Contains(TVector point);

    /// <summary>Returns whether the box contains <paramref name="point" />, including minimum edges and excluding maximum edges.</summary>
    bool ContainsHalfOpen(TVector point);

    /// <summary>Returns whether the box inclusively contains <paramref name="point" />.</summary>
    bool ContainsInclusive(TVector point);

    /// <summary>Returns whether the box exclusively contains <paramref name="point" />.</summary>
    bool ContainsExclusive(TVector point);

    /// <summary>Returns whether the box inclusively contains <paramref name="other" />.</summary>
    bool Contains(TSelf other);

    /// <summary>Returns whether the box contains <paramref name="other" />, including minimum edges and excluding maximum edges.</summary>
    bool ContainsHalfOpen(TSelf other);

    /// <summary>Returns whether the box inclusively contains <paramref name="other" />.</summary>
    bool ContainsInclusive(TSelf other);

    /// <summary>Returns whether the box exclusively contains <paramref name="other" />.</summary>
    bool ContainsExclusive(TSelf other);

    /// <summary>Returns whether the box intersects <paramref name="other" />, counting touching edges as an intersection.</summary>
    bool Intersects(TSelf other);

    /// <summary>Returns the closest point in this box to <paramref name="point" />.</summary>
    TVector ClosestPoint(TVector point);

    /// <summary>Returns the smallest box containing both boxes.</summary>
    static abstract TSelf Union(TSelf left, TSelf right);

    /// <summary>Returns the overlapping box shared by both boxes.</summary>
    static abstract TSelf Intersection(TSelf left, TSelf right);

    /// <summary>Returns whether two boxes intersect, counting touching edges as an intersection.</summary>
    static abstract bool Intersects(TSelf left, TSelf right);
}
