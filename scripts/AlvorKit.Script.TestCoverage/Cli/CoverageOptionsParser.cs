namespace AlvorKit.Script.TestCoverage;

/// <summary>Parses command-line arguments for the coverage runner.</summary>
internal sealed class CoverageOptionsParser
{
    /// <summary>Creates the command-line surface for the coverage runner.</summary>
    /// <param name="execute">Action that executes coverage with parsed options.</param>
    internal static RootCommand CreateRootCommand(Func<CoverageOptions, Task<int>> execute)
    {
        var options = CreateCliOptions();
        var command = new RootCommand("Repository coverage reporting tool.");
        AddOptions(command, options);
        command.SetAction(parse => execute(ToCoverageOptions(parse, options)));
        return command;
    }

    /// <summary>Parses command-line arguments into validated coverage options.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public CoverageOptions Parse(IReadOnlyList<string> args)
    {
        var options = CreateCliOptions();
        var command = new RootCommand("Repository coverage reporting tool.");
        AddOptions(command, options);
        var result = command.Parse(args.ToArray());
        ThrowIfErrors(result);
        return ToCoverageOptions(result, options);
    }

    /// <summary>Creates the option instances used by one command tree.</summary>
    private static (
        Option<string> Configuration,
        Option<string> Threshold,
        Option<string> LineThreshold,
        Option<string> BranchThreshold,
        Option<string> MethodThreshold,
        Option<string[]> TestProjects,
        Option<string[]> SourceProjects,
        Option<string[]> Bindings,
        Option<string> MaxParallel,
        Option<bool> Agent,
        Option<bool> NoHtml,
        Option<bool> NoLcov,
        Option<string> OutputRoot,
        Option<string> RunId,
        Option<string> MaxTestDuration,
        Option<bool> TestTimingWarnOnly) CreateCliOptions() =>
        (
            new("--configuration", "-c") { Description = "Build configuration." },
            new("--threshold", "-t") { Description = "Coverage threshold for every metric." },
            new("--line-threshold") { Description = "Line coverage threshold." },
            new("--branch-threshold") { Description = "Branch coverage threshold." },
            new("--method-threshold") { Description = "Method coverage threshold." },
            new("--test-project", "--project") { Description = "Test project filter." },
            new("--source-project", "--source") { Description = "Source project filter." },
            new("--binding") { Description = "Native binding filter." },
            new("--max-parallel", "-m") { Description = "Maximum parallel test projects." },
            new("--agent", "--agent-fast") { Description = "Emit only agent-readable coverage artifacts." },
            new("--no-html") { Description = "Skip HTML report generation." },
            new("--no-lcov") { Description = "Skip LCOV report generation." },
            new("--output-root") { Description = "Coverage output parent directory." },
            new("--run-id") { Description = "Stable coverage run directory name." },
            new("--max-test-duration-ms") { Description = "Per-test duration budget in milliseconds." },
            new("--test-timing-warn-only") { Description = "Warn instead of failing on slow tests." });

    /// <summary>Adds the coverage option set to the command.</summary>
    private static void AddOptions(
        RootCommand command,
        (
            Option<string> Configuration,
            Option<string> Threshold,
            Option<string> LineThreshold,
            Option<string> BranchThreshold,
            Option<string> MethodThreshold,
            Option<string[]> TestProjects,
            Option<string[]> SourceProjects,
            Option<string[]> Bindings,
            Option<string> MaxParallel,
            Option<bool> Agent,
            Option<bool> NoHtml,
            Option<bool> NoLcov,
            Option<string> OutputRoot,
            Option<string> RunId,
            Option<string> MaxTestDuration,
            Option<bool> TestTimingWarnOnly) options)
    {
        command.Options.Add(options.Configuration);
        command.Options.Add(options.Threshold);
        command.Options.Add(options.LineThreshold);
        command.Options.Add(options.BranchThreshold);
        command.Options.Add(options.MethodThreshold);
        command.Options.Add(options.TestProjects);
        command.Options.Add(options.SourceProjects);
        command.Options.Add(options.Bindings);
        command.Options.Add(options.MaxParallel);
        command.Options.Add(options.Agent);
        command.Options.Add(options.NoHtml);
        command.Options.Add(options.NoLcov);
        command.Options.Add(options.OutputRoot);
        command.Options.Add(options.RunId);
        command.Options.Add(options.MaxTestDuration);
        command.Options.Add(options.TestTimingWarnOnly);
    }

