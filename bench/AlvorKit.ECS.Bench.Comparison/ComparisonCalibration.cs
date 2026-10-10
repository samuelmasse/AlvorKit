namespace AlvorKit;

/// <summary>Uses discarded pilot processes to choose one pass count for every comparable implementation.</summary>
internal static class ComparisonCalibration
{
    internal static ComparisonExperiment[] Prepare(
        ComparisonExperiment[] selected, string directory, double targetMilliseconds, int warmup, JsonArray pilots)
    {
        if (!double.IsFinite(targetMilliseconds) || targetMilliseconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetMilliseconds));

        var runner = new BenchProcessRunner();
        var durations = new Dictionary<string, double>();

        for (var index = 0; index < selected.Length; index++)
        {
            var experiment = selected[index];
            var path = Path.Combine(directory, $"pilot-{index:D5}.json");
            Console.WriteLine($"Pilot [{index + 1}/{selected.Length}]: {experiment.Id}");
            var result = runner.Run(typeof(ComparisonCalibration).Assembly.Location,
                ["worker", experiment.Id, warmup.ToString(), "3", path, experiment.Input.Passes.ToString()],
                TimeSpan.FromMinutes(5));
            File.WriteAllText(Path.ChangeExtension(path, ".log"), result.Output + result.Errors);

            if (result.ExitCode != 0)
                throw new IOException($"Pilot failed for {experiment.Id}; see {Path.ChangeExtension(path, ".log")}");

            var document = JsonNode.Parse(File.ReadAllText(path))!;
            var values = document["benchmarks"]![0]!["samples"]!.AsArray()
                .Select(sample => (double)sample!["elapsedNanoseconds"]! / 1e6).Order().ToArray();
            durations.Add(experiment.Id, values[values.Length / 2]);
        }

        List<ComparisonExperiment> calibrated = [];

        foreach (var group in selected.GroupBy(entry => (entry.Scenario, entry.Mode, entry.Input)))
        {
            var fastest = group.Min(entry => durations[entry.Id]);
            var slowest = group.Max(entry => durations[entry.Id]);
            var capacity = group.Min(entry => int.MaxValue / entry.Operations);
            var multiplier = Multiplier(fastest, slowest, targetMilliseconds, capacity);

            foreach (var experiment in group)
            {
                var chosen = experiment.Parameterized == null ? experiment : experiment.WithPasses(experiment.Input.Passes * multiplier);
                calibrated.Add(chosen);
                pilots.Add(new JsonObject
                {
                    ["case"] = experiment.Id, ["pilotMedianMilliseconds"] = durations[experiment.Id],
                    ["originalPasses"] = experiment.Input.Passes, ["chosenPasses"] = chosen.Input.Passes,
                    ["targetMilliseconds"] = targetMilliseconds,
                    ["groupSlowestPilotMilliseconds"] = slowest,
                    ["maximumBatchMilliseconds"] = targetMilliseconds * 10,
                    ["fixedScale"] = experiment.Parameterized == null,
                });
            }
        }

        return [.. calibrated];
    }

    /// <summary>Bounds batching by the slowest strategy while preserving one shared operation count.</summary>
    internal static int Multiplier(double fastest, double slowest, double target, int capacity)
    {
        var desired = Math.Min(1024, Math.Ceiling(target / fastest));
        var durationLimit = Math.Max(1, Math.Floor(target * 10 / slowest));
        return Math.Min(capacity, Math.Max(1, (int)Math.Min(desired, durationLimit)));
    }
}
