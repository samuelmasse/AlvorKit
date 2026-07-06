namespace AlvorKit.Script.TestTiming;

/// <summary>Parses command-line options for the unit test timing guard.</summary>
internal sealed class TestTimingCommandParser
{
    /// <summary>Parses options using repository discovery for default paths.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public TestTimingOptions Parse(IReadOnlyList<string> args) =>
        Parse(args, SolutionRoot.FindPrimaryFromCurrentProcess(typeof(TestTimingCommandParser)));

    /// <summary>Parses options with an explicit repository root for deterministic tests.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    /// <param name="defaultRepoRoot">Repository root used when no <c>--repo-root</c> option is supplied.</param>
    internal TestTimingOptions Parse(IReadOnlyList<string> args, string defaultRepoRoot)
    {
        var repoRoot = Path.GetFullPath(defaultRepoRoot);
        var split = TestTimingCommandLineSplit.Create(args);
        var options = TimingOptions();
        var command = CreateRootCommand(options);
        var result = command.Parse(split.TimingArguments);
        ThrowIfErrors(result);
        return Options(result, options, repoRoot, split.ForwardedArguments);
    }

    /// <summary>Creates the command-line surface for the timing guard.</summary>
    /// <param name="defaultRepoRoot">Repository root provider used when <c>--repo-root</c> is omitted.</param>
    /// <param name="forwardedArguments">Arguments forwarded to <c>dotnet test</c>.</param>
    /// <param name="execute">Action that executes timing with parsed options.</param>
    internal static RootCommand CreateRootCommand(
        Func<string> defaultRepoRoot,
        IReadOnlyList<string> forwardedArguments,
        Func<TestTimingOptions, int> execute)
    {
        var options = TimingOptions();
        var command = CreateRootCommand(options);
        command.SetAction(parse => execute(Options(parse, options, Path.GetFullPath(defaultRepoRoot()), forwardedArguments)));
        return command;
    }

    /// <summary>Adds the solution path when callers provide only options for <c>dotnet test</c>.</summary>
    internal static IReadOnlyList<string> NormalizeDotNetArguments(IReadOnlyList<string> args)
    {
        var repoRoot = SolutionRoot.FindPrimaryFromCurrentProcess(typeof(TestTimingCommandParser));
        return NormalizeDotNetArguments(repoRoot, args);
    }

    /// <summary>Adds the solution path when callers provide only options for <c>dotnet test</c>.</summary>
    internal static IReadOnlyList<string> NormalizeDotNetArguments(string repoRoot, IReadOnlyList<string> args)
    {
        if (args.Count == 0)
            return TestTimingOptions.DefaultDotNetTestArguments(repoRoot);

        return args[0].StartsWith("-", StringComparison.Ordinal)
            ? [SolutionRoot.PrimarySolutionFileName(repoRoot), .. args]
            : args.ToArray();
    }

    /// <summary>Creates the option instances used by one timing command tree.</summary>
    private static (
        Option<string> MaxDuration,
        Option<bool> WarnOnly,
        Option<string> Trx,
        Option<string> ResultsDirectory,
        Option<string> RepoRoot) TimingOptions() =>
        (
            new("--max-duration-ms") { Description = "Maximum allowed duration for one test." },
            new("--warn-only") { Description = "Warn instead of failing when tests are slow." },
            new("--trx") { Description = "Existing TRX file to inspect." },
            new("--results-directory") { Description = "Directory for reports and TRX files." },
            new("--repo-root") { Description = "Repository root." });

    /// <summary>Creates the System.CommandLine root command for timing guard options.</summary>
    private static RootCommand CreateRootCommand(
        (
            Option<string> MaxDuration,
            Option<bool> WarnOnly,
            Option<string> Trx,
            Option<string> ResultsDirectory,
            Option<string> RepoRoot) options)
    {
        var command = new RootCommand("Unit test timing guard.");
        command.Options.Add(options.MaxDuration);
        command.Options.Add(options.WarnOnly);
        command.Options.Add(options.Trx);
        command.Options.Add(options.ResultsDirectory);
        command.Options.Add(options.RepoRoot);
        return command;
    }

    /// <summary>Creates immutable timing options from parsed command-line values.</summary>
    private static TestTimingOptions Options(
        ParseResult parse,
        (
            Option<string> MaxDuration,
            Option<bool> WarnOnly,
            Option<string> Trx,
            Option<string> ResultsDirectory,
            Option<string> RepoRoot) options,
        string defaultRepoRoot,
        IReadOnlyList<string> forwardedArguments)
    {
        var root = Path.GetFullPath(parse.GetValue(options.RepoRoot) ?? defaultRepoRoot);
        var results = parse.GetValue(options.ResultsDirectory) ?? Path.Combine(root, "out", "test-timing", "runs", CreateRunId());
        var trxPath = parse.GetValue(options.Trx);
        return new(
            root,
            Path.GetFullPath(results, root),
            ParseMilliseconds(parse.GetValue(options.MaxDuration)),
            parse.GetValue(options.WarnOnly),
            trxPath is null ? null : Path.GetFullPath(trxPath, root),
            NormalizeDotNetArguments(root, forwardedArguments));
    }

    /// <summary>Parses an optional positive millisecond duration.</summary>
    private static TimeSpan ParseMilliseconds(string? value)
    {
        if (value is null)
            return TestTimingOptions.DefaultMaxDuration;
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds) || milliseconds <= 0)
            throw new ArgumentException("--max-duration-ms must be a positive number.");

        return TimeSpan.FromMilliseconds(milliseconds);
    }

    /// <summary>Creates a filesystem-safe identifier for this timing run.</summary>
    private static string CreateRunId() =>
        $"{DateTimeOffset.UtcNow:yyyyMMddTHHmmssfffZ}-{Environment.ProcessId}";

    /// <summary>Throws an argument exception when System.CommandLine found parse errors.</summary>
    private static void ThrowIfErrors(ParseResult result)
    {
        if (result.Action is System.CommandLine.Help.HelpAction)
            throw new ArgumentException("Help is generated by the command-line app.");
        if (result.Errors.Count > 0)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.Message)));
    }
}
