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
            var options = LintOptions.Parse(args);
            if (options.ShowHelp)
            {
                Console.WriteLine(LintOptions.HelpText);
                return 0;
            }

            var runner = new LintRunner(options, new ProcessRunner(), new ActionlintTool());
            return await runner.RunAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
