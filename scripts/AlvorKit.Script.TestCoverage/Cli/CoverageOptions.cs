using System.Globalization;

namespace AlvorKit.Script.TestCoverage;

/// <summary>Command-line options controlling coverage collection.</summary>
/// <param name="Configuration">Build configuration passed to dotnet test.</param>
/// <param name="Threshold">Required percentage for line, branch, and method coverage.</param>
/// <param name="TestProjectFilters">Optional test project names or paths to run.</param>
/// <param name="SourceProjectFilters">Optional source project names or paths to measure.</param>
/// <param name="MaxParallel">Maximum number of coverage-enabled test projects to run concurrently.</param>
/// <param name="GenerateHtmlReport">Whether to generate the browser-readable ReportGenerator output.</param>
/// <param name="GenerateCoberturaReport">Whether to emit raw Cobertura XML reports.</param>
/// <param name="GenerateLcovReport">Whether to emit raw LCOV reports.</param>
internal sealed record CoverageOptions(
    string Configuration,
    double Threshold,
    IReadOnlyList<string> TestProjectFilters,
    IReadOnlyList<string> SourceProjectFilters,
    int MaxParallel,
    bool GenerateHtmlReport,
    bool GenerateCoberturaReport,
    bool GenerateLcovReport)
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
        var threshold = 100.0;
        var testProjectFilters = new List<string>();
        var sourceProjectFilters = new List<string>();
        var maxParallel = DefaultMaxParallel;
        var generateHtmlReport = true;
        var generateCoberturaReport = true;
        var generateLcovReport = true;

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
                    threshold = double.Parse(ReadValue(args, ref index), CultureInfo.InvariantCulture);
                    break;
                case "--test-project":
                case "--project":
                    testProjectFilters.Add(ReadValue(args, ref index));
                    break;
                case "--source-project":
                case "--source":
                    sourceProjectFilters.Add(ReadValue(args, ref index));
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
                default:
                    throw new ArgumentException($"Unknown argument '{args[index]}'.");
            }
        }

        if (threshold is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(args), "Threshold must be between 0 and 100.");
        if (maxParallel < 1)
            throw new ArgumentOutOfRangeException(nameof(args), "Max parallelism must be at least 1.");

        return new(
            configuration,
            threshold,
            testProjectFilters,
            sourceProjectFilters,
            maxParallel,
            generateHtmlReport,
            generateCoberturaReport,
            generateLcovReport);
    }

    /// <summary>Reads the value following an option name.</summary>
    private static string ReadValue(IReadOnlyList<string> args, ref int index)
    {
        if (++index >= args.Count)
            throw new ArgumentException($"Missing value for '{args[index - 1]}'.");

        return args[index];
    }
}
