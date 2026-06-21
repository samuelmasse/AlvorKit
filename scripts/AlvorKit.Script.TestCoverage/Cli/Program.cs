namespace AlvorKit.Script.TestCoverage;

/// <summary>Entry point for the repository coverage reporting tool.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs coverage collection and returns a process exit code.</summary>
    public static async Task<int> Main(string[] args)
    {
        var command = CoverageOptionsParser.CreateRootCommand(RunAsync);
        return await command.Parse(args).InvokeAsync(new() { EnableDefaultExceptionHandler = false });
    }

    /// <summary>Runs coverage with parsed command-line options.</summary>
    private static async Task<int> RunAsync(CoverageOptions options)
    {
        var runner = new CoverageRunner(options);
        return await runner.RunAsync();
    }
}
