namespace AlvorKit.Script.NativeBuild;

/// <summary>Runs external processes for build and verification steps.</summary>
internal interface IProcessRunner
{
    /// <summary>Runs a process and throws when it returns a non-zero exit code.</summary>
    Task RunAsync(CommandSpec command);

    /// <summary>Runs a process and returns captured standard output.</summary>
    Task<string> CaptureAsync(CommandSpec command);
}
