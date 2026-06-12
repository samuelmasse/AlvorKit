using System.Globalization;
using AlvorKit.XxHash;

namespace AlvorKit.Demo.XxHashBench;

/// <summary>
/// Benchmarks the AlvorKit.XxHash binding with the methodology of the upstream
/// harness (tests/bench) and compares the results against the table published
/// in the xxHash v0.8.3 README.
/// </summary>
public static class BenchApp
{
    // Time budgets of bhDisplay.c.
    private const int LargeTotalMs = 1010;
    private const int LargeIterMs = 490;
    private const int SmallTotalMs = 490;
    private const int SmallIterMs = 170;

    /// <summary>The published bandwidth uses a ~100 KB input; 1&lt;&lt;17 = 131072 is the closest sweep point.</summary>
    private const int BandwidthSizeLog = 17;

    /// <summary>A benchmarked hash plus its row in the v0.8.3 README table (i7-9700K, Ubuntu 20.04, clang v10 -O3).</summary>
    private sealed record Candidate(string Name, HashFn Hash, double PublishedGbps, double PublishedVelocity);

    public static int Run(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        var options = BenchOptions.Parse(args);
        if (options is null)
            return 1;

        Xxh xxh = new XxhBackend();
        Candidate[] candidates =
        [
            new("xxh3", (input, length) => xxh.Xxh3_64bits(input, length), PublishedGbps: 31.5, PublishedVelocity: 133.1),
            new("XXH32", (input, length) => xxh.Xxh32(input, length, 0), PublishedGbps: 9.7, PublishedVelocity: 71.9),
            new("XXH64", (input, length) => xxh.Xxh64(input, length, 0), PublishedGbps: 19.4, PublishedVelocity: 71.0),
            new("XXH128", (input, length) => xxh.Xxh3_128bits(input, length).Low64, PublishedGbps: 29.6, PublishedVelocity: 118.1),
        ];

        PrintEnvironment(xxh);

        var largeLogs = Enumerable.Range(options.LargeLogMin, Math.Max(0, options.LargeLogMax - options.LargeLogMin + 1)).ToArray();
        var smallSizes = options.SmallSizes();
        var bench = new HashBench();

        Console.WriteLine($"Large inputs, MB/s (throughput, fixed size, fastest run; {1 << options.LargeLogMin} B to {(1 << options.LargeLogMax) >> 10} KiB):");
        var bandwidth = Sweep(bench, candidates, largeLogs,
            header: log => $"log{log}",
            measure: (hash, log) => bench.Run(hash, BenchMode.Throughput, 1 << log, SizeMode.Fixed, LargeTotalMs, LargeIterMs) * (1 << log) / 1e6,
            format: mbps => mbps.ToString("F0"));

        Console.WriteLine($"Small inputs, million hashes/s ({(options.Dense ? "every size" : "sampled sizes, --dense for every size")}):");
        Console.WriteLine("Throughput, fixed size:");
        Sweep(bench, candidates, smallSizes,
            header: size => size.ToString(),
            measure: (hash, size) => bench.Run(hash, BenchMode.Throughput, size, SizeMode.Fixed, SmallTotalMs, SmallIterMs) / 1e6,
            format: mhps => mhps.ToString("F1"));

        Console.WriteLine("Throughput, random size 1..N:");
        var smallVelocity = Sweep(bench, candidates, smallSizes,
            header: size => size.ToString(),
            measure: (hash, size) => bench.Run(hash, BenchMode.Throughput, size, SizeMode.Random, SmallTotalMs, SmallIterMs) / 1e6,
            format: mhps => mhps.ToString("F1"));

        Console.WriteLine("Latency, fixed size:");
        Sweep(bench, candidates, smallSizes,
            header: size => size.ToString(),
            measure: (hash, size) => bench.Run(hash, BenchMode.Latency, size, SizeMode.Fixed, SmallTotalMs, SmallIterMs) / 1e6,
            format: mhps => mhps.ToString("F1"));

        Console.WriteLine("Latency, random size 1..N:");
        Sweep(bench, candidates, smallSizes,
            header: size => size.ToString(),
            measure: (hash, size) => bench.Run(hash, BenchMode.Latency, size, SizeMode.Random, SmallTotalMs, SmallIterMs) / 1e6,
            format: mhps => mhps.ToString("F1"));

        PrintSummary(bench, candidates, largeLogs, bandwidth, smallVelocity);
        return 0;
    }

