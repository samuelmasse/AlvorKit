namespace AlvorKit.Ranges.Test;

[TestClass]
public sealed class RangeAllocatorTest
{
    /// <summary>Allocating a range records its requested size, alignment, and live handle.</summary>
    [TestMethod]
    public void Alloc_TracksLiveAlignedRange()
    {
        var allocator = new RangeAllocator();
        var allocation = 0;

        allocator.Alloc(ref allocation, 8, 16);

        Assert.AreNotEqual(0, allocation);
        Assert.AreEqual(24, allocator.Used);
        Assert.AreEqual(1, allocator.Allocations.Length);
        Assert.AreEqual(16, allocator.AllocationSlots[allocation].Size);
        Assert.AreEqual(8, allocator.AllocationSlots[allocation].Alignment);
        Assert.AreEqual(8, allocator.Addr(allocation));
    }

    /// <summary>Allocating at an already aligned index returns that exact address while reserving max needed padding.</summary>
    [TestMethod]
    public void Alloc_WhenIndexAlreadyAligned_UsesExactAddressAndMaxReservedPadding()
    {
        var allocator = new RangeAllocator(initialSize: 100);
        var prefix = 0;
        var aligned = 0;
        allocator.Alloc(ref prefix, 0, 7);

        allocator.Alloc(ref aligned, 8, 8);

        Assert.AreEqual(8, allocator.AllocationSlots[aligned].Index);
        Assert.AreEqual(8, allocator.Addr(aligned));
        Assert.AreEqual(23, allocator.Used);
    }

    /// <summary>Best-fit allocation reuses an exact-alignment hole that can hold max reserved padding.</summary>
    [TestMethod]
    public void Alloc_WhenAlignedHoleHasReservedCapacity_ReusesHole()
    {
        var allocator = new RangeAllocator(initialSize: 100);
        var prefix = 0;
        var hole = 0;
        var suffix = 0;
        allocator.Alloc(ref prefix, 0, 7);
        allocator.Alloc(ref hole, 0, 15);
        allocator.Alloc(ref suffix, 0, 1);
        allocator.Free(hole);

        var replacement = 0;
        allocator.Alloc(ref replacement, 8, 8);

        Assert.AreEqual(8, allocator.AllocationSlots[replacement].Index);
        Assert.AreEqual(8, allocator.Addr(replacement));
        Assert.AreEqual(24, allocator.Used);
    }

    /// <summary>Freeing adjacent ranges coalesces them back into one free block.</summary>
    [TestMethod]
    public void Free_CoalescesAdjacentRanges()
    {
        var allocator = new RangeAllocator();
        var first = 0;
        var second = 0;
        allocator.Alloc(ref first, 0, 10);
        allocator.Alloc(ref second, 0, 20);

        allocator.Free(first);
        allocator.Free(second);

        Assert.AreEqual(1, allocator.Used);
        Assert.AreEqual(1, allocator.FreeBlockCount);
        Assert.AreEqual(0, allocator.Allocations.Length);
    }

    /// <summary>Packing moves live ranges forward and exposes previous slots during the callback.</summary>
    [TestMethod]
    public void Pack_CompactsLiveRangesAndReportsPreviousSlots()
    {
        RangeAllocator? allocator = null;
        var callbackCount = 0;
        var movedAllocation = 0;
        RangeAllocation last = default;
        RangeAllocation current = default;
        allocator = new(() =>
        {
            callbackCount++;
            movedAllocation = allocator!.Allocations[0];
            last = allocator.LastAllocationSlots[movedAllocation];
            current = allocator.AllocationSlots[movedAllocation];
        });
        var first = 0;
        var second = 0;
        allocator.Alloc(ref first, 0, 10);
        allocator.Alloc(ref second, 0, 20);
        allocator.Free(first);

        allocator.Pack();

        Assert.AreEqual(1, callbackCount);
        Assert.AreEqual(second, movedAllocation);
        Assert.AreEqual(11, last.Index);
        Assert.AreEqual(1, current.Index);
        Assert.AreEqual(21, allocator.Used);
    }

