namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>Prints human-readable measurements and optionally writes the complete JSON artifact.</summary>
internal static class MockPerformanceOutput
{
    /// <summary>Prints environment, method, measurements, and interpretation boundaries.</summary>
    internal static void Print(MockPerformanceReport report)
    {
        var environment = report.Environment;
        var options = report.Options;

        Console.WriteLine("AlvorKit mocking performance fixture");
        Console.WriteLine($"Recorded UTC: {environment.RecordedAtUtc:O}");
        Console.WriteLine($"OS: {environment.OperatingSystem}");
        Console.WriteLine($"Processor: {environment.Processor}");
        Console.WriteLine(
            $"Process: {environment.ProcessArchitecture}, " +
            $"{environment.LogicalProcessorCount} logical processors");
        Console.WriteLine($"Runtime: {environment.Framework}");
        Console.WriteLine(
            $"Build: {environment.BuildConfiguration}, " +
            $"GC: {environment.GarbageCollector}");
        Console.WriteLine(
            $"Runs: {options.Runs}, warmups: {options.Warmups}, " +
            $"workers: {options.Workers}");
        Console.WriteLine();
        Console.WriteLine(
            $"{"Case",-43} {"unit",-15} {"median ns",12} " +
            $"{"min ns",12} {"max ns",12} {"spread",12} {"median B",12}");

        foreach (var result in report.Results)
        {
            var allocated = result.MedianAllocatedBytesPerOperation is double bytes
                ? bytes.ToString("n2")
                : "n/a";
            Console.WriteLine(
                $"{result.Name,-43} {result.Unit,-15} " +
                $"{result.MedianNanosecondsPerOperation,12:n2} " +
                $"{result.MinimumNanosecondsPerOperation,12:n2} " +
                $"{result.MaximumNanosecondsPerOperation,12:n2} " +
                $"{result.SpreadNanosecondsPerOperation,12:n2} " +
                $"{allocated,12}");
        }

        Console.WriteLine();
        Console.WriteLine("Notes:");
        foreach (var result in report.Results)
            Console.WriteLine($"- {result.Name}: {result.Notes}");

        Console.WriteLine();
        Console.WriteLine("Measurement boundaries:");
        foreach (var boundary in report.MeasurementBoundaries)
            Console.WriteLine($"- {boundary}");
    }

    /// <summary>Writes the structured artifact when an output path was supplied.</summary>
    internal static void WriteJson(MockPerformanceReport report)
    {
        if (report.Options.OutputPath is null)
            return;

        var fullPath = Path.GetFullPath(report.Options.OutputPath);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(
            report,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fullPath, json);
        Console.WriteLine();
        Console.WriteLine($"JSON: {fullPath}");
    }
}
