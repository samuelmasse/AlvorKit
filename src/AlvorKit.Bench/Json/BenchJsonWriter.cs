namespace AlvorKit;

/// <summary>Exports complete measurements, selection, and environment metadata as versioned JSON.</summary>
[Bench]
public class BenchJsonWriter(BenchConsole output, BenchAllocationTracking allocationTracking)
{
    /// <summary>Schema identifies separate whole-measurement and timed-workload allocation fields.</summary>
    private const int SchemaVersion = 9;

    /// <summary>Shared deterministic JSON naming and indentation settings.</summary>
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>Writes JSON to the requested file or standard output and returns the resolved file path.</summary>
    public string? Write(string path, BenchCatalog catalog, BenchCommand command, BenchExecution execution)
    {
        var document = CreateDocument(catalog, command, execution);
        var json = JsonSerializer.Serialize(document, SerializerOptions);

        if (path == "-")
        {
            output.WriteJson(json);
            return null;
        }

        var fullPath = Path.GetFullPath(path);
        var directoryPath = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directoryPath);
        File.WriteAllText(fullPath, json + Environment.NewLine);
        return fullPath;
    }

    /// <summary>Combines run provenance with all measured case results.</summary>
    private BenchJsonDocument CreateDocument(
        BenchCatalog catalog,
        BenchCommand command,
        BenchExecution execution) => new(
        SchemaVersion,
        catalog.SuiteName,
        execution.StartedAtUtc,
        execution.Duration.TotalMilliseconds,
        allocationTracking.Enabled,
        new(command.IncludePatterns, command.ExcludePatterns, command.WarmupCount, command.SampleCount),
        CreateEnvironment(),
        [.. execution.Benchmarks.Select(CreateBenchmark)]);

    /// <summary>Captures runtime facts needed when comparing saved runs.</summary>
    private BenchJsonEnvironment CreateEnvironment() => new(
        RuntimeInformation.FrameworkDescription,
        RuntimeInformation.OSDescription,
        RuntimeInformation.ProcessArchitecture.ToString(),
        Environment.ProcessorCount,
        GCSettings.IsServerGC);

    /// <summary>Preserves warmups and retained samples as separate arrays.</summary>
    private BenchJsonBenchmark CreateBenchmark(BenchCaseRun run) => new(
        run.Benchmark.Id,
        run.Benchmark.Description,
        run.Measurement.Summary.Unit,
        CreateComparison(run.Benchmark),
        CreateSamples(run.Measurement.Warmups, 1),
        CreateSamples(run.Measurement.Samples, run.Measurement.Warmups.Length + 1),
        CreateSummary(run.Measurement));

    /// <summary>Describes comparisons and leaves standalone measurements ungrouped.</summary>
    private BenchJsonComparison? CreateComparison(BenchCase benchmark) => benchmark.Role switch
    {
        BenchCaseRole.Baseline => new("baseline", null),
        BenchCaseRole.Candidate => new("candidate", benchmark.BaselineId),
        _ => null,
    };

    /// <summary>Numbers retained samples after the preceding warmup sequence.</summary>
    private BenchJsonSample[] CreateSamples(BenchResult[] results, int firstRunNumber)
    {
        var samples = new BenchJsonSample[results.Length];

        for (var index = 0; index < results.Length; index++)
        {
            var result = results[index];
            samples[index] = new(
                firstRunNumber + index,
                result.Elapsed.TotalNanoseconds,
                result.OperationCount,
                result.OperationsPerSecond,
                result.UnmanagedMemory,
                new(result.AllocatedBytes, result.AllocatedObjects, result.WorkloadAllocatedBytes));
        }

        return samples;
    }

    /// <summary>Exports retained means; negative baseline percentages indicate less elapsed time.</summary>
    private BenchJsonSummary CreateSummary(BenchMeasurementRun measurement)
    {
        var result = measurement.Summary;
        return new(
            result.Elapsed.TotalNanoseconds,
            result.OperationCount,
            result.OperationsPerSecond,
            measurement.BaselineMultiplier,
            measurement.BaselineMultiplier.HasValue ? (measurement.BaselineMultiplier.Value - 1) * 100 : null,
            result.UnmanagedMemory,
            new(measurement.Samples.Average(static sample => sample.AllocatedBytes),
                allocationTracking.Enabled
                ? measurement.Samples.Average(static sample => (double)sample.AllocatedObjects!.Value)
                : null,
                measurement.Samples.Average(static sample => (double?)sample.WorkloadAllocatedBytes)));
    }
}
