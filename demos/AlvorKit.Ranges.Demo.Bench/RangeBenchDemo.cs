namespace AlvorKit.Ranges.Demo.Bench;

/// <summary>Runs focused microbenchmarks for range allocation, reuse, fragmentation, and packing.</summary>
public sealed class RangeBenchDemo(RangeBenchOptions options)
{
    private const int LinearAlignment = 16;
    private const int LinearMinSize = 32;
    private const int LinearSizeMask = 127;
    private const int HoleSize = 64;
    private const int SeparatorSize = 1;
    private const long FirstUsableIndex = 1;

    /// <summary>Stores observed addresses so benchmark loops keep visible work.</summary>
    private static long addressSink;

    /// <summary>Stores observed counts so benchmark loops keep visible allocator state.</summary>
    private static int countSink;

    /// <summary>Prints runtime information that materially affects benchmark interpretation.</summary>
    public void PrintEnvironment()
    {
        Console.WriteLine("AlvorKit ranges benchmark");
        Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"Process: {RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"GC: {(GCSettings.IsServerGC ? "server" : "workstation")}");
        Console.WriteLine($"Allocator units per run: {options.Operations:n0}, runs: {options.Runs}, warmups: {options.Warmups}");
        Console.WriteLine($"Window / fragmented-hole count: {options.Window:n0}");
        Console.WriteLine();
    }

    /// <summary>Measures all benchmark cases and returns the collected results.</summary>
    public RangeBenchResult[] MeasureAll() =>
    [
        Measure("single-range-alloc-free", "alloc", options.Operations, CreateSingleRangeAllocFree),
        Measure("steady-window-churn", "alloc", options.Operations, CreateSteadyWindowChurn),
        Measure("steady-window-prefilled", "alloc", options.Operations, CreateSteadyWindowPrefilled),
        Measure("linear-alloc-no-resize", "alloc", options.Operations, CreateLinearAllocNoResize),
        Measure("linear-alloc-with-resize", "alloc", options.Operations, CreateLinearAllocWithResize),
        Measure("same-handle-reuse-hit", "call", options.Operations, CreateSameHandleReuseHit),
        Measure("same-handle-grow-replace", "call", options.Operations, CreateSameHandleGrowReplace),
        Measure("fragmented-same-size-holes", "alloc", options.Operations, CreateFragmentedSameSizeHoles),
        Measure("fragmented-distinct-size-holes", "alloc", options.Operations, CreateFragmentedDistinctSizeHoles),
        Measure("fragmented-pack-scenario", "alloc", options.Operations, CreateFragmentedPackScenario),
        Measure("pack-only-fragmented", "range", PackLiveRangeCount(options.Operations), CreatePackOnlyFragmented),
        Measure("pack-callback-simulated-copy", "byte", PackLiveBytes(options.Operations), CreatePackCallbackSimulatedCopy),
    ];

    /// <summary>Prints benchmark results as a compact console table.</summary>
    public void PrintResults(IReadOnlyList<RangeBenchResult> results)
    {
        Console.WriteLine(
            $"{"Benchmark",-32} {"unit",-6} {"best/s",14} {"mean/s",14} {"B/unit",12} " +
            $"{"packs",7} {"resizes",7} {"live",8} {"reserved",12} {"padding",10}");

        foreach (var result in results)
        {
            Console.WriteLine(
                $"{result.Name,-32} {result.Unit,-6} {result.BestUnitsPerSecond,14:n0} " +
                $"{result.MeanUnitsPerSecond,14:n0} {result.ManagedBytesPerUnit,12:n4} " +
                $"{result.MeanPackCount,7:n1} {result.MeanResizeCount,7:n1} {result.LiveRangeCount,8:n0} " +
                $"{result.ReservedBytes,12:n0} {result.EstimatedPaddingBytes,10:n0}");
        }
    }

