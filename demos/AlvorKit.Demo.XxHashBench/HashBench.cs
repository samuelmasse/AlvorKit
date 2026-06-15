namespace AlvorKit.Demo.XxHashBench;

/// <summary>
/// Port of the upstream xxHash benchmark core (tests/bench/benchHash.c and
/// benchfn.c): hash one input repeatedly, with an adaptive loop count per
/// measurement, and report the fastest observed run. One run hashes a batch of
/// blocks sized so roughly 200 KB are hashed per run.
/// </summary>
public sealed unsafe class HashBench
{
    private const int MarginForLatency = 1024;
    private const int StartMask = MarginForLatency - 1;
    private const int SizeToHashPerRound = 200_000;
    private const int NbHashRoundsMax = 1000;
    private static readonly double NsPerTick = 1e9 / System.Diagnostics.Stopwatch.Frequency;

    private ulong latencyChain;
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
        using var buffer = NativeBuffer.Allocate(bufferSize);

        InitBuffer(buffer.Pointer, bufferSize);
        return 1e9 / MeasureFastestRunNs(hashFn, benchMode, buffer.Pointer, lengths, totalTimeMs, iterTimeMs) * nbBlocks;
    }

    /// <summary>
    /// The timed loop of benchfn.c: calibrate the loop count so a measurement
    /// lasts about <paramref name="iterTimeMs"/>, keep measuring until
    /// <paramref name="totalTimeMs"/> is spent, and return the fastest
    /// nanoseconds-per-run observed.
    /// </summary>
    private double MeasureFastestRunNs(HashFn hashFn, BenchMode benchMode, nint buffer, nuint[] lengths, int totalTimeMs, int iterTimeMs)
    {
        var runBudgetNs = iterTimeMs * 1e6;
        var totalBudgetNs = totalTimeMs * 1e6;
        var fastestNsPerRun = double.MaxValue;
        var spentNs = 0.0;
        var nbLoops = 1L;

        while (spentNs < totalBudgetNs)
        {
            var started = System.Diagnostics.Stopwatch.GetTimestamp();
            for (var loop = 0L; loop < nbLoops; loop++)
            {
                if (benchMode == BenchMode.Throughput)
                    ThroughputRun(hashFn, buffer, lengths);
                else
                    LatencyRun(hashFn, buffer, lengths);
            }
            var elapsedNs = (System.Diagnostics.Stopwatch.GetTimestamp() - started) * NsPerTick;

            spentNs += elapsedNs;
            fastestNsPerRun = Math.Min(fastestNsPerRun, elapsedNs / nbLoops);
            nbLoops = elapsedNs > runBudgetNs / 50
                ? (long)(runBudgetNs / fastestNsPerRun) + 1
                : nbLoops * 10;
        }

        return fastestNsPerRun;
    }

    private void ThroughputRun(HashFn hashFn, nint buffer, nuint[] lengths)
    {
        var acc = 0ul;
        foreach (var length in lengths)
            acc ^= hashFn(buffer, length);
        sink = acc;
    }

    private void LatencyRun(HashFn hashFn, nint buffer, nuint[] lengths)
    {
        // The chain survives across runs, like the static accumulator in
        // upstream's benchLatency.
        var chain = latencyChain;
        foreach (var length in lengths)
            chain = hashFn(buffer + (nint)(chain & StartMask), length);
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

    /// <summary>Owns one native allocation for a benchmark cell and frees it when the measurement completes.</summary>
    /// <param name="pointer">The native pointer returned by <see cref="NativeMemory.AllocZeroed(nuint)"/>.</param>
    private readonly struct NativeBuffer(nint pointer) : IDisposable
    {
        /// <summary>The native pointer passed to xxHash during the timed loops.</summary>
        public nint Pointer { get; } = pointer;

        /// <summary>Allocates a native byte buffer without adding GC pressure to the measured path.</summary>
        /// <param name="size">The number of bytes to allocate.</param>
        /// <returns>An owned native buffer that must be disposed after the benchmark cell completes.</returns>
        public static NativeBuffer Allocate(nuint size) =>
            new((nint)NativeMemory.AllocZeroed(size));

        /// <summary>Frees the owned native allocation.</summary>
        public void Dispose() =>
            NativeMemory.Free((void*)Pointer);
    }
}

/// <summary>A single hash invocation over <paramref name="input"/>, returning the (truncated) hash value.</summary>
public delegate ulong HashFn(nint input, nuint length);

/// <summary>The dependency pattern used between repeated hash calls in a benchmark cell.</summary>
public enum BenchMode
{
    /// <summary>Independent back-to-back hash calls.</summary>
    Throughput,

    /// <summary>Each call's input pointer depends on the previous hash value, serializing the chain.</summary>
    Latency
}

/// <summary>The input length policy used for each block in a benchmark cell.</summary>
public enum SizeMode
{
    /// <summary>Hash exactly Size bytes every call.</summary>
    Fixed,

    /// <summary>Hash a per-block random number of bytes between 1 and Size (inclusive).</summary>
    Random
}
