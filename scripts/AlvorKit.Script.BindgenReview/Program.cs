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
            var command = BindgenReviewCommandParser.Parse(args);
            var result = await BindgenReviewCoordinator.CreateDefault().ExecuteAsync(command);
            foreach (var line in result.Lines)
                Console.WriteLine(line);

            return result.ExitCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
