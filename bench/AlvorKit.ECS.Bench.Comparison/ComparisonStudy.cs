namespace AlvorKit;

/// <summary>Randomized independent-process studies; every child and its launch identity remain archived.</summary>
internal static class ComparisonStudy
{
    internal static int Run(string[] args)
    {
        var patterns = new Argument<string[]>("glob") { Arity = ArgumentArity.ZeroOrMore };
        var output = new Option<string>("--output") { Required = true };
        var launches = new Option<int>("--launches") { DefaultValueFactory = _ => 5 };
        var warmup = new Option<int>("--warmup") { DefaultValueFactory = _ => 201 };
        var samples = new Option<int>("--samples") { DefaultValueFactory = _ => 9 };
        var seed = new Option<int>("--seed") { DefaultValueFactory = _ => 1729 };
        var cold = new Option<bool>("--cold");
        var pilot = new Option<double>("--pilot-ms") { DefaultValueFactory = _ => 2 };
        var command = new RootCommand("Run selected cases in independent processes; save raw children and aggregate results.")
        {
            patterns, output, launches, warmup, samples, seed, cold, pilot,
        };
        command.SetAction(result => Execute(result.GetValue(patterns) ?? [], result.GetValue(output)!,
            result.GetValue(launches), result.GetValue(warmup), result.GetValue(samples), result.GetValue(seed),
            result.GetValue(cold), result.GetValue(pilot)));
        return command.Parse(args).Invoke();
    }

    internal static int Worker(string[] args)
    {
        if (args.Length != 5)
            throw new ArgumentException("Worker requires ID, warmups, samples, output, and passes.");

        var experiment = ComparisonExperiments.All.Single(entry => entry.Id == args[0]);
        var passes = int.Parse(args[4]);

        if (passes != experiment.Input.Passes)
            experiment = experiment.WithPasses(passes);

        ComparisonWorkerBenchmarks.Select(experiment);
        var status = BenchHost.Run<ComparisonWorkerBenchmarks>(new BenchCommand(BenchCommandKind.Run, [args[0]], [],
            int.Parse(args[1]), int.Parse(args[2]), args[3], new(true, false, false)));

        if (status != 0)
            return status;

        var document = JsonNode.Parse(File.ReadAllText(args[3]))!.AsObject();
        using var process = Process.GetCurrentProcess();
        document["processPeakWorkingSetBytes"] = process.PeakWorkingSet64;
        File.WriteAllText(args[3], document.ToJsonString(ComparisonReport.Options));
        return 0;
    }

