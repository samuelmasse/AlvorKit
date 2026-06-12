namespace AlvorKit.Demo.XxHashBench;

/// <summary>
/// Command line options, mirroring the flags of the upstream harness
/// (tests/bench/main.c): --minl/--maxl for the large input size logs and
/// --mins/--maxs for the small input sizes.
/// </summary>
public sealed record BenchOptions(int LargeLogMin, int LargeLogMax, int SmallMin, int SmallMax, bool Dense)
{
    // Upstream defaults are minl=9 maxl=27 mins=1 maxs=127; maxl is lowered here
    // to keep the demo around three minutes, and the small sweep is sampled
    // unless --dense is given. The in-cache plateau the published table reports
    // sits well inside log 9..22.
    public const int LargeLogMinDefault = 9;
    public const int LargeLogMaxDefault = 22;
    public const int SmallMinDefault = 1;
    public const int SmallMaxDefault = 127;

    public static BenchOptions? Parse(string[] args)
    {
        var options = new BenchOptions(LargeLogMinDefault, LargeLogMaxDefault, SmallMinDefault, SmallMaxDefault, Dense: false);
        foreach (var arg in args)
        {
            if (TryReadInt(arg, "--minl=", out var minl))
                options = options with { LargeLogMin = minl };
            else if (TryReadInt(arg, "--maxl=", out var maxl))
                options = options with { LargeLogMax = maxl };
            else if (TryReadInt(arg, "--mins=", out var mins))
                options = options with { SmallMin = mins };
            else if (TryReadInt(arg, "--maxs=", out var maxs))
                options = options with { SmallMax = maxs };
            else if (arg == "--dense")
                options = options with { Dense = true };
            else
            {
                PrintUsage(arg);
                return null;
            }
        }

        if (options.LargeLogMax is < 0 or > 27 || options.LargeLogMin < 0 || options.SmallMin < 1 || options.SmallMax > 100_000)
        {
            PrintUsage(null);
            return null;
        }
        return options;
    }

    /// <summary>The small input sizes to measure: every size when dense, a sampled subset otherwise.</summary>
    public int[] SmallSizes()
    {
        if (Dense)
            return [.. Enumerable.Range(SmallMin, Math.Max(0, SmallMax - SmallMin + 1))];

        int[] sampled = [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 127];
        var inRange = sampled.Where(size => size >= SmallMin && size <= SmallMax).ToList();
        if (inRange.Count == 0 || inRange[^1] != SmallMax)
            inRange.Add(SmallMax);
        return [.. inRange];
    }

    private static bool TryReadInt(string arg, string name, out int value)
    {
        value = 0;
        return arg.StartsWith(name, StringComparison.Ordinal) && int.TryParse(arg.AsSpan(name.Length), out value);
    }

    private static void PrintUsage(string? badArg)
    {
        if (badArg is not null)
            Console.WriteLine($"Unknown argument: {badArg}");
        Console.WriteLine("usage: AlvorKit.Demo.XxHashBench [options]");
        Console.WriteLine($"  --minl=LOG   First large-input size log2 (default: {LargeLogMinDefault})");
        Console.WriteLine($"  --maxl=LOG   Last large-input size log2, up to 27 (default: {LargeLogMaxDefault})");
        Console.WriteLine($"  --mins=LEN   Starting length for the small-size bench (default: {SmallMinDefault})");
        Console.WriteLine($"  --maxs=LEN   End length for the small-size bench (default: {SmallMaxDefault})");
        Console.WriteLine("  --dense      Measure every small size instead of a sampled subset (slow)");
    }
}
