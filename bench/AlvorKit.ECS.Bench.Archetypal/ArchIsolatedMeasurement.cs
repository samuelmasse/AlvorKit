namespace AlvorKit;

internal static class ArchIsolatedMeasurement
{
    internal static BenchResult Run(string name)
    {
        var start = new ProcessStartInfo(Environment.ProcessPath!)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        if (Path.GetFileNameWithoutExtension(start.FileName).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(ArchIsolatedMeasurement).Assembly.Location);
        start.ArgumentList.Add("--isolated-sample");
        start.ArgumentList.Add(name);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(120000))
        {
            process.Kill(true);
            throw new TimeoutException($"Isolated benchmark {name} exceeded two minutes.");
        }

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Isolated benchmark {name} failed: {error.GetAwaiter().GetResult()}");
        return JsonSerializer.Deserialize<BenchResult>(output.GetAwaiter().GetResult());
    }

    internal static int Sample(string name)
    {
        // JIT the reference-type generic path without populating RunArch's static catalog.
        Measure<WarmArch>(name);
        var result = Measure<RunArch>(name);
        Console.WriteLine(JsonSerializer.Serialize(result));
        return 0;
    }

    private static BenchResult Measure<A>(string name) => name switch
    {
        "AddGrowth" => ArchStructuralMeasurements.AddGrowth<A>(4096),
        "AddUnknown" => ArchStructuralMeasurements.AddUnknown<A>(2048),
        "RemoveUnknown" => ArchStructuralMeasurements.RemoveUnknown<A>(2048),
        "UniqueSignature" => ArchStructuralMeasurements.UniqueSignature<A>(2048),
        "LowOccupancy" => ArchStructuralMeasurements.LowOccupancy<A>(2048),
        "HighOccupancy" => ArchStructuralMeasurements.HighOccupancy<A>(4096),
        "ConcurrentResolve" => ArchConcurrencyMeasurements.Resolve<A>(1024, 4),
        _ => throw new ArgumentException($"Unknown isolated benchmark '{name}'."),
    };
}
