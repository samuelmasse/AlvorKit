namespace AlvorKit.Script.TestCoverage;

/// <summary>Parses command-line arguments for the coverage runner.</summary>
internal sealed class CoverageOptionsParser
{
    /// <summary>Parses command-line arguments into validated coverage options.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public CoverageOptions Parse(IReadOnlyList<string> args)
    {
        var configuration = "Debug";
        var thresholds = CoverageThresholds.Default;
        var testProjectFilters = new List<string>();
        var sourceProjectFilters = new List<string>();
        var bindingFilters = new List<string>();
        var maxParallel = CoverageOptions.DefaultMaxParallel;
        var reports = CoverageReportModes.All;
        string? outputRoot = null;
        string? runId = null;
        var maxTestDurationMilliseconds = 1000.0;
        var testTimingWarnOnly = false;

        for (var index = 0; index < args.Count; index++)
            ParseOne(
                args,
                ref index,
                ref configuration,
                ref thresholds,
                testProjectFilters,
                sourceProjectFilters,
                bindingFilters,
                ref maxParallel,
                ref reports,
                ref outputRoot,
                ref runId,
                ref maxTestDurationMilliseconds,
                ref testTimingWarnOnly);

        CoverageOptionsValidation.Validate(thresholds, maxParallel, outputRoot, runId, maxTestDurationMilliseconds);
        return new(
            configuration,
            thresholds,
            testProjectFilters,
            sourceProjectFilters,
            bindingFilters,
            maxParallel,
            reports.Html,
            reports.Cobertura,
            reports.Lcov,
            outputRoot,
            runId,
            maxTestDurationMilliseconds,
            testTimingWarnOnly);
    }

    /// <summary>Parses one command-line option and updates the accumulated option state.</summary>
    private static void ParseOne(
        IReadOnlyList<string> args,
        ref int index,
        ref string configuration,
        ref CoverageThresholds thresholds,
        List<string> testProjectFilters,
        List<string> sourceProjectFilters,
        List<string> bindingFilters,
        ref int maxParallel,
        ref CoverageReportModes reports,
        ref string? outputRoot,
        ref string? runId,
        ref double maxTestDurationMilliseconds,
        ref bool testTimingWarnOnly)
    {
        switch (args[index])
        {
            case "--configuration":
            case "-c":
                configuration = CoverageOptionsValidation.ReadValue(args, ref index);
                break;
            case "--threshold":
            case "-t":
                thresholds = CoverageThresholds.All(double.Parse(CoverageOptionsValidation.ReadValue(args, ref index), CultureInfo.InvariantCulture));
                break;
            case "--line-threshold":
                thresholds = thresholds with { Line = double.Parse(CoverageOptionsValidation.ReadValue(args, ref index), CultureInfo.InvariantCulture) };
                break;
            case "--branch-threshold":
                thresholds = thresholds with { Branch = double.Parse(CoverageOptionsValidation.ReadValue(args, ref index), CultureInfo.InvariantCulture) };
                break;
            case "--method-threshold":
                thresholds = thresholds with { Method = double.Parse(CoverageOptionsValidation.ReadValue(args, ref index), CultureInfo.InvariantCulture) };
                break;
            case "--test-project":
            case "--project":
                testProjectFilters.Add(CoverageOptionsValidation.ReadValue(args, ref index));
                break;
            case "--source-project":
            case "--source":
                sourceProjectFilters.Add(CoverageOptionsValidation.ReadValue(args, ref index));
                break;
            case "--binding":
                bindingFilters.Add(CoverageOptionsValidation.ReadValue(args, ref index));
                break;
            case "--max-parallel":
            case "-m":
                maxParallel = int.Parse(CoverageOptionsValidation.ReadValue(args, ref index), CultureInfo.InvariantCulture);
                break;
            case "--agent":
            case "--agent-fast":
                reports = CoverageReportModes.Agent;
                break;
            case "--no-html":
                reports = reports with { Html = false };
                break;
            case "--no-lcov":
                reports = reports with { Lcov = false };
                break;
            case "--output-root":
                outputRoot = CoverageOptionsValidation.ReadValue(args, ref index);
                break;
            case "--run-id":
                runId = CoverageOptionsValidation.ReadValue(args, ref index);
                break;
            case "--max-test-duration-ms":
                maxTestDurationMilliseconds = double.Parse(CoverageOptionsValidation.ReadValue(args, ref index), CultureInfo.InvariantCulture);
                break;
            case "--test-timing-warn-only":
                testTimingWarnOnly = true;
                break;
            default:
                throw new ArgumentException($"Unknown argument '{args[index]}'.");
        }
    }

}