    private static int Execute(string[] patterns, string output, int launches, int warmup, int samples, int seed, bool cold,
        double pilotMilliseconds)
    {
        if (launches < 1 || warmup < 0 || samples < 1)
            throw new ArgumentException("Launches and samples must be positive; warmups must be nonnegative.");

        if (cold)
        {
            warmup = 0;
            samples = 1;
        }

        var catalog = new BenchCatalogBuilder().Create(new ComparisonBenchmarks().Create());
        var selectedIds = new BenchSelector().Select(catalog, patterns, []).Select(entry => entry.Id).ToHashSet();
        var authored = ComparisonExperiments.All;
        var selected = authored.Where(entry => selectedIds.Contains(entry.Id)).ToArray();

        if (selected.Length == 0)
            throw new ArgumentException("No cases match this study.");

        var directory = Path.GetFullPath(output);

        if (Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any())
            throw new ArgumentException("Use an empty study directory to preserve prior measurements.");

        Directory.CreateDirectory(directory);
        var pilots = new JsonArray();

        if (cold)
            selected = [.. selected.Select(entry => entry.Parameterized == null ? entry : entry.WithPasses(1))];
        else selected = ComparisonCalibration.Prepare(selected, directory, pilotMilliseconds, warmup, pilots);

        var actual = selected.ToDictionary(entry => entry.Id);
        var provenance = ComparisonProvenance.Capture(authored.Select(entry => actual.GetValueOrDefault(entry.Id, entry)));
        var schedule = Enumerable.Range(0, launches).SelectMany(launch => selected.Select(entry => (launch, entry.Id))).ToArray();
        new Random(seed).Shuffle(schedule);
        var processRunner = new BenchProcessRunner();
        var cases = new Dictionary<string, JsonObject>();
        var children = new JsonArray();
        JsonObject? combined = null;
        var elapsed = Stopwatch.StartNew();
        File.WriteAllText(Path.Combine(directory, "study-plan.json"), new JsonObject
        {
            ["ecsComparison"] = provenance.DeepClone(),
            ["schedule"] = JsonSerializer.SerializeToNode(schedule.Select(item => new { item.launch, item.Id })),
            ["launches"] = launches, ["warmup"] = warmup, ["samples"] = samples, ["seed"] = seed,
            ["state"] = cold ? "ProcessCold" : "WarmedStorage",
        }.ToJsonString(ComparisonReport.Options));

        for (var index = 0; index < schedule.Length; index++)
        {
            var (launch, id) = schedule[index];
            var path = Path.Combine(directory, $"child-{index:D5}.json");
            Console.WriteLine($"[{index + 1}/{schedule.Length}] launch {launch + 1}: {id}");
            var child = processRunner.Run(typeof(ComparisonStudy).Assembly.Location,
                ["worker", id, warmup.ToString(), samples.ToString(), path, actual[id].Input.Passes.ToString()],
                TimeSpan.FromMinutes(5));
            File.WriteAllText(Path.ChangeExtension(path, ".log"), child.Output + child.Errors);

            if (child.ExitCode != 0)
                throw new IOException($"Child failed ({child.ExitCode}): {id}. See {Path.ChangeExtension(path, ".log")}");

            var document = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            combined ??= document.DeepClone().AsObject();
            var measurement = document["benchmarks"]![0]!.DeepClone().AsObject();

            foreach (var phase in new[] { "warmups", "samples" })
            {
                foreach (var sample in measurement[phase]!.AsArray())
                    sample!["launch"] = launch;
            }

            if (!cases.TryGetValue(id, out var aggregate))
                cases[id] = measurement;
            else
            {
                foreach (var phase in new[] { "warmups", "samples" })
                {
                    foreach (var sample in measurement[phase]!.AsArray())
                        aggregate[phase]!.AsArray().Add(sample!.DeepClone());
                }
            }

            children.Add(new JsonObject
            {
                ["case"] = id, ["launch"] = launch, ["order"] = index, ["file"] = Path.GetFileName(path),
                ["peakProcessWorkingSetBytes"] = document["processPeakWorkingSetBytes"]!.DeepClone(),
            });
        }

        foreach (var entry in cases.Values)
        {
            var estimates = entry["samples"]!.AsArray().GroupBy(sample => (int)sample!["launch"]!)
                .Select(group => group.Average(sample => (double)sample!["elapsedNanoseconds"]! / (int)sample["operationCount"]!))
                .ToArray();
            entry["launchEstimate"] = JsonSerializer.SerializeToNode(BenchStatistics.Estimate(estimates), ComparisonReport.Options);
            var samplesForSummary = entry["samples"]!.Deserialize<BenchJsonSample[]>(ComparisonReport.Options)!;
            entry["summary"] = JsonSerializer.SerializeToNode(
                BenchStatistics.SummarizeSamples(samplesForSummary), ComparisonReport.Options);
        }

        combined!["benchmarks"] = new JsonArray([.. cases.Values.Select(entry => (JsonNode)entry)]);
        combined["selection"] = new JsonObject
        {
            ["include"] = JsonSerializer.SerializeToNode(patterns), ["exclude"] = new JsonArray(),
            ["warmupCount"] = warmup, ["sampleCount"] = samples,
        };
        combined["ecsComparison"] = provenance;
        combined["durationMilliseconds"] = elapsed.Elapsed.TotalMilliseconds;
        combined["study"] = new JsonObject
        {
            ["launches"] = launches, ["seed"] = seed, ["state"] = cold ? "ProcessCold" : "WarmedStorage",
            ["children"] = children, ["calibration"] = pilots,
        };
        var destination = Path.Combine(directory, "results.json");
        File.WriteAllText(destination, combined.ToJsonString(ComparisonReport.Options));
        Console.WriteLine($"Report: {ComparisonReport.Write(destination, null)}");
        return 0;
    }
}
