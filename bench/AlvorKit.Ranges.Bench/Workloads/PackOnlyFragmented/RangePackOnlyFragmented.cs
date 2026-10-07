namespace AlvorKit;

internal static class RangePackOnlyFragmented
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(RangeAllocator[] allocators)
    {
        foreach (var allocator in allocators)
            allocator.Pack();
    }
}
