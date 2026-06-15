namespace AlvorKit.Script.Lint;

/// <summary>Process invocation planned by the lint coordinator.</summary>
/// <param name="FileName">Executable file name or absolute path.</param>
/// <param name="Arguments">Arguments passed without shell expansion.</param>
/// <param name="WorkingDirectory">Working directory for the process.</param>
/// <param name="Label">Short display label for console progress output.</param>
internal sealed record CommandSpec(
    string FileName,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory,
    string Label = "");
