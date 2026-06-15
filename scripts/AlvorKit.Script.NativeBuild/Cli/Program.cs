namespace AlvorKit.Script.NativeBuild;

/// <summary>Entry point for the native package build runner.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs the command-line interface and returns the process exit code.</summary>
    public static Task<int> Main(string[] args) =>
        new NativeBuildCli().RunAsync(args);
}
