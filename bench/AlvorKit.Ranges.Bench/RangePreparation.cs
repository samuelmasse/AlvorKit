namespace AlvorKit;

internal static class RangePreparation
{
    internal const int LinearAlignment = 16;
    internal const int LinearMinSize = 32;
    internal const int LinearSizeMask = 127;
    internal const int HoleSize = 64;
    internal const int SeparatorSize = 1;
    internal const int ShrinkPackCapacity = 320;
    internal const int ShrinkPackMinSize = 64;
    internal const int ShrinkPackSizeMask = 15;
    internal const int ShrinkPackSizeStep = 8;
    internal const long FirstUsableIndex = 1;
    /// <summary>Creates same-sized holes with live separator ranges between them.</summary>
    internal static void CreateSameSizeHoles(RangeAllocator allocator, int[] separators)
    {
        var holes = new int[separators.Length];

        for (var i = 0; i < separators.Length; i++)
        {
            allocator.Alloc(ref holes[i], 0, HoleSize);
            allocator.Alloc(ref separators[i], 0, SeparatorSize);
        }

        FreeLiveHandles(allocator, holes);
    }

    /// <summary>Creates distinct-sized holes with live separator ranges between them.</summary>
    internal static void CreateDistinctSizeHoles(RangeAllocator allocator, int[] separators)
    {
        var holes = new int[separators.Length];

        for (var i = 0; i < separators.Length; i++)
        {
            allocator.Alloc(ref holes[i], 0, 32 + i);
            allocator.Alloc(ref separators[i], 0, SeparatorSize);
        }

        FreeLiveHandles(allocator, holes);
    }

    /// <summary>Creates the fragmented live range set used by pack-focused scenarios.</summary>
    internal static int[] CreateFragmentedPackSetup(RangeAllocator allocator, int operations)
    {
        var handles = new int[operations];

        for (var i = 0; i < handles.Length; i++)
            allocator.Alloc(ref handles[i], LinearAlignment, LinearMinSize + (i & 63));

        for (var i = 0; i < handles.Length; i += 2)
        {
            allocator.Free(handles[i]);
            handles[i] = 0;
        }

        return handles;
    }

    /// <summary>Frees non-zero live handles after a measured benchmark body has completed.</summary>
    internal static void FreeLiveHandles(RangeAllocator allocator, int[] handles)
    {
        for (var i = 0; i < handles.Length; i++)
        {
            if (handles[i] == 0)
                continue;
            allocator.Free(handles[i]);
            handles[i] = 0;
        }
    }

    /// <summary>Returns the number of live ranges packed by pack-only scenarios.</summary>
    internal static int PackLiveRangeCount(int operations) => operations / 2;
    /// <summary>Returns the total live payload bytes moved by the simulated-copy pack scenario.</summary>
    internal static int PackLiveBytes(int operations)
    {
        var total = 0;

        for (var i = 1; i < operations; i += 2)
            total += LinearMinSize + (i & 63);
        return total;
    }

    /// <summary>Returns a no-resize initial size for fragmented pack setup.</summary>
    internal static long FragmentedPackInitialSize(int operations) =>
        FirstUsableIndex + (long)operations * (LinearMinSize + 63 + LinearAlignment) + 1;
    /// <summary>Returns a no-resize initial size for same-handle shrink-pack setup.</summary>
    internal static long ShrinkPackInitialSize(int ranges) =>
        FirstUsableIndex + (long)ranges * (ShrinkPackCapacity + ShrinkPackSizeMask * ShrinkPackSizeStep + LinearAlignment) + 1;
    /// <summary>Returns a no-resize initial size for same-size fragmented-hole setup.</summary>
    internal static long SameSizeHoleInitialSize(int window) =>
        FirstUsableIndex + (long)window * (HoleSize + SeparatorSize) + 1;
    /// <summary>Returns a no-resize initial size for distinct-size fragmented-hole setup.</summary>
    internal static long DistinctSizeHoleInitialSize(int window)
    {
        var size = FirstUsableIndex;

        for (var i = 0; i < window; i++)
            size += 32 + i + SeparatorSize;
        return size + 1;
    }
}
