namespace AlvorKit.Ranges;

/// <summary>Provides address arithmetic for aligned range offsets.</summary>
internal static class RangeAddress
{
    /// <summary>Returns the aligned address at or after <paramref name="index"/>.</summary>
    internal static long Align(long index, int alignment)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(alignment);

        if (alignment == 0)
            return index;

        var rem = index % alignment;
        return index + alignment - rem;
    }
}
