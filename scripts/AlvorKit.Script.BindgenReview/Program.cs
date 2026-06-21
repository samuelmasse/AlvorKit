namespace AlvorKit.Script.BindgenReview;

/// <summary>Entry point for the bindgen generated-output review helper.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs a bindgen review command and returns a process exit code.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var root = BindgenReviewCommandParser.CreateRootCommand(
                () => ProjectRoot.FindFromCurrentProcess(typeof(BindgenReviewCommandParser)),
                ExecuteAsync);
            return await root.Parse(args).InvokeAsync(new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>Executes one parsed bindgen review command.</summary>
    private static async Task<int> ExecuteAsync(BindgenReviewCommand command)
    {
        var result = await BindgenReviewCoordinator.CreateDefault().ExecuteAsync(command);
        foreach (var line in result.Lines)
            Console.WriteLine(line);

        return result.ExitCode;
    }
}