    /// <summary>Allocating beyond current capacity grows the backing-store size and invokes the resize callback.</summary>
    [TestMethod]
    public void Alloc_WhenRangeDoesNotFit_ResizesBackingStore()
    {
        var resizedTo = 0L;
        var allocator = new RangeAllocator(resizeCallback: size => resizedTo = size);
        var allocation = 0;

        allocator.Alloc(ref allocation, 0, RangeAllocator.DefaultInitialSize);

        Assert.AreNotEqual(0, allocation);
        Assert.AreEqual(resizedTo, allocator.Size);
        Assert.IsTrue(allocator.Size > RangeAllocator.DefaultInitialSize);
    }

    /// <summary>Allocating zero bytes clears an existing allocation handle.</summary>
    [TestMethod]
    public void Alloc_WithZeroSize_ClearsExistingAllocation()
    {
        var allocator = new RangeAllocator();
        var allocation = 0;
        allocator.Alloc(ref allocation, 0, 10);

        allocator.Alloc(ref allocation, 0, 0);

        Assert.AreEqual(0, allocation);
        Assert.AreEqual(1, allocator.Used);
        Assert.AreEqual(0, allocator.Allocations.Length);
    }

    /// <summary>Allocating zero bytes on an empty handle remains a no-op.</summary>
    [TestMethod]
    public void Alloc_WithZeroSizeAndEmptyHandle_RemainsEmpty()
    {
        var allocator = new RangeAllocator();
        var allocation = 0;

        allocator.Alloc(ref allocation, 0, 0);

        Assert.AreEqual(0, allocation);
        Assert.AreEqual(1, allocator.Used);
    }

    /// <summary>Freeing the empty handle is a no-op.</summary>
    [TestMethod]
    public void Free_WithEmptyHandle_DoesNothing()
    {
        var allocator = new RangeAllocator();

        allocator.Free(0);

        Assert.AreEqual(1, allocator.Used);
        Assert.AreEqual(0, allocator.Allocations.Length);
    }

    /// <summary>Zero alignment returns the original address.</summary>
    [TestMethod]
    public void AlignedAddr_WithZeroAlignment_ReturnsOriginalIndex()
    {
        var allocator = new RangeAllocator();

        Assert.AreEqual(5, allocator.AlignedAddr(5, 0));
    }

    /// <summary>Non-power-of-two alignment still rounds up to the next valid multiple.</summary>
    [TestMethod]
    public void AlignedAddr_WithNonPowerOfTwoAlignment_RoundsUp()
    {
        var allocator = new RangeAllocator();

        Assert.AreEqual(6, allocator.AlignedAddr(5, 3));
    }

