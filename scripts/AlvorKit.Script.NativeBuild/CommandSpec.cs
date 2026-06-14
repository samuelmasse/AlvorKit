namespace AlvorKit.Script.NativeBuild;

/// <summary>Process invocation planned by the native build runner.</summary>
internal sealed class CommandSpec
{
    /// <summary>Creates a process invocation.</summary>
    public CommandSpec(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        bool createWorkingDirectory = false)
    {
        FileName = fileName;
        Arguments = arguments;
        WorkingDirectory = workingDirectory;
        CreateWorkingDirectory = createWorkingDirectory;
    }

    /// <summary>Executable file name or path.</summary>
    public string FileName { get; }

    /// <summary>Arguments passed without shell expansion.</summary>
    public IReadOnlyList<string> Arguments { get; }

    /// <summary>Optional working directory for the process.</summary>
    public string? WorkingDirectory { get; }

    /// <summary>True when the working directory should be created before running.</summary>
    public bool CreateWorkingDirectory { get; }
}
