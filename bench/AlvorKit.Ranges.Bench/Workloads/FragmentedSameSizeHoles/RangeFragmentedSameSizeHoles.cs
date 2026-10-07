using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal static class RangeFragmentedSameSizeHoles
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(RangeAllocator allocator, int operations, long address)
    {
        for (var i = 0; i < operations; i++)
        {
            var allocation = 0;
            allocator.Alloc(ref allocation, 0, HoleSize);
            address ^= allocator.Addr(allocation);
            allocator.Free(allocation);
        }

        return address;
    }
}
