namespace AlvorKit;

internal static class RangeSameHandleShrinkPack
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(RangeAllocator[] allocators)
    {
        foreach (var allocator in allocators)
            allocator.Pack();
    }
}
