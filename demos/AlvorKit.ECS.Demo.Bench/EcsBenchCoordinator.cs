namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Coordinates core measurements and isolated archetypal worker samples.</summary>
internal sealed class EcsBenchCoordinator(EcsBenchOptions options)
{
    private const int ReportSchemaVersion = 2;

    internal int Run()
    {
        var catalog = EcsArchBenchScenarios.Create(options.Widths);
        if (options.List)
        {
            foreach (var scenario in catalog)
                Console.WriteLine(scenario.Id);
            return 0;
        }

        var selected = Select(catalog);
        PrintEnvironment(selected.Length);

        EcsBenchResult[] coreResults = [];
        if (options.Suite == EcsBenchOptions.AllSuite)
        {
            var core = new EcsBenchDemo(options);
            coreResults = core.MeasureAll();
            core.PrintResults(coreResults);
            Console.WriteLine();
        }

        var archResults = new EcsArchBenchResult[selected.Length];
        for (int i = 0; i < selected.Length; i++)
        {
            archResults[i] = Measure(selected[i]);
            PrintProgress(i + 1, selected.Length, archResults[i]);
        }

        Console.WriteLine();
        PrintResults(archResults);
        WriteJson(coreResults, archResults);
        return 0;
    }

    private EcsArchBenchScenario[] Select(EcsArchBenchScenario[] catalog)
    {
        if (options.Cases.Length == 0)
            return catalog;

        var selected = new EcsArchBenchScenario[options.Cases.Length];
        for (int i = 0; i < options.Cases.Length; i++)
        {
            string requested = options.Cases[i];
            int index = Array.FindIndex(catalog, scenario => scenario.Id.Equals(requested, StringComparison.Ordinal));
            if (index < 0)
                throw new ArgumentOutOfRangeException("--cases", $"Unknown archetypal case '{requested}'. Use --list to inspect IDs.");
            selected[i] = catalog[index];
        }

        return selected;
    }

    private EcsArchBenchResult Measure(EcsArchBenchScenario scenario)
    {
        var samples = new EcsArchBenchSample[options.Runs];
        var ns = new double[samples.Length];
        double allocatedPerOperation = 0;

        for (int sampleIndex = 0; sampleIndex < samples.Length; sampleIndex++)
        {
            var sample = EcsBenchProcess.Run(options, scenario.Id, sampleIndex);
            samples[sampleIndex] = sample;
            ns[sampleIndex] = sample.ElapsedTicks * 1_000_000_000.0 / Stopwatch.Frequency / sample.Operations;
            allocatedPerOperation += (double)sample.AllocatedBytes / sample.Operations;
        }

        Array.Sort(ns);
        double sum = 0;
        for (int i = 0; i < ns.Length; i++)
            sum += ns[i];

        double median = ns.Length % 2 == 0
            ? (ns[ns.Length / 2 - 1] + ns[ns.Length / 2]) / 2
            : ns[ns.Length / 2];

        return new(
            scenario.Id,
            scenario.Category,
            scenario.Unit,
            scenario.Width,
            samples[0].Operations,
            ns[0],
            median,
            sum / ns.Length,
            allocatedPerOperation / samples.Length,
            samples[^1].Footprint,
            samples);
    }

    private void PrintEnvironment(int caseCount)
    {
        Console.WriteLine("AlvorKit ECS archetypal benchmark");
        Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"Process: {RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"GC: {(GCSettings.IsServerGC ? "server" : "workstation")}");
        Console.WriteLine($"Cases: {caseCount}, isolated runs: {options.Runs}, warmups: {options.Warmups}");
        Console.WriteLine(
            $"Operations: {options.Operations:n0}, arches: {options.Arches:n0}, rows: {options.Rows:n0}, allocs: {options.Allocs}");
        Console.WriteLine();
    }

    private static void PrintProgress(int current, int count, EcsArchBenchResult result) =>
        Console.WriteLine($"[{current,2}/{count,2}] {result.ScenarioId}: {result.MeanNanosecondsPerOperation:n2} ns/{result.Unit}");

    private static void PrintResults(IReadOnlyList<EcsArchBenchResult> results)
    {
        Console.WriteLine(
            $"{"Benchmark",-40} {"K",3} {"best ns",11} {"median ns",11} {"mean ns",11} {"alloc B",10}");
        foreach (var result in results)
        {
            Console.WriteLine(
                $"{result.ScenarioId,-40} {result.Width,3} {result.BestNanosecondsPerOperation,11:n2} " +
                $"{result.MedianNanosecondsPerOperation,11:n2} {result.MeanNanosecondsPerOperation,11:n2} " +
                $"{result.MeanAllocatedBytesPerOperation,10:n4}");
        }

        Console.WriteLine();
        Console.WriteLine(
            $"{"Benchmark",-40} {"arches",9} {"edges",9} {"states",8} {"rows",9} " +
            $"{"slack",9} {"logical B",12} {"managed B",12} {"objects",8}");
        foreach (var result in results)
        {
            var footprint = result.Footprint;
            Console.WriteLine(
                $"{result.ScenarioId,-40} {footprint.MaterializedArchCount,9:n0} " +
                $"{footprint.DirectedStructuralEdgeCount,9:n0} {footprint.ActiveStateCount,8:n0} " +
                $"{footprint.ActiveRowCount,9:n0} {footprint.RowSlack,9:n0} " +
                $"{footprint.TotalLogicalRetainedBytes,12:n0} {footprint.EstimatedManagedBytes,12:n0} " +
                $"{footprint.OwnedManagedObjectCount,8:n0}");
        }
    }

    private void WriteJson(EcsBenchResult[] coreResults, EcsArchBenchResult[] archResults)
    {
        if (options.JsonPath is null)
            return;

        string? directory = Path.GetDirectoryName(options.JsonPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var report = new EcsBenchReport(
            ReportSchemaVersion,
            options.Label,
            DateTimeOffset.UtcNow,
            new(
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.ProcessArchitecture.ToString(),
                RuntimeInformation.OSDescription,
                GCSettings.IsServerGC ? "server" : "workstation",
                Environment.ProcessorCount,
                Stopwatch.Frequency),
            new(
                options.Suite,
                options.Operations,
                options.Runs,
                options.Warmups,
                options.Widths,
                options.Arches,
                options.Rows,
                options.Allocs,
                options.Cases),
            coreResults,
            archResults);

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(options.JsonPath, JsonSerializer.Serialize(report, jsonOptions));
    }
}
