namespace AlvorKit.Script.DevSolution;

/// <summary>Entry point for the local development solution generator.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Generates a combined consumer and AlvorKit solution and returns a process exit code.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var command = DevSolutionOptions.CreateRootCommand(
                SolutionDefaults.FindConsumerSolution,
                SolutionDefaults.FindEngineSolution,
                RunAsync);
            return await command.Parse(args).InvokeAsync(new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>Runs the generator with parsed command-line options.</summary>
    private static Task<int> RunAsync(DevSolutionOptions options)
    {
        var result = new DevSolutionGenerator().Generate(options);
        var verb = result.Changed ? "Wrote" : "Unchanged";
        Console.WriteLine(
            $"{verb} {result.OutputPath} ({result.ConsumerProjectCount} consumer projects, {result.EngineProjectCount} engine projects).");
        return Task.FromResult(0);
    }
}
