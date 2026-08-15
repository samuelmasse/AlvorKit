namespace AlvorKit;

/// <summary>Physical baseline-IL region owned by one interception claim.</summary>
public readonly struct InterceptionPhysicalRegion : IEquatable<InterceptionPhysicalRegion>
{
    private InterceptionPhysicalRegion(
        InterceptionPhysicalRegionKind kind,
        int offset,
        int length)
    {
        Kind = kind;
        Offset = offset;
        Length = length;
    }

    /// <summary>Gets whether this region covers a whole method or one IL range.</summary>
    public InterceptionPhysicalRegionKind Kind { get; }

    /// <summary>Gets the original baseline IL offset for an IL-range claim.</summary>
    public int Offset { get; }

    /// <summary>Gets the number of baseline IL bytes covered by an IL-range claim.</summary>
    public int Length { get; }

    /// <summary>Gets the region covering an entire loaded method.</summary>
    public static InterceptionPhysicalRegion MethodWide { get; } =
        new(InterceptionPhysicalRegionKind.MethodWide, 0, 0);

    /// <summary>Creates one exact non-empty baseline IL range.</summary>
    public static InterceptionPhysicalRegion IlRange(
        int offset,
        int length = 1)
    {
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (length <= 0 || offset > int.MaxValue - length)
            throw new ArgumentOutOfRangeException(nameof(length));
        return new(
            InterceptionPhysicalRegionKind.IlRange,
            offset,
            length);
    }

    internal bool Overlaps(InterceptionPhysicalRegion other)
    {
        if (Kind == InterceptionPhysicalRegionKind.MethodWide ||
            other.Kind == InterceptionPhysicalRegionKind.MethodWide)
        {
            return true;
        }

        return Offset < other.Offset + other.Length &&
            other.Offset < Offset + Length;
    }

    internal bool IsValid =>
        Kind == InterceptionPhysicalRegionKind.MethodWide
            ? Offset == 0 && Length == 0
            : Kind == InterceptionPhysicalRegionKind.IlRange &&
                Offset >= 0 &&
                Length > 0 &&
                Offset <= int.MaxValue - Length;

    /// <inheritdoc />
    public bool Equals(InterceptionPhysicalRegion other) =>
        Kind == other.Kind &&
        Offset == other.Offset &&
        Length == other.Length;

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is InterceptionPhysicalRegion other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(Kind, Offset, Length);

    /// <inheritdoc />
    public override string ToString() =>
        Kind == InterceptionPhysicalRegionKind.MethodWide
            ? "method-wide"
            : $"IL_{Offset:X4}..IL_{Offset + Length:X4}";

    /// <summary>Tests exact physical-region identity.</summary>
    public static bool operator ==(
        InterceptionPhysicalRegion left,
        InterceptionPhysicalRegion right) =>
        left.Equals(right);

    /// <summary>Tests exact physical-region inequality.</summary>
    public static bool operator !=(
        InterceptionPhysicalRegion left,
        InterceptionPhysicalRegion right) =>
        !left.Equals(right);
}
