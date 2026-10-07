using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal static class RangeLinearAllocWithResize
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(RangeAllocator allocator, int[] handles, long address)
    {
        for (var i = 0; i < handles.Length; i++)
        {
            allocator.Alloc(ref handles[i], LinearAlignment, LinearMinSize + (i & LinearSizeMask));
            address ^= allocator.Addr(handles[i]);
        }

        return address;
    }
}
