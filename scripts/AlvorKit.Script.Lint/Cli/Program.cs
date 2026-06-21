namespace AlvorKit.Script.Lint;

/// <summary>Entry point for the repository lint coordinator.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs the configured lint checks and returns a process exit code.</summary>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var command = LintOptions.CreateRootCommand(() => RepositoryPaths.FindRoot(), RunAsync);
            return await command.Parse(args).InvokeAsync(new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>Runs linting with parsed command-line options.</summary>
    private static async Task<int> RunAsync(LintOptions options)
    {
        var runner = new LintRunner(options, new ProcessRunner(), new ActionlintTool());
        return await runner.RunAsync();
    }
}
