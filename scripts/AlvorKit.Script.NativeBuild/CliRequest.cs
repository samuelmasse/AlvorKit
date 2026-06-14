namespace AlvorKit.Script.NativeBuild;

/// <summary>Parsed command-line request with values normalized for execution.</summary>
/// <param name="Command">Command selected by the user.</param>
/// <param name="Selection">Library name or all selection used by commands that need a library.</param>
/// <param name="Rid">Optional runtime identifier supplied to the build command.</param>
/// <param name="ShowHelp">True when the request should print usage instead of doing work.</param>
internal sealed record CliRequest(CliCommand Command, string? Selection, string? Rid, bool ShowHelp);
