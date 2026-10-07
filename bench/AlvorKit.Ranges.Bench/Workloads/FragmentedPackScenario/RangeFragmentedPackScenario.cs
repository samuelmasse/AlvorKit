using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal static class RangeFragmentedPackScenario
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long Run(RangeAllocator allocator, int[] handles, long address)
    {
        for (var i = 0; i < handles.Length; i++)
            allocator.Alloc(ref handles[i], LinearAlignment, LinearMinSize + (i & 63));

        for (var i = 0; i < handles.Length; i += 2)
        {
            allocator.Free(handles[i]);
            handles[i] = 0;
        }

        allocator.Pack();

        for (var i = 1; i < handles.Length; i += 2)
            address ^= allocator.Addr(handles[i]);
        return address;
    }
}
