using System.Globalization;

namespace AlvorKit.Script.TestCoverage;

/// <summary>Command-line options controlling coverage collection.</summary>
/// <param name="Configuration">Build configuration passed to dotnet test.</param>
/// <param name="Threshold">Required percentage for line, branch, and method coverage.</param>
/// <param name="TestProjectFilters">Optional test project names or paths to run.</param>
internal sealed record CoverageOptions(string Configuration, double Threshold, IReadOnlyList<string> TestProjectFilters)
{
    /// <summary>Parses command-line arguments into validated coverage options.</summary>
    public static CoverageOptions Parse(IReadOnlyList<string> args)
    {
        var configuration = "Debug";
        var threshold = 100.0;
        var testProjectFilters = new List<string>();

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
                default:
                    throw new ArgumentException($"Unknown argument '{args[index]}'.");
            }
        }

        if (threshold is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(args), "Threshold must be between 0 and 100.");

        return new(configuration, threshold, testProjectFilters);
    }

    /// <summary>Reads the value following an option name.</summary>
    private static string ReadValue(IReadOnlyList<string> args, ref int index)
    {
        if (++index >= args.Count)
            throw new ArgumentException($"Missing value for '{args[index - 1]}'.");

        return args[index];
    }
}
