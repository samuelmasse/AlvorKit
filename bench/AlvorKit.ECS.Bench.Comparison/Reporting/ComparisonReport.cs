namespace AlvorKit;

internal static class ComparisonReport
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    internal static JsonSerializerOptions Options => _options;

    internal static string Write(string input, string? output)
    {
        var path = Path.GetFullPath(output ?? Path.ChangeExtension(input, ".html"));

        if (path.Equals(Path.GetFullPath(input), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The report output cannot overwrite the input JSON.");
        var html = Render(File.ReadAllText(input));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, html);
        return path;
    }

    internal static string Render(string json)
    {
        var document = JsonNode.Parse(json)!.AsObject();

        if ((string?)document["suite"] != "AlvorKit.ECS.Bench.Comparison" || (int?)document["schemaVersion"] != 9)
            throw new InvalidDataException("Expected AlvorKit.ECS.Bench.Comparison schema-9 results.");

        if ((int?)document["ecsComparison"]?["schemaVersion"] != 2)
            throw new InvalidDataException("The results have no supported ECS comparison provenance.");
        var approaches = document["ecsComparison"]!["approaches"]!.AsArray();

        foreach (var approach in approaches)
        {
            if ((string?)approach!["mode"] is not ("Scalar" or "Simd"))
                throw new InvalidDataException("The comparison suite supports scalar and SIMD workloads only.");

            var storage = (string?)approach!["storage"];
            var isAlvorKit = (string?)approach["framework"] == "AlvorKit";

            if (isAlvorKit ? storage is not ("Archetypal" or "Sparse") : storage != "")
                throw new InvalidDataException("Approaches must record the compared AlvorKit storage model.");
        }

        var groups = approaches.GroupBy(entry =>
            $"{entry!["scenario"]}/{entry["mode"]}/{entry["framework"]}/{entry["storage"]}");

        foreach (var group in groups)
        {
            if (group.Count(entry => (bool?)entry!["representative"] == true) != 1)
                throw new InvalidDataException($"Expected one representative per comparison row: {group.Key}");
        }

        foreach (var group in approaches.GroupBy(entry => $"{entry!["scenario"]}/{entry["mode"]}"))
        {
            var hasAlvorKit = group.Any(entry => (string?)entry!["framework"] == "AlvorKit");
            var hasExternal = group.Any(entry => (string?)entry!["framework"] != "AlvorKit");

            if (!hasAlvorKit || !hasExternal)
                throw new InvalidDataException($"Each workload must support AlvorKit and external comparisons: {group.Key}");
        }

        var known = approaches.Select(entry =>
            $"{entry!["scenario"]}/{entry["mode"]}/{entry["framework"]}/{entry["variant"]}").ToHashSet();
        var ids = new HashSet<string>();
        var benchmarks = document["benchmarks"]!.AsArray();

        if (benchmarks.Count == 0)
            throw new InvalidDataException("The results contain no measurements.");

        foreach (var entry in benchmarks)
        {
            var id = (string)entry!["id"]!;
            var parts = id.Split('/');

            if (parts.Length != 6 || !ids.Add(id) || (string?)entry["unit"] != "Ent")
                throw new InvalidDataException($"Invalid, repeated, or incorrectly normalized case: {id}");

            if (!parts[1].StartsWith('N') || !int.TryParse(parts[1].AsSpan(1), out var count) || count <= 0)
                throw new InvalidDataException($"Invalid matching Ent count: {id}");

            if (parts[2] != "Padding0" && (parts[2] != "Padding10" || !parts[0].StartsWith("Update")))
                throw new InvalidDataException($"Invalid padding for this scenario: {id}");

            var expectedOperations = (long)count * (parts[0].StartsWith("Create") ? 1 : 64);
            var key = $"{parts[0]}/{parts[3]}/{parts[4]}/{parts[5]}";

            if (!known.Contains(key))
                throw new InvalidDataException($"The case has no recorded approach metadata: {id}");
            var samples = entry!["samples"]!.AsArray();

            if (samples.Count == 0)
                throw new InvalidDataException("Every measured case must contain retained samples.");

            foreach (var sample in samples)
            {
                var operations = (int)sample!["operationCount"]!;
                var elapsed = (double)sample["elapsedNanoseconds"]!;

                if (operations <= 0 || !double.IsFinite(elapsed) || elapsed <= 0)
                    throw new InvalidDataException("Samples must contain positive operation counts and finite positive elapsed times.");

                if (operations != expectedOperations)
                    throw new InvalidDataException($"Operation count does not match the workload contract: {id}");
            }
        }
        var counts = benchmarks.Select(entry => ((string)entry!["id"]!).Split('/')[1]).Distinct();

        if (counts.Count() != 1)
            throw new InvalidDataException("A comparison report must use one matching Ent count.");
        return RepositoryTemplates.ForArea(typeof(ComparisonReport), "ecs-comparison")
            .Render("report.html.tmpl", ("Data", document.ToJsonString(Options)),
                ("Sources", ComparisonSources.Capture(approaches).ToJsonString(Options)));
    }
}
