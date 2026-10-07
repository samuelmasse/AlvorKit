namespace AlvorKit;

/// <summary>Visits the same relocation metadata as the former simulated-copy experiment.</summary>
internal class RangeRelocation
{
    private RangeAllocator allocator = null!;
    private long observed;
    internal long Observed => observed;

    internal void Attach(RangeAllocator value) => allocator = value;
    internal void Copy()
    {
        var slots = allocator.AllocationSlots;
        var previous = allocator.LastAllocationSlots;

        foreach (var allocation in allocator.Allocations)
        {
            var last = previous[allocation];
            var current = slots[allocation];
            observed ^= allocator.AlignedAddr(last.Index, last.Alignment);
            observed ^= allocator.AlignedAddr(current.Index, current.Alignment);
            observed += current.Size;
        }
    }
}
