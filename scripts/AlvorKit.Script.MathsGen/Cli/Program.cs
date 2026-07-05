namespace AlvorKit.Script.MathsGen;

/// <summary>Command-line entry point for regenerating AlvorKit.Maths primitive package source.</summary>
internal static class Program
{
    /// <summary>Runs the generator with an optional <c>--output-root</c> directory override.</summary>
    public static int Main(string[] args)
    {
        try
        {
            var command = MathsGenOptions.CreateRootCommand(
                () => ProjectRoot.FindFromCurrentProcess(typeof(Program), requireResDirectory: true),
                Run);
            return command.Parse(args).Invoke(new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    /// <summary>Runs generation with parsed command-line options.</summary>
    private static int Run(MathsGenOptions options)
    {
        var repoRoot = ProjectRoot.FindFromCurrentProcess(typeof(Program), requireResDirectory: true);
        MathsGenerator.GenerateTo(options.OutputRoot, ReadPackageVersion(repoRoot));
        return 0;
    }

    /// <summary>Reads the package version pin shared with consumers.</summary>
    private static string ReadPackageVersion(string repoRoot)
    {
        var versionFile = Path.Combine(repoRoot, "src", "AlvorKit.Maths", "version", "VERSION");
        var value = File.Exists(versionFile) ? File.ReadAllText(versionFile).Trim() : "";
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("src/AlvorKit.Maths/version/VERSION must define the math package version.");

        return value;
    }
}
