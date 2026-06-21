namespace AlvorKit.Script.TestTiming;

/// <summary>Parses command-line options for the unit test timing guard.</summary>
internal sealed class TestTimingCommandParser
{
    /// <summary>Usage text for the timing guard.</summary>
    public const string HelpText =
        """
        Usage:
          dotnet run --project scripts\AlvorKit.Script.TestTiming -- [options] [dotnet test args]

        Examples:
          dotnet run --project scripts\AlvorKit.Script.TestTiming -- AlvorKit.slnx --no-build --no-restore
          dotnet run --project scripts\AlvorKit.Script.TestTiming -- --warn-only --trx out\test-timing\run.trx

        Options:
          --max-duration-ms <n>     Maximum allowed duration for one test. Default: 1000.
          --warn-only              Print slow-test warnings but do not fail when tests pass.
          --trx <path>             Inspect an existing TRX file instead of running dotnet test.
          --results-directory <p>  Directory for TRX files and timing reports.
          --repo-root <path>       Repository root. Default: discovered from the current process.
          -h, --help, help         Show this help.
        """;

    /// <summary>Parses options using repository discovery for default paths.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public TestTimingOptions Parse(IReadOnlyList<string> args) =>
        Parse(args, ProjectRoot.FindFromCurrentProcess(typeof(TestTimingCommandParser)));

    /// <summary>Parses options with an explicit repository root for deterministic tests.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    /// <param name="defaultRepoRoot">Repository root used when no <c>--repo-root</c> option is supplied.</param>
    internal TestTimingOptions Parse(IReadOnlyList<string> args, string defaultRepoRoot)
    {
        var repoRoot = Path.GetFullPath(defaultRepoRoot);
        if (args.Count == 0 || IsHelp(args[0]))
            return TestTimingOptions.Help(repoRoot);

        TimeSpan maxDuration = TestTimingOptions.DefaultMaxDuration;
        string? resultsDirectory = null;
        string? trxPath = null;
        var warnOnly = false;
        var dotNetArgs = new List<string>();

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            if (arg == "--")
            {
                AddRemaining(dotNetArgs, args, i + 1);
                break;
            }

            if (arg == "--warn-only")
            {
                warnOnly = true;
                continue;
            }

            if (TryParseValuedOption(args, ref i, ref repoRoot, ref resultsDirectory, ref trxPath, ref maxDuration))
                continue;

            AddRemaining(dotNetArgs, args, i);
            break;
        }

        resultsDirectory ??= Path.Combine(repoRoot, "out", "test-timing", "runs", CreateRunId());
        return new TestTimingOptions(
            repoRoot,
            Path.GetFullPath(resultsDirectory, repoRoot),
            maxDuration,
            warnOnly,
            trxPath is null ? null : Path.GetFullPath(trxPath, repoRoot),
            NormalizeDotNetArguments(dotNetArgs));
    }

    /// <summary>Parses one option that requires a following value.</summary>
    private static bool TryParseValuedOption(
        IReadOnlyList<string> args,
        ref int index,
        ref string repoRoot,
        ref string? resultsDirectory,
        ref string? trxPath,
        ref TimeSpan maxDuration)
    {
        switch (args[index])
        {
            case "--repo-root":
                repoRoot = Path.GetFullPath(RequiredValue(args, ref index));
                return true;
            case "--results-directory":
                resultsDirectory = RequiredValue(args, ref index);
                return true;
            case "--trx":
                trxPath = RequiredValue(args, ref index);
                return true;
            case "--max-duration-ms":
                maxDuration = ParseMilliseconds(RequiredValue(args, ref index));
                return true;
            default:
                return false;
        }
    }

    /// <summary>Returns the required value following an option.</summary>
    private static string RequiredValue(IReadOnlyList<string> args, ref int index)
    {
        if (index + 1 >= args.Count)
            throw new ArgumentException($"{args[index]} requires a value.");

        index++;
        return args[index];
    }

    /// <summary>Parses a positive millisecond duration.</summary>
    private static TimeSpan ParseMilliseconds(string value)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds) || milliseconds <= 0)
            throw new ArgumentException("--max-duration-ms must be a positive number.");

        return TimeSpan.FromMilliseconds(milliseconds);
    }

    /// <summary>Adds all remaining arguments to the forwarded <c>dotnet test</c> argument list.</summary>
    private static void AddRemaining(List<string> target, IReadOnlyList<string> args, int start)
    {
        for (var i = start; i < args.Count; i++)
            target.Add(args[i]);
    }

    /// <summary>Adds the solution path when callers provide only options for <c>dotnet test</c>.</summary>
    private static IReadOnlyList<string> NormalizeDotNetArguments(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
            return TestTimingOptions.DefaultDotNetTestArguments;

        return args[0].StartsWith("-", StringComparison.Ordinal)
            ? ["AlvorKit.slnx", .. args]
            : args.ToArray();
    }

    /// <summary>Returns whether an argument requests help output.</summary>
    private static bool IsHelp(string arg) =>
        arg is "help" or "-h" or "--help";

    /// <summary>Creates a filesystem-safe identifier for this timing run.</summary>
    private static string CreateRunId() =>
        $"{DateTimeOffset.UtcNow:yyyyMMddTHHmmssfffZ}-{Environment.ProcessId}";
}
