namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Runs one archetypal case inside a fresh worker process.</summary>
internal sealed partial class EcsArchBenchWorker(EcsBenchOptions options)
{
    private static long longSink;
    private static object? referenceSink;
    private static EcsBenchWideValue wideSink;
    private static EcsBenchRefStruct refStructSink;

    internal int Run()
    {
        string scenarioId = options.WorkerCase!;
        EcsArchBenchSample sample = scenarioId switch
        {
            _ when scenarioId.StartsWith("arch-get-present-", StringComparison.Ordinal) => RunPoint(scenarioId, PointOperation.Get),
            _ when scenarioId.StartsWith("arch-get-absent-", StringComparison.Ordinal) => RunPoint(scenarioId, PointOperation.GetAbsent),
            _ when scenarioId.StartsWith("arch-has-present-", StringComparison.Ordinal) => RunPoint(scenarioId, PointOperation.HasPresent),
            _ when scenarioId.StartsWith("arch-has-absent-", StringComparison.Ordinal) => RunPoint(scenarioId, PointOperation.HasAbsent),
            _ when scenarioId.StartsWith("arch-set-existing-", StringComparison.Ordinal) => RunPoint(scenarioId, PointOperation.Set),
            _ when scenarioId.StartsWith("arch-membership-", StringComparison.Ordinal) => RunMembership(scenarioId),
            _ when scenarioId.StartsWith("arch-hot-", StringComparison.Ordinal) => RunHotPath(scenarioId),
            "arch-get-wide-k08" => RunWide(scenarioId, false),
            "arch-set-wide-k08" => RunWide(scenarioId, true),
            "arch-get-reference-k08" => RunReference(scenarioId, false),
            "arch-set-reference-k08" => RunReference(scenarioId, true),
            "arch-get-ref-struct-k08" => RunRefStruct(scenarioId, false),
            "arch-set-ref-struct-k08" => RunRefStruct(scenarioId, true),
            "arch-add-cached-k08" => RunAddCached(scenarioId),
            "arch-add-growth-k08" => RunAddGrowth(scenarioId),
            "arch-add-unknown-k08" => RunAddUnknown(scenarioId),
            "arch-remove-cached-k08" => RunRemoveCached(scenarioId),
            "arch-remove-unknown-k08" => RunRemoveUnknown(scenarioId),
            "arch-compact-first-k08" => RunCompaction(scenarioId, CompactionPosition.First),
            "arch-compact-middle-k08" => RunCompaction(scenarioId, CompactionPosition.Middle),
            "arch-compact-last-k08" => RunCompaction(scenarioId, CompactionPosition.Last),
            "arch-create-unique-gray" => RunUniqueSignatures(scenarioId),
            "arch-low-occupancy" => RunLowOccupancy(scenarioId),
            "arch-high-occupancy" => RunHighOccupancy(scenarioId),
            "arch-concurrent-get-a01" => RunConcurrentGet(scenarioId, 1),
            "arch-concurrent-get-many" => RunConcurrentGet(scenarioId, options.Allocs),
            "arch-concurrent-set-a01" => RunConcurrentSet(scenarioId, 1),
            "arch-concurrent-set-many" => RunConcurrentSet(scenarioId, options.Allocs),
            "arch-concurrent-resolve-many" => RunConcurrentResolve(scenarioId),
            _ => throw new ArgumentOutOfRangeException("--worker-case", $"Unknown worker case '{scenarioId}'."),
        };

        if (options.WorkerJsonPath is null)
            throw new ArgumentException("An isolated worker requires --worker-json.");

        File.WriteAllText(
            options.WorkerJsonPath,
            JsonSerializer.Serialize(sample, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }

    private EcsArchBenchSample Measure<A>(string scenarioId, Func<EcsArchBenchCase> createCase, Action? prewarm = null)
    {
        prewarm?.Invoke();
        Collect();
        long retainedBefore = GC.GetTotalMemory(true);
        var benchmarkCase = createCase();

        if (benchmarkCase.Repeatable)
        {
            for (int i = 0; i < options.Warmups; i++)
                benchmarkCase.Body();
        }

        Collect();
        int gen0 = GC.CollectionCount(0);
        int gen1 = GC.CollectionCount(1);
        int gen2 = GC.CollectionCount(2);
        long allocatedBefore = GC.GetTotalAllocatedBytes(true);
        long started = Stopwatch.GetTimestamp();

        benchmarkCase.Body();

        long elapsed = Stopwatch.GetTimestamp() - started;
        long allocated = GC.GetTotalAllocatedBytes(true) - allocatedBefore;
        benchmarkCase.Quiesce?.Invoke();
        gen0 = GC.CollectionCount(0) - gen0;
        gen1 = GC.CollectionCount(1) - gen1;
        gen2 = GC.CollectionCount(2) - gen2;
        var footprint = EcsArchFootprint.Capture<A>();
        long retainedAfter = GC.GetTotalMemory(true);
        GC.KeepAlive(benchmarkCase.Root);

        return new(
            scenarioId,
            options.SampleIndex,
            benchmarkCase.Unit,
            benchmarkCase.Operations,
            elapsed,
            allocated,
            retainedAfter - retainedBefore,
            gen0,
            gen1,
            gen2,
            footprint);
    }

    private static EntMut Alloc(EntArena alloc) => alloc.Alloc();

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private enum PointOperation
    {
        Get,
        GetAbsent,
        HasPresent,
        HasAbsent,
        Set,
    }

    private enum CompactionPosition
    {
        First,
        Middle,
        Last,
    }
}
