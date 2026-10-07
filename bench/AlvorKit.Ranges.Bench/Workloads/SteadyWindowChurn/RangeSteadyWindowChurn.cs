using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal static class RangeSteadyWindowChurn
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(RangeAllocator allocator, int[] handles, int operations, long address)
    {
        for (var i = 0; i < operations; i++)
        {
            var slot = i % handles.Length;

            if (handles[slot] != 0)
                allocator.Free(handles[slot]);
            var allocation = 0;
            allocator.Alloc(ref allocation, 8 << (i & 3), 24 + (i * 13 & 255));
            handles[slot] = allocation;
            address ^= allocator.Addr(allocation);
        }

        return address;
    }
}
