namespace AlvorKit;

/// <summary>Samples a prepared measurement and averages retained timing, memory, and allocation results.</summary>
[Bench]
public class BenchMeasurementRunner(
    BenchCommand command,
    BenchConsole output,
    BenchAllocationTracking allocationTracking)
{
    /// <summary>Collects warmups and retained samples, then computes a retained-only mean.</summary>
    public BenchMeasurementRun Run(Func<BenchResult> measurement, int depth, TimeSpan? baseline)
    {
        var labelWidth = output.GetRunLabelWidth(command.WarmupCount + command.SampleCount);
        var warmups = RunWarmups(measurement, depth, labelWidth, out var transientLineLength);
        var samples = RunSamples(measurement, depth, labelWidth, ref transientLineLength);
        var summary = Mean(samples);
        double? multiplier = baseline.HasValue ? summary.Elapsed / baseline.Value : null;

        if (!command.Display.Quiet)
        {
            output.WriteMean(summary, labelWidth, depth, multiplier, transientLineLength);

            if (ShouldWriteMemoryTree(summary))
                output.WriteMemoryTree(summary.UnmanagedMemory!, depth + 1, true);
        }

        return new(warmups, samples, summary, multiplier);
    }

    /// <summary>Requires both the output option and a supplied native-memory snapshot.</summary>
    private bool ShouldWriteMemoryTree(BenchResult result) =>
        command.Display.ShowMemoryTree && result.UnmanagedMemory is not null;

    /// <summary>Runs preparation measurements and retains them separately from summary inputs.</summary>
    private BenchResult[] RunWarmups(
        Func<BenchResult> measurement,
        int depth,
        int labelWidth,
        out int transientLineLength)
    {
        var results = new BenchResult[command.WarmupCount];
        transientLineLength = 0;

        for (var warmupIndex = 0; warmupIndex < results.Length; warmupIndex++)
        {
            var result = Measure(measurement);
            results[warmupIndex] = result;

            if (!command.Display.Quiet)
            {
                transientLineLength = output.WriteTransient(
                    result,
                    warmupIndex + 1,
                    labelWidth,
                    depth,
                    transientLineLength);
            }
        }

        return results;
    }

    /// <summary>Collects the configured nonempty retained sample sequence.</summary>
    private BenchResult[] RunSamples(
        Func<BenchResult> measurement,
        int depth,
        int labelWidth,
        ref int transientLineLength)
    {
        var results = new BenchResult[command.SampleCount];

        for (var sampleIndex = 0; sampleIndex < results.Length; sampleIndex++)
        {
            var result = Measure(measurement);
            results[sampleIndex] = result;

            if (!command.Display.Quiet)
                WriteSampleProgress(result, sampleIndex, labelWidth, depth, ref transientLineLength);
        }

        return results;
    }

    /// <summary>Chooses permanent sample rows or interactive progress based on invocation options.</summary>
    private void WriteSampleProgress(
        BenchResult result,
        int sampleIndex,
        int labelWidth,
        int depth,
        ref int transientLineLength)
    {
        var runNumber = command.WarmupCount + sampleIndex + 1;

        if (command.Display.ShowSamples)
        {
            var previousLineLength = sampleIndex == 0 ? transientLineLength : 0;
            output.WriteRetained(result, runNumber, labelWidth, depth, previousLineLength);

            if (ShouldWriteMemoryTree(result))
            {
                output.WriteMemoryTree(result.UnmanagedMemory!, depth + 1, false);
                output.WriteBlankLine();
            }

            transientLineLength = 0;
            return;
        }

        transientLineLength = output.WriteTransient(result, runNumber, labelWidth, depth, transientLineLength);
    }

    /// <summary>Averages time and allocation totals across all retained samples.</summary>
    private BenchResult Mean(BenchResult[] samples)
    {
        var elapsedTicks = 0L;
        var tracksUnmanagedMemory = samples[0].UnmanagedMemory is not null;
        var memorySamples = tracksUnmanagedMemory ? new BenchMemoryUsage[samples.Length] : null;

        for (var index = 0; index < samples.Length; index++)
        {
            var sample = samples[index];
            elapsedTicks += sample.Elapsed.Ticks;

            if (tracksUnmanagedMemory)
                memorySamples![index] = sample.UnmanagedMemory!;
        }

        var meanElapsed = TimeSpan.FromTicks(elapsedTicks / samples.Length);
        var meanMemory = tracksUnmanagedMemory ? BenchMemoryUsage.Mean(memorySamples!) : null;
        return samples[^1] with
        {
            Elapsed = meanElapsed,
            UnmanagedMemory = meanMemory,
            AllocatedBytes = (long)samples.Average(static sample => sample.AllocatedBytes),
            AllocatedObjects = samples[0].AllocatedObjects.HasValue
                ? (ulong)samples.Average(static sample => (double)sample.AllocatedObjects!.Value) : null,
            WorkloadAllocatedBytes = samples[0].WorkloadAllocatedBytes.HasValue
                ? (long)samples.Average(static sample => sample.WorkloadAllocatedBytes!.Value) : null,
        };
    }

    /// <summary>Captures whole-measurement allocations, including fixture preparation and disposal.</summary>
    private BenchResult Measure(Func<BenchResult> measurement)
    {
        using var capture = allocationTracking.Begin();
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var result = measurement();
        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        var allocatedObjects = capture?.Complete();
        return result with { AllocatedBytes = allocatedBytes, AllocatedObjects = allocatedObjects };
    }
}
