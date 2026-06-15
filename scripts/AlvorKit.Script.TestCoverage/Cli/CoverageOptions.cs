using System.Globalization;

namespace AlvorKit.Script.TestCoverage;

/// <summary>Command-line options controlling coverage collection.</summary>
/// <param name="Configuration">Build configuration passed to dotnet test.</param>
/// <param name="Thresholds">Required percentages for line, branch, and method coverage.</param>
/// <param name="TestProjectFilters">Optional test project names or paths to run.</param>
/// <param name="SourceProjectFilters">Optional source project names or paths to measure.</param>
/// <param name="BindingFilters">Optional native library binding names to measure under out/bindgen.</param>
/// <param name="MaxParallel">Maximum number of coverage-enabled test projects to run concurrently.</param>
/// <param name="GenerateHtmlReport">Whether to generate the browser-readable ReportGenerator output.</param>
/// <param name="GenerateCoberturaReport">Whether to emit raw Cobertura XML reports.</param>
/// <param name="GenerateLcovReport">Whether to emit raw LCOV reports.</param>
/// <param name="OutputRoot">Optional parent directory for run-scoped coverage output.</param>
/// <param name="RunId">Optional directory name for this coverage run.</param>
internal sealed record CoverageOptions(
    string Configuration,
    CoverageThresholds Thresholds,
    IReadOnlyList<string> TestProjectFilters,
    IReadOnlyList<string> SourceProjectFilters,
    IReadOnlyList<string> BindingFilters,
    int MaxParallel,
    bool GenerateHtmlReport,
    bool GenerateCoberturaReport,
    bool GenerateLcovReport,
    string? OutputRoot = null,
    string? RunId = null)
{
    /// <summary>Default bounded parallelism that avoids overwhelming local build and test infrastructure.</summary>
    public static int DefaultMaxParallel => Math.Min(4, Math.Max(1, Environment.ProcessorCount));

    /// <summary>Returns the Coverlet output formats required by the selected report mode.</summary>
    public IReadOnlyList<string> CoverletOutputFormats()
    {
        var formats = new List<string> { "json" };

        if (GenerateHtmlReport || GenerateCoberturaReport)
            formats.Add("cobertura");
        if (GenerateLcovReport)
            formats.Add("lcov");

        return formats;
    }

    /// <summary>Parses command-line arguments into validated coverage options.</summary>
    public static CoverageOptions Parse(IReadOnlyList<string> args)
    {
        var configuration = "Debug";
        var thresholds = CoverageThresholds.Default;
        var testProjectFilters = new List<string>();
        var sourceProjectFilters = new List<string>();
        var bindingFilters = new List<string>();
        var maxParallel = DefaultMaxParallel;
        var generateHtmlReport = true;
        var generateCoberturaReport = true;
        var generateLcovReport = true;
        string? outputRoot = null;
        string? runId = null;

        for (var index = 0; index < args.Count; index++)
        {
            switch (args[index])
            {
                case "--configuration":
                case "-c":
                    configuration = ReadValue(args, ref index);
                    break;
                case "--threshold":
                case "-t":
                    thresholds = CoverageThresholds.All(double.Parse(ReadValue(args, ref index), CultureInfo.InvariantCulture));
                    break;
                case "--line-threshold":
                    thresholds = thresholds with { Line = double.Parse(ReadValue(args, ref index), CultureInfo.InvariantCulture) };
                    break;
                case "--branch-threshold":
                    thresholds = thresholds with { Branch = double.Parse(ReadValue(args, ref index), CultureInfo.InvariantCulture) };
                    break;
                case "--method-threshold":
                    thresholds = thresholds with { Method = double.Parse(ReadValue(args, ref index), CultureInfo.InvariantCulture) };
                    break;
                case "--test-project":
                case "--project":
                    testProjectFilters.Add(ReadValue(args, ref index));
                    break;
                case "--source-project":
                case "--source":
                    sourceProjectFilters.Add(ReadValue(args, ref index));
                    break;
                case "--binding":
                    bindingFilters.Add(ReadValue(args, ref index));
                    break;
                case "--max-parallel":
                case "-m":
                    maxParallel = int.Parse(ReadValue(args, ref index), CultureInfo.InvariantCulture);
                    break;
                case "--agent":
                case "--agent-fast":
                    generateHtmlReport = false;
                    generateCoberturaReport = false;
                    generateLcovReport = false;
                    break;
                case "--no-html":
                    generateHtmlReport = false;
                    break;
                case "--no-lcov":
                    generateLcovReport = false;
                    break;
                case "--output-root":
                    outputRoot = ReadValue(args, ref index);
                    break;
                case "--run-id":
                    runId = ReadValue(args, ref index);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[index]}'.");
            }
        }

        thresholds.Validate();
        if (maxParallel < 1)
            throw new ArgumentOutOfRangeException(nameof(args), "Max parallelism must be at least 1.");
        if (outputRoot is { Length: 0 })
            throw new ArgumentException("Output root must not be empty.", nameof(args));
        if (runId is not null)
            ValidateRunId(runId);

        return new(
            configuration,
            thresholds,
            testProjectFilters,
            sourceProjectFilters,
            bindingFilters,
            maxParallel,
            generateHtmlReport,
            generateCoberturaReport,
            generateLcovReport,
            outputRoot,
            runId);
    }

    /// <summary>Rejects run IDs that cannot safely be used as one directory name.</summary>
    private static void ValidateRunId(string value)
    {
        if (value.Length == 0)
            throw new ArgumentException("Run ID must not be empty.");
        if (value.Contains('/') || value.Contains('\\') || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Run ID must be a single valid directory name.");
    }

    /// <summary>Reads the value following an option name.</summary>
    private static string ReadValue(IReadOnlyList<string> args, ref int index)
    {
        if (++index >= args.Count)
            throw new ArgumentException($"Missing value for '{args[index - 1]}'.");

        return args[index];
    }
}