    /// <summary>Creates immutable coverage options from parsed command-line values.</summary>
    private static CoverageOptions ToCoverageOptions(
        ParseResult parse,
        (
            Option<string> Configuration,
            Option<string> Threshold,
            Option<string> LineThreshold,
            Option<string> BranchThreshold,
            Option<string> MethodThreshold,
            Option<string[]> TestProjects,
            Option<string[]> SourceProjects,
            Option<string[]> Bindings,
            Option<string> MaxParallel,
            Option<bool> Agent,
            Option<bool> NoHtml,
            Option<bool> NoLcov,
            Option<string> OutputRoot,
            Option<string> RunId,
            Option<string> MaxTestDuration,
            Option<bool> TestTimingWarnOnly) options)
    {
        var thresholds = Thresholds(parse, options);
        var parallel = IntOption(parse, options.MaxParallel, CoverageOptions.DefaultMaxParallel);
        var maxDuration = DoubleOption(parse, options.MaxTestDuration, 1000.0);
        var reports = Reports(parse, options);
        var root = parse.GetValue(options.OutputRoot);
        var id = parse.GetValue(options.RunId);
        CoverageOptionsValidation.Validate(thresholds, parallel, root, id, maxDuration);

        return new(
            parse.GetValue(options.Configuration) ?? "Debug",
            thresholds,
            parse.GetValue(options.TestProjects) ?? [],
            parse.GetValue(options.SourceProjects) ?? [],
            parse.GetValue(options.Bindings) ?? [],
            parallel,
            reports.Html,
            reports.Cobertura,
            reports.Lcov,
            root,
            id,
            maxDuration,
            parse.GetValue(options.TestTimingWarnOnly));
    }

    /// <summary>Builds threshold settings from the all-metric and metric-specific options.</summary>
    private static CoverageThresholds Thresholds(
        ParseResult parse,
        (
            Option<string> Configuration,
            Option<string> Threshold,
            Option<string> LineThreshold,
            Option<string> BranchThreshold,
            Option<string> MethodThreshold,
            Option<string[]> TestProjects,
            Option<string[]> SourceProjects,
            Option<string[]> Bindings,
            Option<string> MaxParallel,
            Option<bool> Agent,
            Option<bool> NoHtml,
            Option<bool> NoLcov,
            Option<string> OutputRoot,
            Option<string> RunId,
            Option<string> MaxTestDuration,
            Option<bool> TestTimingWarnOnly) options)
    {
        var value = DoubleOption(parse, options.Threshold, double.NaN);
        var thresholds = double.IsNaN(value) ? CoverageThresholds.Default : CoverageThresholds.All(value);
        return thresholds with
        {
            Line = DoubleOption(parse, options.LineThreshold, thresholds.Line),
            Branch = DoubleOption(parse, options.BranchThreshold, thresholds.Branch),
            Method = DoubleOption(parse, options.MethodThreshold, thresholds.Method)
        };
    }

    /// <summary>Builds report mode settings from agent and individual disable flags.</summary>
    private static CoverageReportModes Reports(
        ParseResult parse,
        (
            Option<string> Configuration,
            Option<string> Threshold,
            Option<string> LineThreshold,
            Option<string> BranchThreshold,
            Option<string> MethodThreshold,
            Option<string[]> TestProjects,
            Option<string[]> SourceProjects,
            Option<string[]> Bindings,
            Option<string> MaxParallel,
            Option<bool> Agent,
            Option<bool> NoHtml,
            Option<bool> NoLcov,
            Option<string> OutputRoot,
            Option<string> RunId,
            Option<string> MaxTestDuration,
            Option<bool> TestTimingWarnOnly) options)
    {
        if (parse.GetValue(options.Agent))
            return CoverageReportModes.Agent;

        return CoverageReportModes.All with
        {
            Html = !parse.GetValue(options.NoHtml),
            Lcov = !parse.GetValue(options.NoLcov)
        };
    }

    /// <summary>Parses an optional floating-point option using invariant culture.</summary>
    private static double DoubleOption(ParseResult parse, Option<string> option, double defaultValue) =>
        parse.GetValue(option) is { } value ? double.Parse(value, CultureInfo.InvariantCulture) : defaultValue;

    /// <summary>Parses an optional integer option using invariant culture.</summary>
    private static int IntOption(ParseResult parse, Option<string> option, int defaultValue) =>
        parse.GetValue(option) is { } value ? int.Parse(value, CultureInfo.InvariantCulture) : defaultValue;

    /// <summary>Throws an argument exception when System.CommandLine found parse errors.</summary>
    private static void ThrowIfErrors(ParseResult result)
    {
        if (result.Action is System.CommandLine.Help.HelpAction)
            throw new ArgumentException("Help is generated by the command-line app.");
        if (result.Errors.Count > 0)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.Message)));
    }
}
