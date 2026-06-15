namespace AlvorKit.Demo.XxHashBench;

/// <summary>
/// Command line options, mirroring the flags of the upstream harness
/// (tests/bench/main.c): --minl/--maxl for the large input size logs and
/// --mins/--maxs for the small input sizes.
/// </summary>
/// <param name="LargeLogMin">The first log2 size used by the large-input sweep.</param>
/// <param name="LargeLogMax">The last log2 size used by the large-input sweep.</param>
/// <param name="SmallMin">The first byte length considered by the small-input sweep.</param>
/// <param name="SmallMax">The last byte length considered by the small-input sweep.</param>
/// <param name="Dense">Whether the small-input sweep measures every size instead of the sampled set.</param>
public sealed record BenchOptions(int LargeLogMin, int LargeLogMax, int SmallMin, int SmallMax, bool Dense)
{
    // Upstream defaults are minl=9 maxl=27 mins=1 maxs=127; maxl is lowered here
    // to keep the demo around three minutes, and the small sweep is sampled
    // unless --dense is given. The in-cache plateau the published table reports
    // sits well inside log 9..22.
    /// <summary>The default first log2 size used by the large-input sweep.</summary>
    public const int LargeLogMinDefault = 9;

    /// <summary>The default last log2 size used by the large-input sweep.</summary>
    public const int LargeLogMaxDefault = 22;

    /// <summary>The default first byte length used by the small-input sweep.</summary>
    public const int SmallMinDefault = 1;

    /// <summary>The default last byte length used by the small-input sweep.</summary>
    public const int SmallMaxDefault = 127;

    /// <summary>Parses benchmark options and lets invalid command lines fail fast instead of wrapping the demo in usage handling.</summary>
    /// <param name="args">The command line arguments passed to the demo.</param>
    /// <returns>The parsed options, with defaults filled in for omitted flags.</returns>
    public static BenchOptions Parse(string[] args)
    {
        var options = new BenchOptions(LargeLogMinDefault, LargeLogMaxDefault, SmallMinDefault, SmallMaxDefault, Dense: false);
        foreach (var arg in args)
        {
            if (arg == "--dense")
                options = options with { Dense = true };
            else if (arg.StartsWith("--minl=", StringComparison.Ordinal))
                options = options with { LargeLogMin = int.Parse(arg.AsSpan("--minl=".Length)) };
            else if (arg.StartsWith("--maxl=", StringComparison.Ordinal))
                options = options with { LargeLogMax = int.Parse(arg.AsSpan("--maxl=".Length)) };
            else if (arg.StartsWith("--mins=", StringComparison.Ordinal))
                options = options with { SmallMin = int.Parse(arg.AsSpan("--mins=".Length)) };
            else if (arg.StartsWith("--maxs=", StringComparison.Ordinal))
                options = options with { SmallMax = int.Parse(arg.AsSpan("--maxs=".Length)) };
            else
                throw new ArgumentException($"Unknown benchmark option '{arg}'.", nameof(args));
        }

        Validate(options);
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

    /// <summary>Enforces the size ranges needed to keep the demo measurements meaningful and memory-safe.</summary>
    /// <param name="options">The parsed options to validate.</param>
    private static void Validate(BenchOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(options.LargeLogMin);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(options.LargeLogMax, 27);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.LargeLogMax, options.LargeLogMin);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.SmallMin, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.SmallMax, options.SmallMin);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(options.SmallMax, 100_000);
    }
}
