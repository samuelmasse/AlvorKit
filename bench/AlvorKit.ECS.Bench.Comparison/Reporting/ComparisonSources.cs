namespace AlvorKit;

/// <summary>Captures exact source text with the run so saved reports do not depend on the current checkout.</summary>
internal static class ComparisonSources
{
    internal static JsonObject Capture(IEnumerable<ComparisonExperiment> experiments)
    {
        var root = ProjectRoot.FindFromCurrentProcess(typeof(ComparisonSources), requireResDirectory: true);
        var project = Path.Combine(root, "bench", "AlvorKit.ECS.Bench.Comparison");
        var files = Directory.GetFiles(project, "*.cs", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetFileNameWithoutExtension(path), path => path);
        var locations = new JsonObject();
        var contents = new JsonObject();

        foreach (var directory in new[]
        {
            project,
            Path.Combine(root, "src", "AlvorKit.Bench"),
            Path.Combine(root, "src", "AlvorKit.ECS"),
            Path.Combine(root, "src", "AlvorKit.ECS.Indexed"),
            Path.Combine(root, "src", "AlvorKit.ECS.Generator"),
            Path.Combine(root, "res", "templates", "ecs", "source-generator"),
        })
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(path => Path.GetExtension(path) is ".cs" or ".csproj" or ".tmpl"))
                contents[Path.GetRelativePath(root, file).Replace('\\', '/')] = File.ReadAllText(file);
        }

        var template = "res/templates/ecs-comparison/report.html.tmpl";
        contents[template] = File.ReadAllText(Path.Combine(root, template));

        foreach (var entry in experiments)
        {
            var workload = files[entry.Workload.Name];
            var measurement = files[entry.MeasurementMethod.DeclaringType!.Name];
            locations[entry.Id] = new JsonObject
            {
                ["workload"] = Location(root, workload, " Run(", contents),
                ["measurement"] = Location(root, measurement, $" BenchResult {entry.MeasurementMethod.Name}(", contents),
            };
        }

        return new JsonObject { ["root"] = root, ["locations"] = locations, ["files"] = contents };
    }

    private static JsonObject Location(string root, string path, string declaration, JsonObject contents)
    {
        var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        var text = File.ReadAllText(path);
        var matches = text.Split('\n').Select((line, index) => (line, index))
            .Where(item => item.line.Contains(declaration, StringComparison.Ordinal)).ToArray();

        if (matches.Length != 1)
            throw new InvalidDataException($"Expected exactly one declaration '{declaration}' in {relative}.");

        contents[relative] = text;
        return new JsonObject { ["path"] = relative, ["line"] = matches[0].index + 1 };
    }
}
