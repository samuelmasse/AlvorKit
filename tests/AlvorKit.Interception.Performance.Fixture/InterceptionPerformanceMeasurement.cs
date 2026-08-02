namespace AlvorKit.Interception.Performance.Fixture;

/// <summary>Warms and samples the caller without adding work inside its measured loop.</summary>
internal static class InterceptionPerformanceMeasurement
{
    /// <summary>Calls used to promote each caller state before it is sampled.</summary>
    internal const int TierWarmupIterations = 250_000;

    /// <summary>Calls included in each latency sample.</summary>
    internal const int TimedIterations = 1_000_000;

    /// <summary>Independent latency samples collected for each caller state.</summary>
    internal const int TimedSamples = 5;

    /// <summary>Calls included in the current-thread allocation assertion.</summary>
    internal const int AllocationIterations = 100_000;

    /// <summary>Collects median, minimum, maximum, and allocation evidence for the current route.</summary>
    internal static WarmInterceptionMeasurement MeasureWarm(string name)
    {
        var samples = new double[TimedSamples];
        for (var sample = 0; sample < samples.Length; sample++)
        {
            var elapsedTicks = MeasureCallerTicks();
            samples[sample] =
                elapsedTicks * 1_000_000_000d /
                Stopwatch.Frequency /
                TimedIterations;
        }

        Array.Sort(samples);
        return new(
            name,
            samples[samples.Length / 2],
            samples[0],
            samples[^1],
            MeasureCallerAllocations());
    }

    /// <summary>Executes enough calls for tiered compilation to settle before sampling.</summary>
    internal static void WarmCaller()
    {
        var checksum = default(AdditiveChecksum64);
        for (var index = 0; index < TierWarmupIterations; index++)
            checksum.Add(InterceptionPerformanceTarget.Caller(index));
        GC.KeepAlive(checksum.Value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureCallerTicks()
    {
        var checksum = default(AdditiveChecksum64);
        var started = Stopwatch.GetTimestamp();
        for (var index = 0; index < TimedIterations; index++)
            checksum.Add(InterceptionPerformanceTarget.Caller(index));
        var elapsed = Stopwatch.GetTimestamp() - started;
        GC.KeepAlive(checksum.Value);
        return elapsed;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureCallerAllocations()
    {
        var checksum = default(AdditiveChecksum64);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < AllocationIterations; index++)
            checksum.Add(InterceptionPerformanceTarget.Caller(index));
        var allocated =
            GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(checksum.Value);
        return allocated;
    }
}
