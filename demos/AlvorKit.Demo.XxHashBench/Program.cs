using System.Globalization;
using AlvorKit.Demo.XxHashBench;
using AlvorKit.XxHash;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var options = BenchOptions.Parse(args);
Xxh xxh = new XxhBackend();
var candidates = BenchReport.CreateCandidates(xxh);

BenchReport.PrintEnvironment(xxh);

var largeLogs = Enumerable
    .Range(options.LargeLogMin, Math.Max(0, options.LargeLogMax - options.LargeLogMin + 1))
    .ToArray();
var smallSizes = options.SmallSizes();

var bench = new HashBench();
var bandwidth = BenchReport.MeasureLargeBandwidth(bench, candidates, options, largeLogs);
BenchReport.MeasureSmallFixedThroughput(bench, candidates, options, smallSizes);
var smallVelocity = BenchReport.MeasureSmallRandomThroughput(bench, candidates, smallSizes);
BenchReport.MeasureSmallFixedLatency(bench, candidates, smallSizes);
BenchReport.MeasureSmallRandomLatency(bench, candidates, smallSizes);

BenchReport.PrintSummary(bench, candidates, largeLogs, bandwidth, smallVelocity);
return 0;

/// <summary>Benchmark table and summary helpers that support the top-level walkthrough above.</summary>
file static class BenchReport
{
    /// <summary>Total wall-clock budget, in milliseconds, for one large-input table cell.</summary>
    private const int LargeTotalMs = 1010;

    /// <summary>Target duration, in milliseconds, for one calibrated large-input measurement loop.</summary>
    private const int LargeIterMs = 490;

    /// <summary>Total wall-clock budget, in milliseconds, for one small-input table cell.</summary>
    private const int SmallTotalMs = 490;

    /// <summary>Target duration, in milliseconds, for one calibrated small-input measurement loop.</summary>
    private const int SmallIterMs = 170;

    /// <summary>The published bandwidth uses a ~100 KB input; 1&lt;&lt;17 = 131072 is the closest sweep point.</summary>
    private const int BandwidthSizeLog = 17;

    /// <summary>Builds the candidate list using the supplied xxHash binding instance.</summary>
    /// <param name="xxh">The binding instance that supplies each native hash call.</param>
    /// <returns>The hash functions and published comparison values in report order.</returns>
    public static HashCandidate[] CreateCandidates(Xxh xxh) =>
    [
        new("xxh3", (input, length) => xxh.Hash3To64(input, length), publishedGbps: 31.5, publishedVelocity: 133.1),
        new("XXH32", (input, length) => xxh.Hash32(input, length, 0), publishedGbps: 9.7, publishedVelocity: 71.9),
        new("XXH64", (input, length) => xxh.Hash64(input, length, 0), publishedGbps: 19.4, publishedVelocity: 71.0),
        new("XXH128", (input, length) => (ulong)xxh.Hash3To128(input, length), publishedGbps: 29.6, publishedVelocity: 118.1),
    ];

    /// <summary>Prints the runtime context needed to interpret benchmark numbers.</summary>
    /// <param name="xxh">The loaded xxHash binding used to query the native library version.</param>
    public static void PrintEnvironment(Xxh xxh)
    {
        Console.WriteLine("AlvorKit.Demo.XxHashBench - port of the xxHash tests/bench harness");
        Console.WriteLine($"xxhash {VersionString(xxh.GetVersionNumber())} via AlvorKit.XxHash.Backend, .NET {Environment.Version}");
        Console.WriteLine($"{RuntimeInformation.OSDescription}, {RuntimeInformation.ProcessArchitecture}, {Environment.ProcessorCount} logical cores");

        var cpu = CpuName();
        if (cpu is not null)
            Console.WriteLine(cpu);

        Console.WriteLine();
    }

    /// <summary>Measures fixed-size large inputs as MB/s and returns the printed values.</summary>
    /// <param name="bench">The upstream-style benchmark engine used for every timed measurement.</param>
    /// <param name="candidates">The hash functions to print as rows in the table.</param>
    /// <param name="options">The command line options that describe the large-input sweep range.</param>
    /// <param name="largeLogs">The log2 input sizes to measure.</param>
    /// <returns>A table keyed by candidate name, with MB/s values in sweep order.</returns>
    public static Dictionary<string, double[]> MeasureLargeBandwidth(
        HashBench bench,
        HashCandidate[] candidates,
        BenchOptions options,
        int[] largeLogs)
    {
        Console.WriteLine($"Large inputs, MB/s (throughput, fixed size, fastest run; {1 << options.LargeLogMin} B to {(1 << options.LargeLogMax) >> 10} KiB):");
        return Sweep(
            candidates,
            largeLogs,
            header: log => $"log{log}",
            measure: (hash, log) => bench.Run(hash, BenchMode.Throughput, 1 << log, SizeMode.Fixed, LargeTotalMs, LargeIterMs) * (1 << log) / 1e6,
            format: mbps => mbps.ToString("F0"));
    }

    /// <summary>Measures fixed-size small-input throughput as million hashes per second.</summary>
    /// <param name="bench">The upstream-style benchmark engine used for every timed measurement.</param>
    /// <param name="candidates">The hash functions to print as rows in the table.</param>
    /// <param name="options">The command line options that describe whether the small sweep is dense.</param>
    /// <param name="smallSizes">The small input sizes to measure.</param>
    public static void MeasureSmallFixedThroughput(
        HashBench bench,
        HashCandidate[] candidates,
        BenchOptions options,
        int[] smallSizes)
    {
        Console.WriteLine($"Small inputs, million hashes/s ({(options.Dense ? "every size" : "sampled sizes, --dense for every size")}):");
        Console.WriteLine("Throughput, fixed size:");
        Sweep(
            candidates,
            smallSizes,
            header: size => size.ToString(),
            measure: (hash, size) => bench.Run(hash, BenchMode.Throughput, size, SizeMode.Fixed, SmallTotalMs, SmallIterMs) / 1e6,
            format: mhps => mhps.ToString("F1"));
    }

    /// <summary>Measures random-size small-input throughput and returns the table used for the summary velocity row.</summary>
    /// <param name="bench">The upstream-style benchmark engine used for every timed measurement.</param>
    /// <param name="candidates">The hash functions to print as rows in the table.</param>
    /// <param name="smallSizes">The inclusive maximum sizes for each random length sweep point.</param>
    /// <returns>A table keyed by candidate name, with million-hashes-per-second values in sweep order.</returns>
    public static Dictionary<string, double[]> MeasureSmallRandomThroughput(
        HashBench bench,
        HashCandidate[] candidates,
        int[] smallSizes)
    {
        Console.WriteLine("Throughput, random size 1..N:");
        return Sweep(
            candidates,
            smallSizes,
            header: size => size.ToString(),
            measure: (hash, size) => bench.Run(hash, BenchMode.Throughput, size, SizeMode.Random, SmallTotalMs, SmallIterMs) / 1e6,
            format: mhps => mhps.ToString("F1"));
    }

    /// <summary>Measures fixed-size small-input latency as a serialized chain of hash calls.</summary>
    /// <param name="bench">The upstream-style benchmark engine used for every timed measurement.</param>
    /// <param name="candidates">The hash functions to print as rows in the table.</param>
    /// <param name="smallSizes">The small input sizes to measure.</param>
    public static void MeasureSmallFixedLatency(HashBench bench, HashCandidate[] candidates, int[] smallSizes)
    {
        Console.WriteLine("Latency, fixed size:");
        Sweep(
            candidates,
            smallSizes,
            header: size => size.ToString(),
            measure: (hash, size) => bench.Run(hash, BenchMode.Latency, size, SizeMode.Fixed, SmallTotalMs, SmallIterMs) / 1e6,
            format: mhps => mhps.ToString("F1"));
    }

    /// <summary>Measures random-size small-input latency as a serialized chain of hash calls.</summary>
    /// <param name="bench">The upstream-style benchmark engine used for every timed measurement.</param>
    /// <param name="candidates">The hash functions to print as rows in the table.</param>
    /// <param name="smallSizes">The inclusive maximum sizes for each random length sweep point.</param>
    public static void MeasureSmallRandomLatency(HashBench bench, HashCandidate[] candidates, int[] smallSizes)
    {
        Console.WriteLine("Latency, random size 1..N:");
        Sweep(
            candidates,
            smallSizes,
            header: size => size.ToString(),
            measure: (hash, size) => bench.Run(hash, BenchMode.Latency, size, SizeMode.Random, SmallTotalMs, SmallIterMs) / 1e6,
            format: mhps => mhps.ToString("F1"));
    }

    /// <summary>Measures one row per candidate over <paramref name="points"/> and prints values immediately.</summary>
    /// <typeparam name="T">The sweep point type, usually an input size or log2 input size.</typeparam>
    /// <param name="candidates">The hash functions to print as rows in the table.</param>
    /// <param name="points">The ordered sweep points to print as columns.</param>
    /// <param name="header">Formats a sweep point for the column header.</param>
    /// <param name="measure">Measures one hash candidate at one sweep point.</param>
    /// <param name="format">Formats a measured value for the table cell.</param>
    /// <returns>A table keyed by candidate name, with values in sweep order.</returns>
    private static Dictionary<string, double[]> Sweep<T>(
        HashCandidate[] candidates,
        T[] points,
        Func<T, string> header,
        Func<HashFn, T, double> measure,
        Func<double, string> format)
    {
        Console.Write($"{"",-7}");
        foreach (var point in points)
            Console.Write($"{header(point),9}");
        Console.WriteLine();

        var results = new Dictionary<string, double[]>();
        foreach (var candidate in candidates)
        {
            Console.Write($"{candidate.Name,-7}");
            var values = new double[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                values[i] = measure(candidate.Hash, points[i]);
                Console.Write($"{format(values[i]),9}");
                Console.Out.Flush();
            }

            Console.WriteLine();
            results[candidate.Name] = values;
        }

        Console.WriteLine();
        return results;
    }

    /// <summary>Writes the measured-vs-published summary and the interpretation notes for the binding demo.</summary>
    /// <param name="bench">The benchmark engine used for the fallback bandwidth measurement when log17 was not part of the sweep.</param>
    /// <param name="candidates">The hash candidates in display order.</param>
    /// <param name="largeLogs">The log2 input sizes printed in the large-input table.</param>
    /// <param name="bandwidth">The large-input table values, in MB/s, keyed by candidate name.</param>
    /// <param name="smallVelocity">The random-size small-input throughput table, in million hashes per second.</param>
    public static void PrintSummary(
        HashBench bench,
        HashCandidate[] candidates,
        int[] largeLogs,
        Dictionary<string, double[]> bandwidth,
        Dictionary<string, double[]> smallVelocity)
    {
        Console.WriteLine("Summary, this run vs the table published in the xxHash v0.8.3 README:");
        Console.WriteLine($"{"",-7}{"Bandwidth (GB/s)",25}{"",4}{"Small data velocity",25}");
        Console.WriteLine($"{"",-7}{"measured",10}{"published",10}{"ratio",9}{"measured",10}{"published",10}{"ratio",9}");

        foreach (var candidate in candidates)
            PrintCandidateSummary(bench, largeLogs, bandwidth, smallVelocity, candidate);

        Console.WriteLine();
        Console.WriteLine($"Bandwidth is the throughput at {1 << BandwidthSizeLog} bytes, matching the ~100 KB input of the published table.");
        Console.WriteLine("Small data velocity has no published formula (\"a rough average of algorithm's efficiency on small data\");");
        Console.WriteLine("here it is the average of the random-size throughput row in million hashes/s, which is in the same unit family.");
        Console.WriteLine("The published table was measured on an Intel i7-9700K with clang v10 -O3 on bare C; this run goes through the");
        Console.WriteLine("AlvorKit P/Invoke binding into an MSVC /O2 DLL (SSE2 paths, like the published \"(SSE2)\" rows), so the per-call");
        Console.WriteLine("overhead mostly shows on small inputs while large-input bandwidth is hardware-bound. Compare ratios, not absolutes.");
    }

    /// <summary>Prints one summary row for <paramref name="candidate"/>.</summary>
    /// <param name="bench">The benchmark engine used only when the bandwidth sweep omitted the published comparison size.</param>
    /// <param name="largeLogs">The log2 input sizes printed in the large-input table.</param>
    /// <param name="bandwidth">The large-input table values, in MB/s, keyed by candidate name.</param>
    /// <param name="smallVelocity">The random-size small-input throughput table, in million hashes per second.</param>
    /// <param name="candidate">The hash candidate to summarize.</param>
    private static void PrintCandidateSummary(
        HashBench bench,
        int[] largeLogs,
        Dictionary<string, double[]> bandwidth,
        Dictionary<string, double[]> smallVelocity,
        HashCandidate candidate)
    {
        var measuredGbps = MeasurePublishedBandwidthGbps(bench, largeLogs, bandwidth, candidate);
        var measuredVelocity = smallVelocity[candidate.Name].Average();

        Console.WriteLine(
            $"{candidate.Name,-7}{measuredGbps,10:F1}{candidate.PublishedGbps,10:F1}{measuredGbps / candidate.PublishedGbps,9:F2}" +
            $"{measuredVelocity,10:F1}{candidate.PublishedVelocity,10:F1}{measuredVelocity / candidate.PublishedVelocity,9:F2}");
    }

    /// <summary>Returns the GB/s value that corresponds to the published table's ~100 KB bandwidth input.</summary>
    /// <param name="bench">The benchmark engine used only when the sweep omitted the published comparison size.</param>
    /// <param name="largeLogs">The log2 input sizes printed in the large-input table.</param>
    /// <param name="bandwidth">The large-input table values, in MB/s, keyed by candidate name.</param>
    /// <param name="candidate">The hash candidate to read or measure.</param>
    /// <returns>The measured bandwidth in GB/s.</returns>
    private static double MeasurePublishedBandwidthGbps(
        HashBench bench,
        int[] largeLogs,
        Dictionary<string, double[]> bandwidth,
        HashCandidate candidate)
    {
        var bandwidthIndex = Array.IndexOf(largeLogs, BandwidthSizeLog);
        if (bandwidthIndex >= 0)
            return bandwidth[candidate.Name][bandwidthIndex] / 1000;

        return bench.Run(
            candidate.Hash,
            BenchMode.Throughput,
            1 << BandwidthSizeLog,
            SizeMode.Fixed,
            LargeTotalMs,
            LargeIterMs) * (1 << BandwidthSizeLog) / 1e9;
    }

    /// <summary>Formats the integer version used by xxHash as a dotted semantic version.</summary>
    /// <param name="versionNumber">The native xxHash version number, encoded as major/minor/patch decimal groups.</param>
    /// <returns>A dotted version string such as <c>0.8.3</c>.</returns>
    private static string VersionString(uint versionNumber) =>
        $"{versionNumber / 10000}.{versionNumber / 100 % 100}.{versionNumber % 100}";

    /// <summary>Returns a platform-specific CPU description when one is cheaply available.</summary>
    /// <returns>The CPU description, or <see langword="null"/> when this platform has no simple source.</returns>
    private static string? CpuName()
    {
        if (OperatingSystem.IsWindows())
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");

        if (OperatingSystem.IsLinux() && File.Exists("/proc/cpuinfo"))
        {
            var modelLine = File.ReadLines("/proc/cpuinfo").FirstOrDefault(line => line.StartsWith("model name", StringComparison.Ordinal));
            return modelLine?.Split(':', 2)[^1].Trim();
        }

        return null;
    }
}

/// <summary>
/// Describes one benchmarked hash function and its published xxHash v0.8.3
/// README comparison row.
/// </summary>
/// <param name="name">The display name used by the upstream benchmark table.</param>
/// <param name="hash">The bound hash function to invoke during timed sweeps.</param>
/// <param name="publishedGbps">The published large-input bandwidth in GB/s.</param>
/// <param name="publishedVelocity">The published small-data velocity score.</param>
file sealed class HashCandidate(
    string name,
    HashFn hash,
    double publishedGbps,
    double publishedVelocity)
{
    /// <summary>The display name used by the upstream benchmark table.</summary>
    public string Name { get; } = name;

    /// <summary>The bound hash function to invoke during timed sweeps.</summary>
    public HashFn Hash { get; } = hash;

    /// <summary>The published large-input bandwidth in GB/s.</summary>
    public double PublishedGbps { get; } = publishedGbps;

    /// <summary>The published small-data velocity score.</summary>
    public double PublishedVelocity { get; } = publishedVelocity;
}
