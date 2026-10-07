using static AlvorKit.RangePreparation;

namespace AlvorKit;

internal class RangePackFixture(RangeAllocator allocator, int[] handles, RangeRelocation? relocation) : IDisposable
{
    internal RangeAllocator Allocator => allocator;
    internal long Observation => relocation?.Observed ?? allocator.Allocations.Length;

    internal static RangePackFixture Shrunk(int window)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize, ShrinkPackInitialSize(window));
        var handles = new int[window];

        for (var i = 0; i < handles.Length; i++)
        {
            var size = ShrinkPackCapacity + (i & ShrinkPackSizeMask) * ShrinkPackSizeStep;
            allocator.Alloc(ref handles[i], LinearAlignment, size);
            allocator.Alloc(ref handles[i], LinearAlignment, ShrinkPackMinSize + (i & ShrinkPackSizeMask));
        }

        return new(allocator, handles, null);
    }

    internal static RangePackFixture Fragmented(int operations)
    {
        var counters = new RangeCounters();
        var allocator = new RangeAllocator(counters.Pack, counters.Resize, FragmentedPackInitialSize(operations));
        return new(allocator, CreateFragmentedPackSetup(allocator, operations), null);
    }

    internal static RangePackFixture Relocated(int operations)
    {
        var relocation = new RangeRelocation();
        var allocator = new RangeAllocator(relocation.Copy, _ => { }, FragmentedPackInitialSize(operations));
        relocation.Attach(allocator);
        return new(allocator, CreateFragmentedPackSetup(allocator, operations), relocation);
    }

    public void Dispose() => FreeLiveHandles(allocator, handles);
}
