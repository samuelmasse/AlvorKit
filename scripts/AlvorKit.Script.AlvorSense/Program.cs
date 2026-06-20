namespace AlvorKit.Script.AlvorSense;

/// <summary>Entry point for persistent AlvorSense sessions.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs an AlvorSense script command and returns a process exit code.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public static int Main(string[] args) =>
        AlvorSenseCli.Run(args, Console.In, Console.Out, Console.Error);
}
