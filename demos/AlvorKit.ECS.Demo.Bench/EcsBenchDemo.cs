namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Runs focused ECS microbenchmarks for hot component and entity lifetime paths.</summary>
public sealed class EcsBenchDemo(EcsBenchOptions options)
{
    private static int intSink;
    private static EntHandle handleSink;

    /// <summary>Prints runtime information that materially affects benchmark interpretation.</summary>
    public void PrintEnvironment()
    {
        Console.WriteLine("AlvorKit ECS benchmark");
        Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"Process: {RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"GC: {(GCSettings.IsServerGC ? "server" : "workstation")}");
        Console.WriteLine($"Operations: {options.Operations:n0}, runs: {options.Runs}, warmups: {options.Warmups}");
        Console.WriteLine();
    }

    /// <summary>Measures all benchmark cases and returns the collected results.</summary>
    public EcsBenchResult[] MeasureAll() =>
    [
        Measure("component-set-existing", ComponentSetExisting),
        Measure("component-get-existing", ComponentGetExisting),
        Measure("component-has-existing", ComponentHasExisting),
        Measure("component-unset-set", ComponentUnsetSet),
        Measure("entptr-alloc-set-dispose", EntPtrAllocSetDispose),
        Measure("arena-alloc-set-dispose", ArenaAllocSetDispose),
        Measure("arena-alloc-set-bulk-dispose", ArenaAllocSetBulkDispose),
    ];

    /// <summary>Prints benchmark results as a compact console table.</summary>
    public void PrintResults(IReadOnlyList<EcsBenchResult> results)
    {
        Console.WriteLine($"{"Benchmark",-32} {"best ns/op",12} {"mean ns/op",12} {"alloc B/op",12}");
        foreach (var result in results)
        {
            Console.WriteLine(
                $"{result.Name,-32} {result.BestNanosecondsPerOperation,12:n2} " +
                $"{result.MeanNanosecondsPerOperation,12:n2} {result.AllocatedBytesPerOperation,12:n4}");
        }
    }

    /// <summary>Writes JSON benchmark results when the caller requested a result file.</summary>
    public void WriteJson(IReadOnlyList<EcsBenchResult> results)
    {
        if (options.JsonPath is null)
            return;

        string? directory = Path.GetDirectoryName(options.JsonPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(options.JsonPath, JsonSerializer.Serialize(results, jsonOptions));
    }

    private EcsBenchResult Measure(string name, Action<int> body)
    {
        for (int i = 0; i < options.Warmups; i++)
            body(options.Operations);

        double best = double.PositiveInfinity;
        double sum = 0;
        long allocated = 0;

        for (int run = 0; run < options.Runs; run++)
        {
            Collect();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            long started = Stopwatch.GetTimestamp();

            body(options.Operations);

            long elapsed = Stopwatch.GetTimestamp() - started;
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
            double nsPerOp = elapsed * 1_000_000_000.0 / Stopwatch.Frequency / options.Operations;
            best = Math.Min(best, nsPerOp);
            sum += nsPerOp;
            allocated += allocatedAfter - allocatedBefore;
        }

        return new(name, options.Operations, best, sum / options.Runs, (double)allocated / options.Runs / options.Operations);
    }

    private static void ComponentSetExisting(int operations)
    {
        var ent = new EntPtr();
        try
        {
            ent.First = 0;

            for (int i = 0; i < operations; i++)
                ent.First = i;

            intSink = ent.First;
        }
        finally
        {
            ent.Dispose();
        }
    }

    private static void ComponentGetExisting(int operations)
    {
        var ent = new EntPtr();
        try
        {
            ent.First = 17;
            int sum = 0;

            for (int i = 0; i < operations; i++)
                sum += ent.First;

            intSink = sum;
        }
        finally
        {
            ent.Dispose();
        }
    }

    private static void ComponentHasExisting(int operations)
    {
        var ent = new EntPtr();
        try
        {
            ent.First = 17;
            int count = 0;

            for (int i = 0; i < operations; i++)
            {
                if (ent.HasFirst)
                    count++;
            }

            intSink = count;
        }
        finally
        {
            ent.Dispose();
        }
    }

    private static void ComponentUnsetSet(int operations)
    {
        var ent = new EntPtr();
        try
        {
            ent.First = 0;

            for (int i = 0; i < operations; i++)
            {
                ent.UnsetFirst();
                ent.First = i;
            }

            intSink = ent.First;
        }
        finally
        {
            ent.Dispose();
        }
    }

    private static void EntPtrAllocSetDispose(int operations)
    {
        for (int i = 0; i < operations; i++)
        {
            var ent = new EntPtr { First = i };
            handleSink = ent.Handle;
            ent.Dispose();
        }
    }

    private static void ArenaAllocSetDispose(int operations)
    {
        using var arena = new EntArena();

        for (int i = 0; i < operations; i++)
        {
            var ent = arena.Alloc();
            ent.First = i;
            handleSink = ent.Handle;
            ent.Dispose();
        }
    }

    private static void ArenaAllocSetBulkDispose(int operations)
    {
        using var arena = new EntArena();

        for (int i = 0; i < operations; i++)
        {
            var ent = arena.Alloc();
            ent.First = i;
            ent.Second = i + 1;
            handleSink = ent.Handle;
        }
    }

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
