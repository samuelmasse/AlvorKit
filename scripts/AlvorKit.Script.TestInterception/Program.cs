namespace AlvorKit;

/// <summary>Entry point for isolated Interception-profiler child launches.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Parses launcher options and returns the child exit code.</summary>
    internal static async Task<int> Main(string[] args)
    {
        var (launcherArguments, childArguments) =
            InterceptionCommandLine.Split(args);
        var command = InterceptionOptionsParser.CreateRootCommand(
            childArguments,
            static options => new InterceptionLauncher().RunAsync(options, CancellationToken.None));
        return await command.Parse(launcherArguments).InvokeAsync(
            new() { EnableDefaultExceptionHandler = false });
    }
}
