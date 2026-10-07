using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal static class RangeSingleRangeAllocFree
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(RangeAllocator allocator, int operations, ref int allocation, long address)
    {
        for (var i = 0; i < operations; i++)
        {
            allocator.Alloc(ref allocation, LinearAlignment, LinearMinSize + (i & LinearSizeMask));
            address ^= allocator.Addr(allocation);
            allocator.Free(allocation);
            allocation = 0;
        }

        return address;
    }
}
