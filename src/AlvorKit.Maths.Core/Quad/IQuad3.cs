namespace AlvorKit.Maths;

/// <summary>Applies to 3D quad types, including <c>Quad3</c> and <c>Quad3d</c>.</summary>
/// <typeparam name="TSelf">The concrete quad type.</typeparam>
/// <typeparam name="TScalar">The component type, such as <see cref="float" /> or <see cref="double" />.</typeparam>
/// <typeparam name="TVector3">The matching three-component vector type.</typeparam>
/// <typeparam name="TBox3">The matching 3D axis-aligned box type.</typeparam>
public interface IQuad3<TSelf, TScalar, TVector3, TBox3> :
    IEquatable<TSelf>,
    IComparable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    ISpanFormattable,
    IUtf8SpanFormattable,
    ISpanParsable<TSelf>,
    IUtf8SpanParsable<TSelf>
    where TSelf : struct, IQuad3<TSelf, TScalar, TVector3, TBox3>
    where TVector3 : struct, IVec3<TVector3, TScalar>
    where TBox3 : struct, IBox3<TBox3, TScalar, TVector3>
{
    /// <summary>Gets the number of scalar components in the quad.</summary>
    static abstract int ComponentCount { get; }

    /// <summary>Gets the byte size of the quad.</summary>
    static abstract int SizeInBytes { get; }

    /// <summary>Gets or sets the top-left corner.</summary>
    TVector3 TopLeft { get; set; }

    /// <summary>Gets or sets the top-right corner.</summary>
    TVector3 TopRight { get; set; }

    /// <summary>Gets or sets the bottom-left corner.</summary>
    TVector3 BottomLeft { get; set; }

    /// <summary>Gets or sets the bottom-right corner.</summary>
    TVector3 BottomRight { get; set; }

    /// <summary>Gets the average of the four corners.</summary>
    TVector3 Center { get; }

    /// <summary>Gets the axis-aligned box containing all four corners.</summary>
    TBox3 Bounds { get; }

    /// <summary>Creates a quad from four corners.</summary>
    static abstract TSelf Create(TVector3 topLeft, TVector3 topRight, TVector3 bottomLeft, TVector3 bottomRight);

    /// <summary>Creates a quad from the first <see cref="ComponentCount" /> scalar values in a span.</summary>
    static abstract TSelf Create(ReadOnlySpan<TScalar> values);

    /// <summary>Gets a mutable reference to a scalar component by zero-based index.</summary>
    static abstract ref TScalar ComponentRef(ref TSelf value, int index);

    /// <summary>Gets or sets a scalar component by zero-based index, with corners in top-left, top-right, bottom-left, bottom-right order.</summary>
    TScalar this[int index] { get; set; }

    /// <summary>Copies corner components into a caller-owned span in top-left, top-right, bottom-left, bottom-right order.</summary>
    void CopyTo(Span<TScalar> destination);

    /// <summary>Attempts to copy corner components into a caller-owned span in top-left, top-right, bottom-left, bottom-right order.</summary>
    bool TryCopyTo(Span<TScalar> destination);
}