    /// <summary>Writes JSON benchmark results when the caller requested a result file.</summary>
    public void WriteJson(IReadOnlyList<RangeBenchResult> results)
    {
        if (options.JsonPath is null)
            return;

        string? directory = Path.GetDirectoryName(options.JsonPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(options.JsonPath, JsonSerializer.Serialize(results, jsonOptions));
    }

    /// <summary>Measures one benchmark body and reports throughput plus managed allocation cost.</summary>
    private RangeBenchResult Measure(string name, string unit, long units, Func<RangeBenchCase> createCase)
    {
        for (var i = 0; i < options.Warmups; i++)
        {
            var warmup = createCase();
            warmup.MeasuredBody();
            warmup.Cleanup();
        }

        var best = 0.0;
        var sum = 0.0;
        var managedAllocated = 0L;
        var packCount = 0.0;
        var packTime = 0.0;
        var resizeCount = 0.0;
        var resizeTime = 0.0;
        RangeBenchMetrics metrics = default;

        for (var run = 0; run < options.Runs; run++)
        {
            Collect();
            var benchmarkCase = createCase();
            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var started = Stopwatch.GetTimestamp();

            benchmarkCase.MeasuredBody();

            var elapsed = Stopwatch.GetTimestamp() - started;
            var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
            metrics = benchmarkCase.Snapshot();
            benchmarkCase.Cleanup();

            var unitsPerSecond = units * (double)Stopwatch.Frequency / elapsed;
            best = Math.Max(best, unitsPerSecond);
            sum += unitsPerSecond;
            managedAllocated += allocatedAfter - allocatedBefore;
            packCount += metrics.PackCount;
            packTime += metrics.PackTimeMilliseconds;
            resizeCount += metrics.ResizeCount;
            resizeTime += metrics.ResizeTimeMilliseconds;
        }

        return new(
            name,
            unit,
            units,
            best,
            sum / options.Runs,
            (double)managedAllocated / options.Runs / units,
            packCount / options.Runs,
            packTime / options.Runs,
            resizeCount / options.Runs,
            resizeTime / options.Runs,
            metrics.FinalFreeBlockCount,
            metrics.FinalFreeSizeCount,
            metrics.LiveRangeCount,
            metrics.ReservedBytes,
            metrics.RequestedBytes,
            metrics.EstimatedPaddingBytes);
    }

    /// <summary>Measures repeated allocation and freeing of one reusable handle.</summary>
    private RangeBenchCase CreateSingleRangeAllocFree()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters);
        var allocation = 0;
        var address = 0L;

