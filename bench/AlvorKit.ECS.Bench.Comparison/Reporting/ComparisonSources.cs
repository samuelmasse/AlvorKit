namespace AlvorKit;

internal static class ComparisonSources
{
    private const string ProjectPath = "bench/AlvorKit.ECS.Bench.Comparison";

    internal static JsonObject Capture(JsonArray approaches)
    {
        var root = ProjectRoot.FindFromCurrentProcess(typeof(ComparisonSources), requireResDirectory: true);
        var sources = new JsonObject();

        foreach (var approach in approaches)
        {
            var entry = ComparisonCatalog.Cases.Single(item =>
                item.Scenario == (string)approach!["scenario"]! && item.Mode == (string)approach["mode"]! &&
                item.Framework == (string)approach["framework"]! && item.Variant == (string)approach["variant"]!);
            var method = entry.Measure.Method;
            sources.Add($"{entry.Scenario}/{entry.Mode}/{entry.Framework}/{entry.Variant}", new JsonObject
            {
                ["workload"] = Location(root,
                    $"{ProjectPath}/Workloads/{entry.Scenario}/{entry.Mode}/{entry.Workload.Name}.cs", " Run("),
                ["measurement"] = Location(root,
                    $"{ProjectPath}/Measurements/{method.DeclaringType!.Name}.cs", $" BenchResult {method.Name}("),
            });
        }
        return new JsonObject { ["root"] = root, ["approaches"] = sources };
    }

    private static JsonObject Location(string root, string path, string declaration)
    {
        var lines = File.ReadAllLines(Path.Combine(root, path));
        var matches = lines.Select((text, index) => (text, line: index + 1))
            .Where(item => item.text.Contains(declaration)).ToArray();

        if (matches.Length != 1)
            throw new InvalidDataException($"Expected one source declaration '{declaration}' in {path}.");
        return new JsonObject { ["path"] = path, ["line"] = matches[0].line };
    }
}
