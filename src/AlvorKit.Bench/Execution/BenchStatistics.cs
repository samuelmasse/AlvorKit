namespace AlvorKit;

/// <summary>Statistics over independent process estimates; inner-loop operations are never independent samples.</summary>
public static class BenchStatistics
{
    /// <summary>Recomputes a JSON summary from all retained samples after independent launches have been combined.</summary>
    public static BenchJsonSummary SummarizeSamples(BenchJsonSample[] samples)
    {
        if (samples.Length == 0 || samples.Any(sample => sample.OperationCount <= 0 ||
            sample.OperationCount != samples[0].OperationCount || !double.IsFinite(sample.ElapsedNanoseconds) ||
            sample.ElapsedNanoseconds <= 0))
            throw new ArgumentException("Samples must be nonempty with positive timings and identical operation counts.");

        var elapsed = samples.Average(sample => sample.ElapsedNanoseconds);
        var memory = samples.Where(sample => sample.UnmanagedMemory != null).Select(sample => sample.UnmanagedMemory!).ToArray();

        if (memory.Length != 0 && memory.Length != samples.Length)
            throw new ArgumentException("Every sample must use the same native-memory diagnostic scope.");

        return new(elapsed, samples[0].OperationCount, samples[0].OperationCount * 1e9 / elapsed, null, null,
            memory.Length == 0 ? null : BenchMemoryUsage.Mean(memory),
            new(samples.Average(sample => sample.Allocations.MeasurementBytes),
                MeanOptional([.. samples.Select(sample => sample.Allocations.MeasurementObjects)]),
                MeanOptional([.. samples.Select(sample => sample.Allocations.WorkloadBytes)])));
    }

    /// <summary>Computes a t interval over launch means. One launch has no estimable between-process uncertainty.</summary>
    public static BenchEstimate Estimate(ReadOnlySpan<double> launches)
    {
        if (launches.IsEmpty)
            throw new ArgumentException("At least one launch is required.");

        double sum = 0;

        foreach (var value in launches)
        {
            if (!double.IsFinite(value) || value < 0)
                throw new ArgumentException("Launch estimates must be finite and nonnegative.");

            sum += value;
        }

        var mean = sum / launches.Length;

        if (launches.Length == 1)
            return new(1, mean, 0, null, null);

        double squared = 0;

        foreach (var value in launches)
            squared += (value - mean) * (value - mean);

        var deviation = Math.Sqrt(squared / (launches.Length - 1));
        var margin = Critical95(launches.Length - 1) * deviation / Math.Sqrt(launches.Length);
        return new(launches.Length, mean, deviation, Math.Max(0, mean - margin), mean + margin);
    }

    /// <summary>Conservative critical values rounded upward, including grouped larger degrees of freedom.</summary>
    private static double Critical95(int degrees) => degrees switch
    {
        1 => 12.707, 2 => 4.303, 3 => 3.183, 4 => 2.777, 5 => 2.571,
        6 => 2.447, 7 => 2.365, 8 => 2.307, 9 => 2.263, 10 => 2.229,
        <= 15 => 2.202, <= 20 => 2.132, <= 30 => 2.080, <= 60 => 2.043,
        _ => 2.001,
    };

    private static double? MeanOptional(double?[] values)
    {
        if (values.All(value => value == null))
            return null;

        if (values.Any(value => value == null))
            throw new ArgumentException("Every sample must use the same allocation diagnostic scope.");

        return values.Average(value => value!.Value);
    }
}
