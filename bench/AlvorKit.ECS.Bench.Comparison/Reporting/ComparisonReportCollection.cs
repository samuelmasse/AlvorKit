namespace AlvorKit;

/// <summary>Publishes measured cases together while retaining each contributing run and its exact source.</summary>
internal static class ComparisonReportCollection
{
    internal static string Write(string[] inputs, string output)
    {
        var jsonPath = Path.GetFullPath(output);
        var htmlPath = Path.ChangeExtension(jsonPath, ".html");

        if (!jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The combined output must be a .json file; its .html report is written beside it.");

        foreach (var input in inputs)
        {
            var path = Path.GetFullPath(input);

            if (path.Equals(jsonPath, StringComparison.OrdinalIgnoreCase) || path.Equals(htmlPath, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Combined output cannot overwrite a contributing input.");
        }

        var documents = inputs.Select(path => JsonNode.Parse(File.ReadAllText(path))!.AsObject()).ToArray();
        var combined = Combine(documents);
        var json = combined.ToJsonString(ComparisonReport.Options);
        var html = ComparisonReport.Render(json);
        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
        File.WriteAllText(jsonPath, json);
        File.WriteAllText(htmlPath, html);
        return htmlPath;
    }

    internal static JsonObject Combine(JsonObject[] documents)
    {
        if (documents.Length < 2)
            throw new ArgumentException("Combine requires at least two measurement documents.");

        foreach (var document in documents)
        {
            if (document["reportRuns"] != null)
                throw new InvalidDataException("Supply original measurement documents, not an already combined report.");

            ComparisonReport.Validate(document);
            RequireSameEnvironment(documents[0], document);
        }

        var combined = documents[0].DeepClone().AsObject();
        var contracts = new Dictionary<string, JsonNode>();
        var cases = new Dictionary<string, JsonNode>();
        var diagnostics = new JsonObject();

        foreach (var document in documents)
        {
            foreach (var contract in document["ecsComparison"]!["experiments"]!.AsArray())
                contracts.TryAdd((string)contract!["id"]!, contract);
        }

        for (var index = 0; index < documents.Length; index++)
        {
            var document = documents[index];
            var inputContracts = document["ecsComparison"]!["experiments"]!.AsArray()
                .ToDictionary(entry => (string)entry!["id"]!);

            foreach (var entry in document["benchmarks"]!.AsArray())
            {
                var id = (string)entry!["id"]!;
                var measurement = entry.DeepClone().AsObject();
                measurement["reportRun"] = index;
                cases[id] = measurement;
                contracts[id] = inputContracts[id]!;
                diagnostics[id] = document["diagnostics"]?[id]?.DeepClone();
            }
        }

        combined["benchmarks"] = new JsonArray([.. cases.Values]);
        combined["ecsComparison"]!["experiments"] = new JsonArray([.. contracts.Values.Select(entry => entry.DeepClone())]);
        combined["diagnostics"] = diagnostics;
        combined.Remove("study");
        combined.Remove("baseline");
        combined["reportRuns"] = new JsonArray([.. documents.Select(document => (JsonNode)document.DeepClone())]);
        ComparisonReport.Validate(combined);
        return combined;
    }

    internal static void RequireSameEnvironment(JsonObject first, JsonObject next)
    {
        if (!JsonNode.DeepEquals(first["environment"], next["environment"]))
            throw new InvalidDataException("Combined runs must use the same runtime and operating-system environment.");

        foreach (var key in new[] { "cpu", "packages", "runtimeOverrides", "vector128", "vector256", "vector512" })
        {
            if (!JsonNode.DeepEquals(first["ecsComparison"]![key], next["ecsComparison"]![key]))
                throw new InvalidDataException($"Combined runs disagree on {key}.");
        }

        if ((string?)first["study"]?["state"] == "ProcessCold" || (string?)next["study"]?["state"] == "ProcessCold")
            throw new InvalidDataException("Cold-process measurements must remain in a separate report.");
    }
}
