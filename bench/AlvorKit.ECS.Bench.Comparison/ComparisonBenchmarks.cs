namespace AlvorKit;

[Bench]
public class ComparisonBenchmarks : IBenchSuiteProvider
{
    public BenchSuite Create()
    {
        const int count = 100000;
        var groups = ComparisonCatalog.Cases.GroupBy(entry => entry.Scenario);
        return new("AlvorKit.ECS.Bench.Comparison", [.. groups.Select(group => CreateScenario(group.Key, [.. group], count))]);
    }

    private static BenchNode CreateScenario(string scenario, ComparisonCase[] cases, int count)
    {
        var description = scenario switch
        {
            "Create1" => "Create one zero-initialized component; submission and storage growth are timed",
            "Create2" => "Create two zero-initialized components; submission and storage growth are timed",
            "Create3" => "Create three zero-initialized components; submission and storage growth are timed",
            "Update1" => "Increment first component; 64 passes; fixture preparation excluded",
            "Update2" => "Add second to first; 64 passes; fixture preparation excluded",
            "Update3" => "Add second and third to first; 64 passes; fixture preparation excluded",
            "Mixed" => "Add second to first across four signatures; 64 passes; fixture preparation excluded",
            _ => throw new ArgumentException($"Unknown comparison scenario: {scenario}"),
        };
        int[] paddings = scenario.StartsWith("Update") ? [0, 10] : [0];
        var inputs = paddings.Select(padding => CreatePadding(cases, count, padding)).ToArray();
        return BenchNode.Group(scenario, description, [BenchNode.Group($"N{count}", $"{count:N0} matching Ents", inputs)]);
    }

    private static BenchNode CreatePadding(ComparisonCase[] cases, int count, int padding)
    {
        var modes = cases.GroupBy(entry => entry.Mode).Select(mode =>
            BenchNode.Group(mode.Key, "", [.. mode.GroupBy(entry => entry.Framework).Select(framework =>
                BenchNode.Group(framework.Key, "", [.. framework.Select(entry =>
                    BenchNode.Measure(entry.Variant, "", () => entry.Measure(count, padding)))]))])).ToArray();
        return BenchNode.Group($"Padding{padding}", $"{padding} nonmatching Ents per matching Ent", modes);
    }
}
