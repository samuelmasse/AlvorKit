namespace AlvorKit.Ranges;

/// <summary>Tracks live allocation handles, allocation slots, and reusable slot indexes.</summary>
internal sealed class RangeAllocationList
{
    private readonly Queue<int> freeAllocations = [];
    private RangeAllocation[] slots = [default];
    private RangeAllocation[] lastSlots = [default];
    private int[] handles = new int[4];
    private int slotCount = 1;
    private int count;

    /// <summary>Gets the number of live allocation handles.</summary>
    internal int Count => count;

    /// <summary>Gets allocation slots by handle index.</summary>
    internal ReadOnlySpan<RangeAllocation> Slots => new(slots, 0, slotCount);

    /// <summary>Gets slots captured before the latest pack operation.</summary>
    internal ReadOnlySpan<RangeAllocation> LastSlots => new(lastSlots, 0, slotCount);

    /// <summary>Gets dense live allocation handles in current packing order.</summary>
    internal ReadOnlySpan<int> Handles => new(handles, 0, count);

    /// <summary>Adds a live allocation slot and returns its handle.</summary>
    internal int Add(long index, long size, int alignment)
    {
        var handle = NextSlot();
        if (count == handles.Length)
            Array.Resize(ref handles, handles.Length * 2);

        handles[count] = handle;
        slots[handle] = new()
        {
            Index = index,
            Size = size,
            Alignment = alignment,
            Rank = count++,
        };

        return handle;
    }

    /// <summary>Captures the current slot for later relocation callbacks.</summary>
    internal void CaptureLast(int handle) => lastSlots[handle] = slots[handle];

    /// <summary>Gets a live allocation handle by dense rank.</summary>
    internal int HandleAt(int rank) => handles[rank];

    /// <summary>Removes a live allocation and returns its previous slot.</summary>
    internal RangeAllocation Remove(int handle)
    {
        var slot = slots[handle];
        var lastHandle = handles[count - 1];
        ref var lastSlot = ref slots[lastHandle];
        handles[slot.Rank] = lastHandle;
        lastSlot.Rank = slot.Rank;
        slots[handle] = default;
        freeAllocations.Enqueue(handle);
        count--;
        return slot;
    }

    /// <summary>Gets the slot for <paramref name="handle"/> by reference.</summary>
    internal ref RangeAllocation Slot(int handle) => ref slots[handle];

    /// <summary>Returns a reused or newly-created allocation slot index.</summary>
    private int NextSlot()
    {
        if (freeAllocations.Count > 0)
            return freeAllocations.Dequeue();

        if (slotCount == slots.Length)
        {
            Array.Resize(ref slots, slotCount * 2);
            Array.Resize(ref lastSlots, slotCount * 2);
        }

        return slotCount++;
    }
}
