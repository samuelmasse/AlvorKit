namespace AlvorKit;

/// <summary>The original creation and update kernels, with explicit measurement contracts.</summary>
internal static class ComparisonExperiments
{
    internal static ComparisonExperiment[] All => [.. Kernels()];

    private static IEnumerable<ComparisonExperiment> Kernels()
    {
        const int count = 100000;

        foreach (var entry in ComparisonCatalog.Cases)
        {
            int[] paddings = entry.Scenario.StartsWith("Update") ? [0, 10] : [0];

            foreach (var padding in paddings)
            {
                var creation = entry.Scenario.StartsWith("Create");
                var input = new ComparisonInput(count, creation ? 1 : 64, padding);
                var description = creation ? "Create zero-valued components" : "Repeated integer arithmetic";
                var timing = creation
                    ? "World construction excluded; registration, reservation, growth and required submission included"
                    : "Prepared fixture; handle selection and retained-query construction excluded; recorded passes";
                yield return new(entry.Id(count, padding), entry.Scenario, entry.Framework, entry.Storage,
                    entry.Variant, entry.Mode, input, "Ent", count * input.Passes, description, timing,
                    entry.Representative, entry.Workload, entry.Measure.Method, () => entry.Measure(count, padding, input.Passes))
                {
                    Parameterized = creation ? null : value => entry.Measure(count, padding, value.Passes),
                };
            }
        }
    }
}
