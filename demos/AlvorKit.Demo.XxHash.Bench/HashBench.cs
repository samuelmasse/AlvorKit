namespace AlvorKit.Demo.XxHash.Bench;

/// <summary>
/// Port of the upstream xxHash benchmark core (tests/bench/benchHash.c and
/// benchfn.c): hash one input repeatedly, with an adaptive loop count per
/// measurement, and report the fastest observed run. One run hashes a batch of
/// blocks sized so roughly 200 KB are hashed per run.
/// </summary>
internal sealed unsafe class HashBench
{
    /// <summary>Extra bytes that let the latency benchmark vary its input pointer without reading past the buffer.</summary>
    private const int MarginForLatency = 1024;

    /// <summary>Mask used to keep the latency-chain pointer offset inside <see cref="MarginForLatency"/>.</summary>
    private const int StartMask = MarginForLatency - 1;

    /// <summary>Approximate input bytes hashed by one measured run before loop-count calibration.</summary>
    private const int SizeToHashPerRound = 200_000;

    /// <summary>Maximum number of hash calls included in one measured run.</summary>
    private const int NbHashRoundsMax = 1000;

    /// <summary>Conversion factor from stopwatch ticks to nanoseconds.</summary>
    private static readonly double NsPerTick = 1e9 / Stopwatch.Frequency;

    /// <summary>Accumulator carried between latency measurements to preserve the upstream dependency chain shape.</summary>
    private ulong latencyChain;

    /// <summary>Stores hash results so the measured loops keep observable work outside the method body.</summary>
    private ulong sink;

    /// <summary>Benchmarks <paramref name="hashFn"/> and returns the number of hashes per second of the fastest run.</summary>
    public double Run(HashFn hashFn, BenchMode benchMode, int size, SizeMode sizeMode, int totalTimeMs, int iterTimeMs)
    {
        var nbBlocks = Math.Min(SizeToHashPerRound / size + 1, NbHashRoundsMax);

        // The same random length sequence is reused by every run, seeded per
        // size like upstream's srand(s) (the generator differs from C rand()).
        var lengths = new nuint[nbBlocks];
        var random = new Random(size);
        for (var n = 0; n < nbBlocks; n++)
            lengths[n] = (nuint)(sizeMode == SizeMode.Fixed ? size : random.Next(1, size + 1));

        var bufferSize = (nuint)size + MarginForLatency;
        var buffer = (nint)NativeMemory.Alloc(bufferSize);
        try
        {
            InitBuffer(buffer, bufferSize);
            return 1e9 / MeasureFastestRunNs(hashFn, benchMode, buffer, lengths, totalTimeMs, iterTimeMs) * nbBlocks;
        }
        finally
        {
            NativeMemory.Free((void*)buffer);
        }
    }

    /// <summary>
    /// The timed loop of benchfn.c: calibrate the loop count so a measurement
    /// lasts about <paramref name="iterTimeMs"/>, keep measuring until
    /// <paramref name="totalTimeMs"/> is spent, and return the fastest
    /// nanoseconds-per-run observed.
    /// </summary>
    private double MeasureFastestRunNs(
        HashFn hashFn,
        BenchMode benchMode,
        nint buffer,
        ReadOnlySpan<nuint> lengths,
        int totalTimeMs,
        int iterTimeMs)
    {
        var runBudgetNs = iterTimeMs * 1e6;
        var totalBudgetNs = totalTimeMs * 1e6;
        var fastestNsPerRun = double.MaxValue;
        var spentNs = 0.0;
        var nbLoops = 1L;

        while (spentNs < totalBudgetNs)
        {
            var started = Stopwatch.GetTimestamp();
            for (var loop = 0L; loop < nbLoops; loop++)
            {
                if (benchMode == BenchMode.Throughput)
                    ThroughputRun(hashFn, buffer, lengths);
                else
                    LatencyRun(hashFn, buffer, lengths);
            }
            var elapsedNs = (Stopwatch.GetTimestamp() - started) * NsPerTick;

            spentNs += elapsedNs;
            fastestNsPerRun = Math.Min(fastestNsPerRun, elapsedNs / nbLoops);
            nbLoops = elapsedNs > runBudgetNs / 50
                ? (long)(runBudgetNs / fastestNsPerRun) + 1
                : nbLoops * 10;
        }

        return fastestNsPerRun;
    }

    /// <summary>Runs independent hash calls for the throughput benchmark mode.</summary>
    private void ThroughputRun(HashFn hashFn, nint buffer, ReadOnlySpan<nuint> lengths)
    {
        var acc = 0ul;
        for (var index = 0; index < lengths.Length; index++)
            acc ^= hashFn(buffer, lengths[index]);
        sink = acc;
    }

    /// <summary>Runs a serialized pointer dependency chain for the latency benchmark mode.</summary>
    private void LatencyRun(HashFn hashFn, nint buffer, ReadOnlySpan<nuint> lengths)
    {
        // The chain survives across runs, like the static accumulator in
        // upstream's benchLatency.
        var chain = latencyChain;
        for (var index = 0; index < lengths.Length; index++)
            chain = hashFn(buffer + (nint)(chain & StartMask), lengths[index]);
        latencyChain = chain;
        sink = chain;
    }

    /// <summary>The deterministic buffer fill of benchHash.c.</summary>
    private static void InitBuffer(nint buffer, nuint size)
    {
        const ulong k1 = 11400714785074694791ul;
        var acc = 14029467366897019727ul;
        var bytes = (byte*)buffer;
        for (nuint i = 0; i < size; i++)
        {
            acc *= k1;
            bytes[i] = (byte)(acc >> 56);
        }
    }
}

/// <summary>A single hash invocation over <paramref name="input"/>, returning the (truncated) hash value.</summary>
internal delegate ulong HashFn(nint input, nuint length);

/// <summary>The dependency pattern used between repeated hash calls in a benchmark cell.</summary>
internal enum BenchMode
{
    /// <summary>Independent back-to-back hash calls.</summary>
    Throughput,

    /// <summary>Each call's input pointer depends on the previous hash value, serializing the chain.</summary>
    Latency
}

/// <summary>The input length policy used for each block in a benchmark cell.</summary>
internal enum SizeMode
{
    /// <summary>Hash exactly Size bytes every call.</summary>
    Fixed,

    /// <summary>Hash a per-block random number of bytes between 1 and Size (inclusive).</summary>
    Random
}
