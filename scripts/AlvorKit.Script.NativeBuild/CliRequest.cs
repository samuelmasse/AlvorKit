namespace AlvorKit.Script.NativeBuild;

/// <summary>Parsed command-line request with values normalized for execution.</summary>
internal sealed class CliRequest
{
    /// <summary>Creates a parsed request.</summary>
    public CliRequest(CliCommand command, string? selection, string? rid, bool showHelp)
    {
        Command = command;
        Selection = selection;
        Rid = rid;
        ShowHelp = showHelp;
    }

    /// <summary>Command selected by the user.</summary>
    public CliCommand Command { get; }

    /// <summary>Library name or all selection used by commands that need a library.</summary>
    public string? Selection { get; }

    /// <summary>Optional runtime identifier supplied to the build command.</summary>
    public string? Rid { get; }

    /// <summary>True when the request should print usage instead of doing work.</summary>
    public bool ShowHelp { get; }
}
