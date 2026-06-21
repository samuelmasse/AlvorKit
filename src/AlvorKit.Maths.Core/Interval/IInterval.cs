namespace AlvorKit.Maths;

/// <summary>Applies to inclusive one-dimensional interval types, including <c>Intervalf</c> and <c>Intervald</c>.</summary>
/// <typeparam name="TSelf">The concrete interval type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
public interface IInterval<TSelf, TScalar> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, IInterval<TSelf, TScalar>
{
    /// <summary>Gets the number of scalar components in the interval.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the interval.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets an empty interval whose minimum is greater than its maximum.</summary>
    static abstract TSelf Empty { get; }

    /// <summary>Gets or sets the inclusive lower endpoint.</summary>
    TScalar Min { get; set; }

    /// <summary>Gets or sets the inclusive upper endpoint.</summary>
    TScalar Max { get; set; }

    /// <summary>Gets whether the minimum endpoint is greater than the maximum endpoint.</summary>
    bool IsEmpty { get; }

    /// <summary>Gets the interval length, or zero when the interval is empty.</summary>
    TScalar Length { get; }

    /// <summary>Gets the midpoint between the minimum and maximum endpoints.</summary>
    TScalar Center { get; }

    /// <summary>Creates an interval from inclusive endpoints, preserving their order.</summary>
    static abstract TSelf Create(TScalar min, TScalar max);

    /// <summary>Creates an interval that contains both endpoints regardless of their order.</summary>
    static abstract TSelf CreateFromEndpoints(TScalar first, TScalar second);

    /// <summary>Creates an interval from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Gets a mutable reference to an endpoint by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets an endpoint by zero-based index, with minimum first.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies the endpoints into a caller-owned span, with minimum first.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy the endpoints into a caller-owned span, with minimum first.</summary>
    bool TryCopyTo(Span<TScalar> destination);

    /// <summary>Returns whether this interval contains <paramref name="value" />.</summary>
    bool Contains(TScalar value);

    /// <summary>Returns whether this interval fully contains <paramref name="other" />.</summary>
    bool Contains(TSelf other);

    /// <summary>Returns whether this interval overlaps <paramref name="other" />.</summary>
    bool Intersects(TSelf other);

    /// <summary>Returns the smallest interval containing both intervals.</summary>
    static abstract TSelf Union(TSelf left, TSelf right);

    /// <summary>Returns the overlapping interval shared by both intervals.</summary>
    static abstract TSelf Intersection(TSelf left, TSelf right);
}
