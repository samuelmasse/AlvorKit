namespace AlvorKit;

internal static class ComparisonReport
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
    internal static JsonSerializerOptions Options => options;

    internal static string Write(string input, string? output, string? baseline = null)
    {
        var path = Path.GetFullPath(output ?? Path.ChangeExtension(input, ".html"));

        if (path.Equals(Path.GetFullPath(input), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The report output cannot overwrite the input JSON.");

        if (baseline != null && path.Equals(Path.GetFullPath(baseline), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The report output cannot overwrite the baseline JSON.");

        var document = JsonNode.Parse(File.ReadAllText(input))!.AsObject();

        if (baseline != null)
        {
            var previous = JsonNode.Parse(File.ReadAllText(baseline))!.AsObject();
            Validate(previous);
            document["baseline"] = previous;
        }

        document["rawDataPath"] = Path.GetRelativePath(Path.GetDirectoryName(path)!, Path.GetFullPath(input))
            .Replace('\\', '/');
        var html = Render(document.ToJsonString());
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, html);
        return path;
    }

    internal static string Render(string json)
    {
        var document = JsonNode.Parse(json)!.AsObject();
        Validate(document);
        PrepareEmbeddedData(document);
        return RepositoryTemplates.ForArea(typeof(ComparisonReport), "ecs-comparison")
            .Render("report.html.tmpl", ("Data", document.ToJsonString(new(Options) { WriteIndented = false })));
    }

    /// <summary>Embeds the chart's exact inputs without duplicate runs or unused warmup history.</summary>
    private static void PrepareEmbeddedData(JsonObject document)
    {
        foreach (var entry in document["benchmarks"]!.AsArray())
        {
            if (entry!["warmups"] is not JsonArray warmups)
                continue;

            var tail = warmups.GroupBy(sample => (int?)sample!["launch"] ?? 0)
                .SelectMany(launch => launch.TakeLast(3)).Select(sample => sample!.DeepClone()).ToArray();
            entry["warmups"] = new JsonArray(tail);
        }

        if (document["reportRuns"] is JsonArray runs)
        {
            foreach (var run in runs)
            {
                var origin = run!.AsObject();
                origin.Remove("benchmarks");
                origin.Remove("diagnostics");
                origin["ecsComparison"]!.AsObject().Remove("experiments");
                origin["ecsComparison"]!.AsObject().Remove("sourceDiff");
            }
        }

        if (document["baseline"] is JsonObject baseline)
            PrepareEmbeddedData(baseline);
    }

    internal static void Validate(JsonObject document)
    {
        if ((string?)document["suite"] != "AlvorKit.ECS.Bench.Comparison" || (int?)document["schemaVersion"] != 9)
            throw new InvalidDataException("Expected ECS comparison schema-9 measurements.");

        if ((int?)document["ecsComparison"]?["schemaVersion"] != 3)
            throw new InvalidDataException("Expected comparison contract schema 3.");

        if (document["reportRuns"] is JsonArray runs)
        {
            if (runs.Count < 2)
                throw new InvalidDataException("A combined report requires at least two contributing runs.");

            foreach (var run in runs)
            {
                if (run!["reportRuns"] != null)
                    throw new InvalidDataException("Nested report collections are not supported.");

                Validate(run.AsObject());
                ComparisonReportCollection.RequireSameEnvironment(document, run.AsObject());
            }
        }

        var experiments = document["ecsComparison"]!["experiments"]!.AsArray();
        var contracts = new Dictionary<string, JsonNode>();

        foreach (var experiment in experiments)
        {
            var id = (string)experiment!["id"]!;

            if (!contracts.TryAdd(id, experiment))
                throw new InvalidDataException($"Repeated contract: {id}");

            if ((int)experiment["operations"]! <= 0 || (int)experiment["input"]!["count"]! <= 0)
                throw new InvalidDataException($"Nonpositive operation or population count: {id}");

            var input = experiment["input"]!;

            if ((int)input["passes"]! <= 0 || (int)input["padding"]! < 0)
                throw new InvalidDataException($"Invalid workload dimensions: {id}");
        }

        var ids = new HashSet<string>();
        var benchmarks = document["benchmarks"]!.AsArray();

        if (benchmarks.Count == 0)
            throw new InvalidDataException("The results contain no measurements.");

        foreach (var entry in benchmarks)
        {
            var id = (string)entry!["id"]!;

            if (!ids.Add(id) || !contracts.TryGetValue(id, out var contract))
                throw new InvalidDataException($"Repeated or unknown case: {id}");

            if ((string?)entry["unit"] != (string?)contract["unit"])
                throw new InvalidDataException($"Operation unit differs from the recorded contract: {id}");

            var samples = entry["samples"]!.AsArray();

            if (samples.Count == 0)
                throw new InvalidDataException($"No retained samples: {id}");

            foreach (var sample in samples)
            {
                var elapsed = (double)sample!["elapsedNanoseconds"]!;

                if (!double.IsFinite(elapsed) || elapsed <= 0 || (int)sample["operationCount"]! != (int)contract["operations"]!)
                    throw new InvalidDataException($"Invalid timing or operation normalization: {id}");
            }

            if (document["reportRuns"] is JsonArray origins)
                ValidateOrigin(origins, entry.AsObject(), contract);
            else if (document["study"] != null)
                ValidateLaunches(document, entry.AsObject());
        }
    }

    private static void ValidateOrigin(JsonArray runs, JsonObject entry, JsonNode contract)
    {
        if ((int?)entry["reportRun"] is not { } index || index < 0 || index >= runs.Count)
            throw new InvalidDataException("Missing or invalid contributing run.");

        var id = (string)entry["id"]!;
        var run = runs[index]!;
        var original = run["benchmarks"]!.AsArray().SingleOrDefault(item => (string?)item!["id"] == id);
        var input = run["ecsComparison"]!["experiments"]!.AsArray().SingleOrDefault(item => (string?)item!["id"] == id);
        var measured = entry.DeepClone().AsObject();
        measured.Remove("reportRun");

        if (!JsonNode.DeepEquals(original, measured) || !JsonNode.DeepEquals(input, contract))
            throw new InvalidDataException($"Combined data differs from its contributing run: {id}");
    }

    private static void ValidateLaunches(JsonObject document, JsonObject entry)
    {
        var launches = (int)document["study"]!["launches"]!;
        var samples = entry["samples"]!.AsArray();

        if (launches <= 0 || samples.Any(sample => (int?)sample!["launch"] is not { } launch || launch < 0 || launch >= launches))
            throw new InvalidDataException("Invalid independent-launch identity.");

        var groups = samples.GroupBy(sample => (int)sample!["launch"]!).ToArray();
        var perLaunch = (int)document["selection"]!["sampleCount"]!;

        if (groups.Length != launches || groups.Any(group => group.Count() != perLaunch))
            throw new InvalidDataException("Independent launches have missing or extra retained samples.");

        var means = groups.Select(group => group.Average(sample =>
            (double)sample!["elapsedNanoseconds"]! / (int)sample["operationCount"]!)).ToArray();
        var expected = JsonSerializer.SerializeToNode(BenchStatistics.Estimate(means), Options);

        if (!JsonNode.DeepEquals(expected, entry["launchEstimate"]))
            throw new InvalidDataException("Launch estimates disagree with the retained raw samples.");
    }
}
