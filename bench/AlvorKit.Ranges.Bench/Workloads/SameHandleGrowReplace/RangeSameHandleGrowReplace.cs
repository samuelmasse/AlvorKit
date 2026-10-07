using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal static class RangeSameHandleGrowReplace
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(RangeAllocator allocator, int operations, ref int allocation, long address)
    {
        for (var i = 0; i < operations; i++)
        {
            allocator.Alloc(ref allocation, LinearAlignment, LinearMinSize + i);
            address ^= allocator.Addr(allocation);
        }

        return address;
    }
}
