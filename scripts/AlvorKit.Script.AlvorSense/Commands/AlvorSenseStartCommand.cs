namespace AlvorKit.Script.AlvorSense;

/// <summary>Starts a persistent AlvorSense session for one project or prebuilt assembly.</summary>
/// <param name="Id">Stable session id used for later send and stop commands.</param>
/// <param name="Project">Project file to run, or <see langword="null" /> for a prebuilt assembly.</param>
/// <param name="WorkingDirectory">Working directory for the hosted game process.</param>
/// <param name="Environment">Extra environment variables passed to the hosted game process.</param>
/// <param name="Timeout">Maximum time to wait for the host to become ready.</param>
/// <param name="Assembly">Prebuilt managed assembly to run, or <see langword="null" /> for a project.</param>
/// <param name="EditableProject">Project to build into an immutable Source Update launch, or <see langword="null" />.</param>
internal sealed record AlvorSenseStartCommand(
    string Id,
    string? Project,
    string WorkingDirectory,
    IReadOnlyDictionary<string, string> Environment,
    TimeSpan Timeout,
    string? Assembly = null,
    string? EditableProject = null) : AlvorSenseCommand
{
    /// <summary>Creates the persistent session manifest used by the host process.</summary>
    internal AlvorSenseSessionManifest ToManifest() =>
        new(
            Id,
            Project,
            WorkingDirectory,
            new Dictionary<string, string>(Environment))
        {
            Assembly = Assembly,
            EditableProject = EditableProject,
        };
}
