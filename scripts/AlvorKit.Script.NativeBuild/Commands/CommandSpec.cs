namespace AlvorKit.Script.NativeBuild;

/// <summary>Process invocation planned by the native build runner.</summary>
/// <param name="FileName">Executable file name or path.</param>
/// <param name="Arguments">Arguments passed without shell expansion.</param>
/// <param name="WorkingDirectory">Optional working directory for the process.</param>
/// <param name="CreateWorkingDirectory">True when the working directory should be created before running.</param>
/// <param name="Environment">Optional environment variable overrides for the child process.</param>
internal sealed record CommandSpec(
    string FileName,
    IReadOnlyList<string> Arguments,
    string? WorkingDirectory = null,
    bool CreateWorkingDirectory = false,
    IReadOnlyDictionary<string, string>? Environment = null);
