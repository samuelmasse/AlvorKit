namespace AlvorKit.Ranges.Demo.Bench;

/// <summary>Runs focused microbenchmarks for range allocation, reuse, fragmentation, and packing.</summary>
public sealed class RangeBenchDemo(RangeBenchOptions options)
{
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
        Console.WriteLine($"Allocator allocations per run: {options.Operations:n0}, runs: {options.Runs}, warmups: {options.Warmups}");
        Console.WriteLine($"Rolling window: {options.Window:n0} live handles");
        Console.WriteLine();
    }

    /// <summary>Measures all benchmark cases and returns the collected results.</summary>
    public RangeBenchResult[] MeasureAll() =>
    [
        Measure("alloc-free-reuse", AllocFreeReuse),
        Measure("rolling-window-fragmentation", RollingWindowFragmentation),
        Measure("pack-fragmented-ranges", PackFragmentedRanges),
    ];

    /// <summary>Prints benchmark results as a compact console table.</summary>
    public void PrintResults(IReadOnlyList<RangeBenchResult> results)
    {
        Console.WriteLine($"{"Benchmark",-32} {"best alloc/s",14} {"mean alloc/s",14} {"managed B/alloc",16}");
        foreach (var result in results)
        {
            Console.WriteLine(
                $"{result.Name,-32} {result.BestAllocationsPerSecond,14:n0} " +
                $"{result.MeanAllocationsPerSecond,14:n0} {result.ManagedBytesPerAllocation,16:n4}");
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

    /// <summary>Measures one benchmark body and reports allocator allocation throughput plus managed allocation cost.</summary>
    private RangeBenchResult Measure(string name, Action<int> body)
    {
        for (var i = 0; i < options.Warmups; i++)
            body(options.Operations);

        var best = 0.0;
        var sum = 0.0;
        var managedAllocated = 0L;

        for (var run = 0; run < options.Runs; run++)
        {
            Collect();
            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var started = Stopwatch.GetTimestamp();

            body(options.Operations);

            var elapsed = Stopwatch.GetTimestamp() - started;
            var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
            var allocationsPerSecond = options.Operations * (double)Stopwatch.Frequency / elapsed;
            best = Math.Max(best, allocationsPerSecond);
            sum += allocationsPerSecond;
            managedAllocated += allocatedAfter - allocatedBefore;
        }

        return new(
            name,
            options.Operations,
            best,
            sum / options.Runs,
            (double)managedAllocated / options.Runs / options.Operations);
    }

    /// <summary>Measures repeated allocation and freeing of one reusable handle.</summary>
    private static void AllocFreeReuse(int operations)
    {
        var allocator = new RangeAllocator();
        var allocation = 0;
        var address = 0L;

        for (var i = 0; i < operations; i++)
        {
            allocator.Alloc(ref allocation, 16, 32 + (i & 127));
            address ^= allocator.Addr(allocation);
            allocator.Free(allocation);
            allocation = 0;
        }

        addressSink = address;
        countSink = allocator.FreeBlockCount;
    }

    /// <summary>Measures allocation churn with a fixed live-handle window.</summary>
    private void RollingWindowFragmentation(int operations)
    {
        var allocator = new RangeAllocator();
        var handles = new int[options.Window];
        var address = 0L;

        for (var i = 0; i < operations; i++)
        {
            var slot = i % handles.Length;
            if (handles[slot] != 0)
                allocator.Free(handles[slot]);

            var allocation = 0;
            allocator.Alloc(ref allocation, 8 << (i & 3), 24 + (i * 13 & 255));
            handles[slot] = allocation;
            address ^= allocator.Addr(allocation);
        }

        for (var i = 0; i < handles.Length; i++)
        {
            if (handles[i] != 0)
                allocator.Free(handles[i]);
        }

        addressSink = address;
        countSink = allocator.IndexSetPoolCount;
    }

    /// <summary>Measures packing after freeing every other allocation.</summary>
    private static void PackFragmentedRanges(int operations)
    {
        var allocator = new RangeAllocator();
        var handles = new int[operations];
        var address = 0L;

        for (var i = 0; i < handles.Length; i++)
            allocator.Alloc(ref handles[i], 16, 32 + (i & 63));

        for (var i = 0; i < handles.Length; i += 2)
        {
            allocator.Free(handles[i]);
            handles[i] = 0;
        }

        allocator.Pack();

        for (var i = 1; i < handles.Length; i += 2)
        {
            address ^= allocator.Addr(handles[i]);
            allocator.Free(handles[i]);
        }

        addressSink = address;
        countSink = allocator.FreeSizeCount;
    }

    /// <summary>Runs a full collection pass before a measured benchmark run.</summary>
    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
