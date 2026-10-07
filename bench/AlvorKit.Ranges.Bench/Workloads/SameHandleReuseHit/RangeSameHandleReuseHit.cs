using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal static class RangeSameHandleReuseHit
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(RangeAllocator allocator, int operations, int passes, ref int allocation, long address)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < operations; i++)
            {
                allocator.Alloc(ref allocation, LinearAlignment, LinearMinSize + (i & LinearSizeMask));
                address ^= allocator.Addr(allocation);
            }
        }

        return address;
    }
}
