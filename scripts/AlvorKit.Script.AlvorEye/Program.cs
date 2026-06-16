namespace AlvorKit.Script.AlvorEye;

/// <summary>Entry point for the AlvorEye desktop game automation helper.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs an AlvorEye command and returns a process exit code.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var command = AlvorEyeCommandParser.Parse(args);
            var result = await AlvorEyeCoordinator.CreateDefault().ExecuteAsync(command, Console.In, Console.Out);
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
