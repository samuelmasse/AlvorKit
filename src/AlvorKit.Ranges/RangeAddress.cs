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

        if (BitOperations.IsPow2(alignment))
        {
            var mask = alignment - 1L;
            return (index + mask) & ~mask;
        }

        var rem = index % alignment;
        return rem == 0 ? index : index + alignment - rem;
    }

    /// <summary>Returns the byte count between an unaligned range start and its aligned payload address.</summary>
    internal static long Padding(long index, int alignment) => Align(index, alignment) - index;
}