    /// <summary>Measures one row per candidate over <paramref name="points"/> and prints the values as they come.</summary>
    private static Dictionary<string, double[]> Sweep<T>(
        HashBench bench,
        Candidate[] candidates,
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

    private static void PrintSummary(
        HashBench bench,
        Candidate[] candidates,
        int[] largeLogs,
        Dictionary<string, double[]> bandwidth,
        Dictionary<string, double[]> smallVelocity)
    {
        Console.WriteLine("Summary, this run vs the table published in the xxHash v0.8.3 README:");
        Console.WriteLine($"{"",-7}{"Bandwidth (GB/s)",25}{"",4}{"Small data velocity",25}");
        Console.WriteLine($"{"",-7}{"measured",10}{"published",10}{"ratio",9}{"measured",10}{"published",10}{"ratio",9}");

        foreach (var candidate in candidates)
        {
            var bandwidthIndex = Array.IndexOf(largeLogs, BandwidthSizeLog);
            var measuredGbps = (bandwidthIndex >= 0
                ? bandwidth[candidate.Name][bandwidthIndex]
                : bench.Run(candidate.Hash, BenchMode.Throughput, 1 << BandwidthSizeLog, SizeMode.Fixed, LargeTotalMs, LargeIterMs) * (1 << BandwidthSizeLog) / 1e6) / 1000;
            var measuredVelocity = smallVelocity[candidate.Name].Average();

            Console.WriteLine(
                $"{candidate.Name,-7}{measuredGbps,10:F1}{candidate.PublishedGbps,10:F1}{measuredGbps / candidate.PublishedGbps,9:F2}" +
                $"{measuredVelocity,10:F1}{candidate.PublishedVelocity,10:F1}{measuredVelocity / candidate.PublishedVelocity,9:F2}");
        }

        Console.WriteLine();
        Console.WriteLine($"Bandwidth is the throughput at {1 << BandwidthSizeLog} bytes, matching the ~100 KB input of the published table.");
        Console.WriteLine("Small data velocity has no published formula (\"a rough average of algorithm's efficiency on small data\");");
        Console.WriteLine("here it is the average of the random-size throughput row in million hashes/s, which is in the same unit family.");
        Console.WriteLine("The published table was measured on an Intel i7-9700K with clang v10 -O3 on bare C; this run goes through the");
        Console.WriteLine("AlvorKit P/Invoke binding into an MSVC /O2 DLL (SSE2 paths, like the published \"(SSE2)\" rows), so the per-call");
        Console.WriteLine("overhead mostly shows on small inputs while large-input bandwidth is hardware-bound. Compare ratios, not absolutes.");
    }

    private static void PrintEnvironment(Xxh xxh)
    {
        Console.WriteLine("AlvorKit.Demo.XxHashBench - port of the xxHash tests/bench harness");
        Console.WriteLine($"xxhash {VersionString(xxh.VersionNumber())} via AlvorKit.XxHash.Backend, .NET {Environment.Version}");
        Console.WriteLine($"{RuntimeInformation.OSDescription}, {RuntimeInformation.ProcessArchitecture}, {Environment.ProcessorCount} logical cores");
        var cpu = CpuName();
        if (cpu is not null)
            Console.WriteLine(cpu);
        Console.WriteLine();
    }

    private static string VersionString(uint versionNumber) =>
        $"{versionNumber / 10000}.{versionNumber / 100 % 100}.{versionNumber % 100}";

    private static string? CpuName()
    {
        if (OperatingSystem.IsWindows())
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
        if (OperatingSystem.IsLinux() && File.Exists("/proc/cpuinfo"))
            return File.ReadLines("/proc/cpuinfo").FirstOrDefault(line => line.StartsWith("model name"))?.Split(':', 2)[^1].Trim();
        return null;
    }
}
