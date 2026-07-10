namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Runs one archetypal sample in a fresh copy of the already-built benchmark assembly.</summary>
internal static class EcsBenchProcess
{
    internal static EcsArchBenchSample Run(EcsBenchOptions options, string scenarioId, int sampleIndex)
    {
        string outputPath = Path.Combine(Path.GetTempPath(), $"alvorkit-ecs-arch-{Guid.NewGuid():N}.json");
        var start = CreateStartInfo();
        Add(start, "--suite", EcsBenchOptions.ArchetypalSuite);
        Add(start, "--worker-case", scenarioId);
        Add(start, "--sample-index", sampleIndex.ToString(CultureInfo.InvariantCulture));
        Add(start, "--worker-json", outputPath);
        Add(start, "--operations", options.Operations.ToString(CultureInfo.InvariantCulture));
        Add(start, "--runs", "1");
        Add(start, "--warmups", options.Warmups.ToString(CultureInfo.InvariantCulture));
        Add(start, "--arches", options.Arches.ToString(CultureInfo.InvariantCulture));
        Add(start, "--rows", options.Rows.ToString(CultureInfo.InvariantCulture));
        Add(start, "--allocs", options.Allocs.ToString(CultureInfo.InvariantCulture));
        Add(start, "--label", options.Label);

        try
        {
            using var process = Process.Start(start) ?? throw new InvalidOperationException("Unable to start isolated benchmark worker.");
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Archetypal worker '{scenarioId}' failed with exit code {process.ExitCode}.\n{stdout}\n{stderr}");
            }

            string json = File.ReadAllText(outputPath);
            return JsonSerializer.Deserialize<EcsArchBenchSample>(json)
                ?? throw new InvalidOperationException($"Archetypal worker '{scenarioId}' wrote an empty sample.");
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    private static ProcessStartInfo CreateStartInfo()
    {
        string assemblyPath = Assembly.GetExecutingAssembly().Location;
        string processPath = Environment.ProcessPath ?? throw new InvalidOperationException("The current process path is unavailable.");
        var start = new ProcessStartInfo
        {
            FileName = processPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        if (Path.GetFileNameWithoutExtension(processPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(assemblyPath);

        return start;
    }

    private static void Add(ProcessStartInfo start, string option, string value)
    {
        start.ArgumentList.Add(option);
        start.ArgumentList.Add(value);
    }
}
