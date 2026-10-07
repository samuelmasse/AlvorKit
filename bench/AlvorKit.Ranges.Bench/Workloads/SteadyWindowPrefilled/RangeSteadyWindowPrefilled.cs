using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal static class RangeSteadyWindowPrefilled
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(RangeAllocator allocator, int[] handles, int operations, long address)
    {
        for (var i = 0; i < operations; i++)
        {
            var slot = i % handles.Length;
            allocator.Free(handles[slot]);
            handles[slot] = 0;
            allocator.Alloc(ref handles[slot], 8 << (i & 3), 24 + (i * 13 & 255));
            address ^= allocator.Addr(handles[slot]);
        }

        return address;
    }
}
