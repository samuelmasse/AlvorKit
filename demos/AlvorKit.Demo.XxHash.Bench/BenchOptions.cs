namespace AlvorKit.Demo.XxHash.Bench;

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
internal sealed record BenchOptions(int LargeLogMin, int LargeLogMax, int SmallMin, int SmallMax, bool Dense)
{
    /// <summary>The sampled small-input sizes used by default to keep the demo compact.</summary>
    private static ReadOnlySpan<int> SampledSmallInputSizes => [1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 127];

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

    /// <summary>Returns the log2 large-input sizes to measure.</summary>
    public int[] LargeInputLogs()
    {
        var count = Math.Max(0, LargeLogMax - LargeLogMin + 1);
        var logs = new int[count];
        for (var index = 0; index < logs.Length; index++)
            logs[index] = LargeLogMin + index;
        return logs;
    }

    /// <summary>Returns the small input sizes to measure: every size when dense, a sampled subset otherwise.</summary>
    public int[] SmallInputSizes()
    {
        if (Dense)
        {
            var count = Math.Max(0, SmallMax - SmallMin + 1);
            var sizes = new int[count];
            for (var index = 0; index < sizes.Length; index++)
                sizes[index] = SmallMin + index;
            return sizes;
        }

        var sampled = SampledSmallInputSizes;
        var inRange = new List<int>(sampled.Length + 1);
        foreach (var size in sampled)
        {
            if (size >= SmallMin && size <= SmallMax)
                inRange.Add(size);
        }

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
