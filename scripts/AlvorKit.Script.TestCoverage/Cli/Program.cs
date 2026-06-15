namespace AlvorKit.Script.TestCoverage;

/// <summary>Entry point for the repository coverage reporting tool.</summary>
internal static class Program
{
    /// <summary>Runs coverage collection and returns a process exit code.</summary>
    public static async Task<int> Main(string[] args)
    {
        var options = CoverageOptions.Parse(args);
        var runner = new CoverageRunner(options);
        return await runner.RunAsync();
    }
}