        return new(
            () =>
            {
                for (var i = 0; i < options.Operations; i++)
                {
                    allocator.Alloc(ref allocation, LinearAlignment, LinearMinSize + (i & LinearSizeMask));
                    address ^= allocator.Addr(allocation);
                    allocator.Free(allocation);
                    allocation = 0;
                }

                addressSink = address;
                countSink = allocator.FreeBlockCount;
            },
            () => Snapshot(allocator, counters),
            () =>
            {
                if (allocation != 0)
                    allocator.Free(allocation);
            });
    }

    /// <summary>Measures allocation churn with a live-handle window that starts empty.</summary>
    private RangeBenchCase CreateSteadyWindowChurn()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters);
        var handles = new int[options.Window];
        var address = 0L;

        return new(
            () =>
            {
                for (var i = 0; i < options.Operations; i++)
                {
                    var slot = i % handles.Length;
                    if (handles[slot] != 0)
                        allocator.Free(handles[slot]);

                    var allocation = 0;
                    allocator.Alloc(ref allocation, 8 << (i & 3), 24 + (i * 13 & 255));
                    handles[slot] = allocation;
                    address ^= allocator.Addr(allocation);
                }

                addressSink = address;
                countSink = allocator.IndexSetPoolCount;
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, handles));
    }

    /// <summary>Measures true steady-state allocation churn after the live-handle window is prefilled.</summary>
    private RangeBenchCase CreateSteadyWindowPrefilled()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters);
        var handles = new int[options.Window];
        var address = 0L;

        for (var i = 0; i < handles.Length; i++)
        {
            allocator.Alloc(ref handles[i], 8 << (i & 3), 24 + (i * 13 & 255));
            address ^= allocator.Addr(handles[i]);
        }

        return new(
            () =>
            {
                for (var i = 0; i < options.Operations; i++)
                {
                    var slot = i % handles.Length;
                    allocator.Free(handles[slot]);
                    handles[slot] = 0;
                    allocator.Alloc(ref handles[slot], 8 << (i & 3), 24 + (i * 13 & 255));
                    address ^= allocator.Addr(handles[slot]);
                }

                addressSink = address;
                countSink = allocator.FreeBlockCount;
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, handles));
    }

    /// <summary>Measures linear allocation when the backing store is already large enough.</summary>
    private RangeBenchCase CreateLinearAllocNoResize()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(
            counters,
            FirstUsableIndex + (long)options.Operations * (LinearMinSize + LinearSizeMask + LinearAlignment) + 1);
        var handles = new int[options.Operations];
        var address = 0L;

        return new(
            () =>
            {
                for (var i = 0; i < handles.Length; i++)
                {
                    allocator.Alloc(ref handles[i], LinearAlignment, LinearMinSize + (i & LinearSizeMask));
                    address ^= allocator.Addr(handles[i]);
                }

                addressSink = address;
                countSink = allocator.FreeBlockCount;
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, handles));
    }

    /// <summary>Measures linear allocation from the default backing size so growth remains visible.</summary>
    private RangeBenchCase CreateLinearAllocWithResize()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters);
        var handles = new int[options.Operations];
        var address = 0L;

        return new(
            () =>
            {
                for (var i = 0; i < handles.Length; i++)
                {
                    allocator.Alloc(ref handles[i], LinearAlignment, LinearMinSize + (i & LinearSizeMask));
                    address ^= allocator.Addr(handles[i]);
                }

                addressSink = address;
                countSink = allocator.FreeBlockCount;
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, handles));
    }

    /// <summary>Measures allocation requests that hit the existing-handle reuse fast path.</summary>
    private RangeBenchCase CreateSameHandleReuseHit()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters);
        var allocation = 0;
        var address = 0L;
        allocator.Alloc(ref allocation, LinearAlignment, 256);

        return new(
            () =>
            {
                for (var i = 0; i < options.Operations; i++)
                {
                    allocator.Alloc(ref allocation, LinearAlignment, LinearMinSize + (i & LinearSizeMask));
                    address ^= allocator.Addr(allocation);
                }

                addressSink = address;
                countSink = allocator.Allocations.Length;
            },
            () => Snapshot(allocator, counters),
            () => allocator.Free(allocation));
    }

    /// <summary>Measures repeated replacement of one handle with a monotonically growing request.</summary>
    private RangeBenchCase CreateSameHandleGrowReplace()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters);
        var allocation = 0;
        var address = 0L;

        return new(
            () =>
            {
                for (var i = 0; i < options.Operations; i++)
                {
                    allocator.Alloc(ref allocation, LinearAlignment, LinearMinSize + i);
                    address ^= allocator.Addr(allocation);
                }

                addressSink = address;
                countSink = allocator.FreeSizeCount;
            },
            () => Snapshot(allocator, counters),
            () => allocator.Free(allocation));
    }

    /// <summary>Measures churn against many same-sized free blocks separated by live ranges.</summary>
    private RangeBenchCase CreateFragmentedSameSizeHoles()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters, SameSizeHoleInitialSize());
        var separators = new int[options.Window];
        var address = 0L;
        CreateSameSizeHoles(allocator, separators);

        return new(
            () =>
            {
                for (var i = 0; i < options.Operations; i++)
                {
                    var allocation = 0;
                    allocator.Alloc(ref allocation, 0, HoleSize);
                    address ^= allocator.Addr(allocation);
                    allocator.Free(allocation);
                }

                addressSink = address;
                countSink = allocator.FreeSizeCount;
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, separators));
    }

    /// <summary>Measures churn against many distinct-size free blocks separated by live ranges.</summary>
    private RangeBenchCase CreateFragmentedDistinctSizeHoles()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters, DistinctSizeHoleInitialSize());
        var separators = new int[options.Window];
        var address = 0L;
        CreateDistinctSizeHoles(allocator, separators);

        return new(
            () =>
            {
                for (var i = 0; i < options.Operations; i++)
                {
                    var allocation = 0;
                    allocator.Alloc(ref allocation, 0, 32 + (i % options.Window));
                    address ^= allocator.Addr(allocation);
                    allocator.Free(allocation);
                }

                addressSink = address;
                countSink = allocator.FreeSizeCount;
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, separators));
    }

    /// <summary>Measures allocating, fragmenting, and packing one range set without timed teardown.</summary>
    private RangeBenchCase CreateFragmentedPackScenario()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters);
        var handles = new int[options.Operations];
        var address = 0L;

        return new(
            () =>
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

                addressSink = address;
                countSink = allocator.FreeSizeCount;
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, handles));
    }

    /// <summary>Measures only the allocator pack operation after fragmentation setup has completed.</summary>
    private RangeBenchCase CreatePackOnlyFragmented()
    {
        var counters = new RangeBenchCounters();
        var allocator = CreateTrackedAllocator(counters, FragmentedPackInitialSize(options.Operations));
        var handles = CreateFragmentedPackSetup(allocator, options.Operations);

        return new(
            () =>
            {
                allocator.Pack();
                countSink = allocator.FreeBlockCount;
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, handles));
    }

    /// <summary>Measures pack with a callback that simulates relocation address and byte-count work.</summary>
    private RangeBenchCase CreatePackCallbackSimulatedCopy()
    {
        var counters = new RangeBenchCounters();
        var copyBytes = 0L;
        RangeAllocator? allocator = null;
        allocator = new(
            () =>
            {
                counters.PackCount++;
                var liveAllocations = allocator!.Allocations;
                var allocationSlots = allocator.AllocationSlots;
                var lastAllocationSlots = allocator.LastAllocationSlots;
                var movedBytes = 0L;

                for (var i = 0; i < liveAllocations.Length; i++)
                {
                    var allocation = liveAllocations[i];
                    var last = lastAllocationSlots[allocation];
                    var current = allocationSlots[allocation];
                    addressSink ^= allocator.AlignedAddr(last.Index, last.Alignment);
                    addressSink ^= allocator.AlignedAddr(current.Index, current.Alignment);
                    movedBytes += current.Size;
                }

                copyBytes += movedBytes;
            },
            _ => counters.ResizeCount++,
            FragmentedPackInitialSize(options.Operations));

        var handles = CreateFragmentedPackSetup(allocator, options.Operations);

        return new(
            () =>
            {
                allocator.Pack();
                countSink = (int)(copyBytes & int.MaxValue);
            },
            () => Snapshot(allocator, counters),
            () => FreeLiveHandles(allocator, handles));
    }

    /// <summary>Creates an allocator that records pack and resize counts through callbacks.</summary>
    private static RangeAllocator CreateTrackedAllocator(RangeBenchCounters counters, long initialSize = RangeAllocator.DefaultInitialSize) =>
        new(() => counters.PackCount++, _ => counters.ResizeCount++, initialSize);

    /// <summary>Creates same-sized holes with live separator ranges between them.</summary>
    private void CreateSameSizeHoles(RangeAllocator allocator, int[] separators)
    {
        var holes = new int[separators.Length];
        for (var i = 0; i < separators.Length; i++)
        {
            allocator.Alloc(ref holes[i], 0, HoleSize);
            allocator.Alloc(ref separators[i], 0, SeparatorSize);
        }

        FreeLiveHandles(allocator, holes);
    }

    /// <summary>Creates distinct-sized holes with live separator ranges between them.</summary>
    private void CreateDistinctSizeHoles(RangeAllocator allocator, int[] separators)
    {
        var holes = new int[separators.Length];
        for (var i = 0; i < separators.Length; i++)
        {
            allocator.Alloc(ref holes[i], 0, 32 + i);
            allocator.Alloc(ref separators[i], 0, SeparatorSize);
        }

        FreeLiveHandles(allocator, holes);
    }

    /// <summary>Creates the fragmented live range set used by pack-focused scenarios.</summary>
    private static int[] CreateFragmentedPackSetup(RangeAllocator allocator, int operations)
    {
        var handles = new int[operations];
        for (var i = 0; i < handles.Length; i++)
            allocator.Alloc(ref handles[i], LinearAlignment, LinearMinSize + (i & 63));

        for (var i = 0; i < handles.Length; i += 2)
        {
            allocator.Free(handles[i]);
            handles[i] = 0;
        }

        return handles;
    }

    /// <summary>Frees non-zero live handles after a measured benchmark body has completed.</summary>
    private static void FreeLiveHandles(RangeAllocator allocator, int[] handles)
    {
        for (var i = 0; i < handles.Length; i++)
        {
            if (handles[i] == 0)
                continue;

            allocator.Free(handles[i]);
            handles[i] = 0;
        }
    }

    /// <summary>Captures allocator state for the benchmark result after the timed body completes.</summary>
    private static RangeBenchMetrics Snapshot(RangeAllocator allocator, RangeBenchCounters counters)
    {
        var requested = 0L;
        var reserved = 0L;
        var handles = allocator.Allocations;
        var slots = allocator.AllocationSlots;

        for (var i = 0; i < handles.Length; i++)
        {
            var slot = slots[handles[i]];
            requested += slot.Size;
            reserved += slot.Size + allocator.AlignedAddr(slot.Index, slot.Alignment) - slot.Index;
        }

        return new(
            counters.PackCount,
            allocator.PackTime,
            counters.ResizeCount,
            allocator.ResizeTime,
            allocator.FreeBlockCount,
            allocator.FreeSizeCount,
            handles.Length,
            reserved,
            requested,
            reserved - requested);
    }

    /// <summary>Returns the number of live ranges packed by pack-only scenarios.</summary>
    private static long PackLiveRangeCount(int operations) => operations / 2;

    /// <summary>Returns the total live payload bytes moved by the simulated-copy pack scenario.</summary>
    private static long PackLiveBytes(int operations)
    {
        var total = 0L;
        for (var i = 1; i < operations; i += 2)
            total += LinearMinSize + (i & 63);

        return total;
    }

    /// <summary>Returns a no-resize initial size for fragmented pack setup.</summary>
    private static long FragmentedPackInitialSize(int operations) =>
        FirstUsableIndex + (long)operations * (LinearMinSize + 63 + LinearAlignment) + 1;

    /// <summary>Returns a no-resize initial size for same-size fragmented-hole setup.</summary>
    private long SameSizeHoleInitialSize() => FirstUsableIndex + (long)options.Window * (HoleSize + SeparatorSize) + 1;

    /// <summary>Returns a no-resize initial size for distinct-size fragmented-hole setup.</summary>
    private long DistinctSizeHoleInitialSize()
    {
        var size = FirstUsableIndex;
        for (var i = 0; i < options.Window; i++)
            size += 32 + i + SeparatorSize;

        return size + 1;
    }

    /// <summary>Runs a full collection pass before a measured benchmark run.</summary>
    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed class RangeBenchCounters
    {
        internal int PackCount;

        internal int ResizeCount;
    }

    private readonly record struct RangeBenchCase(Action MeasuredBody, Func<RangeBenchMetrics> Snapshot, Action Cleanup);

    private readonly record struct RangeBenchMetrics(
        int PackCount,
        double PackTimeMilliseconds,
        int ResizeCount,
        double ResizeTimeMilliseconds,
        int FinalFreeBlockCount,
        int FinalFreeSizeCount,
        int LiveRangeCount,
        long ReservedBytes,
        long RequestedBytes,
        long EstimatedPaddingBytes);
}