    /// <summary>Negative sizes and alignments fail clearly instead of changing allocator state.</summary>
    [TestMethod]
    public void Alloc_WithNegativeInputs_Throws()
    {
        var allocator = new RangeAllocator();
        var allocation = 0;

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => allocator.Alloc(ref allocation, -1, 10));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => allocator.Alloc(ref allocation, 0, -1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => allocator.AlignedAddr(1, -1));
        Assert.AreEqual(0, allocation);
        Assert.AreEqual(1, allocator.Used);
    }

    /// <summary>Invalid initial sizes fail before any backing state is created.</summary>
    [TestMethod]
    public void Constructor_WithInvalidInitialSize_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new RangeAllocator(initialSize: 1));
    }

    /// <summary>Allocating into the same handle keeps the existing range when it already fits.</summary>
    [TestMethod]
    public void Alloc_WhenExistingRangeFits_ReusesCurrentRange()
    {
        var allocator = new RangeAllocator();
        var allocation = 0;
        allocator.Alloc(ref allocation, 8, 32);
        var address = allocator.Addr(allocation);
        var used = allocator.Used;

        allocator.Alloc(ref allocation, 8, 16);

        Assert.AreEqual(address, allocator.Addr(allocation));
        Assert.AreEqual(used, allocator.Used);
        Assert.AreEqual(1, allocator.Allocations.Length);
    }

    /// <summary>Allocating into the same handle frees and replaces the existing range when it no longer fits.</summary>
    [TestMethod]
    public void Alloc_WhenExistingRangeDoesNotFit_ReplacesCurrentRange()
    {
        var allocator = new RangeAllocator();
        var allocation = 0;
        allocator.Alloc(ref allocation, 0, 10);

        allocator.Alloc(ref allocation, 0, 20);

        Assert.AreNotEqual(0, allocation);
        Assert.AreEqual(21, allocator.Used);
        Assert.AreEqual(20, allocator.AllocationSlots[allocation].Size);
    }

    /// <summary>Fragmented allocation packs when total used space is still below the resize threshold.</summary>
    [TestMethod]
    public void Alloc_WhenFragmentedAndNotNearCapacity_PacksBeforeAllocating()
    {
        var packs = 0;
        var allocator = new RangeAllocator(() => packs++, initialSize: 100);
        var first = 0;
        var second = 0;
        var third = 0;
        allocator.Alloc(ref first, 0, 30);
        allocator.Alloc(ref second, 0, 30);
        allocator.Free(first);

        allocator.Alloc(ref third, 0, 50);

        Assert.AreEqual(1, packs);
        Assert.AreNotEqual(0, third);
        Assert.AreEqual(81, allocator.Used);
        Assert.IsTrue(allocator.IndexSetPoolCount > 0);
        Assert.IsTrue(allocator.PackTime >= 0);
    }

    /// <summary>Packing without a callback still compacts live ranges and resets free space.</summary>
    [TestMethod]
    public void Pack_WithoutCallback_CompactsLiveRanges()
    {
        var allocator = new RangeAllocator();
        var first = 0;
        var second = 0;
        allocator.Alloc(ref first, 0, 10);
        allocator.Alloc(ref second, 0, 20);
        allocator.Free(first);

        allocator.Pack();

        Assert.AreEqual(1, allocator.AllocationSlots[second].Index);
        Assert.AreEqual(21, allocator.Used);
        Assert.AreEqual(1, allocator.FreeBlockCount);
    }

    /// <summary>Resize also works without a callback and exposes resize timing.</summary>
    [TestMethod]
    public void Alloc_WhenFull_ResizesWithoutCallback()
    {
        var allocator = new RangeAllocator(initialSize: 16);
        var first = 0;
        var second = 0;
        allocator.Alloc(ref first, 0, 15);

        allocator.Alloc(ref second, 0, 1);

        Assert.AreEqual(32, allocator.Size);
        Assert.AreEqual(17, allocator.Used);
        Assert.IsTrue(allocator.ResizeTime >= 0);
    }

    /// <summary>Freed allocation handles and same-sized free block sets are reused.</summary>
    [TestMethod]
    public void Alloc_ReusesFreedHandlesAndEqualSizeFreeBlocks()
    {
        var allocator = new RangeAllocator(initialSize: 100);
        var first = 0;
        var second = 0;
        var third = 0;
        var fourth = 0;
        allocator.Alloc(ref first, 0, 10);
        allocator.Alloc(ref second, 0, 10);
        allocator.Alloc(ref third, 0, 10);
        allocator.Alloc(ref fourth, 0, 10);
        allocator.Free(first);
        allocator.Free(third);

        var replacement = 0;
        allocator.Alloc(ref replacement, 0, 10);

        Assert.AreEqual(first, replacement);
        Assert.AreEqual(2, allocator.FreeSizeCount);
        Assert.IsTrue(allocator.IndexSetPoolCount >= 0);
    }

    /// <summary>Allocation slot storage grows while keeping dense live handle ordering.</summary>
    [TestMethod]
    public void Alloc_MoreThanInitialHandleCapacity_GrowsStorage()
    {
        var allocator = new RangeAllocator();
        var handles = new int[5];

        for (var i = 0; i < handles.Length; i++)
            allocator.Alloc(ref handles[i], 0, 1);

        CollectionAssert.AreEqual(handles, allocator.Allocations.ToArray());
    }
}
