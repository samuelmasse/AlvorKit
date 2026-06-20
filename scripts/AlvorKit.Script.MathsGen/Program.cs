namespace AlvorKit.Script.MathsGen;

/// <summary>Command-line entry point for regenerating AlvorKit.Maths primitive package source.</summary>
internal static class Program
{
    /// <summary>Runs the generator with an optional <c>--output-root</c> directory override.</summary>
    public static int Main(string[] args)
    {
        try
        {
            var repoRoot = ProjectRoot.FindFromCurrentProcess(typeof(Program), requireResDirectory: true);
            MathsGenerator.GenerateTo(ResolveOutputRoot(args, repoRoot), ReadPrimitivesVersion(repoRoot));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    /// <summary>Resolves the output root from the command line or the repository default.</summary>
    private static string ResolveOutputRoot(IReadOnlyList<string> args, string repoRoot)
    {
        if (args.Count == 0)
            return Path.Combine(repoRoot, "out", "mathgen");

        if (args.Count == 2 && args[0] is "--output-root" or "--output")
            return Path.GetFullPath(args[1]);

        throw new ArgumentException("Usage: dotnet run --project scripts/AlvorKit.Script.MathsGen -- [--output-root <directory>]");
    }

    /// <summary>Reads the package version pin shared with consumers.</summary>
    private static string ReadPrimitivesVersion(string repoRoot)
    {
        var props = XDocument.Load(Path.Combine(repoRoot, "AlvorKit.Packages.props"));
        var value = props.Descendants("MathsPrimitivesVer").SingleOrDefault()?.Value.Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("AlvorKit.Packages.props must define MathsPrimitivesVer.");

        return value;
    }
}
