namespace AlvorKit.Script.AlvorSense;

/// <summary>Starts a persistent AlvorSense session for one target project.</summary>
/// <param name="Id">Stable session id used for later send and stop commands.</param>
/// <param name="Project">Project file to run under the agent windowing backend.</param>
/// <param name="WorkingDirectory">Working directory for the hosted game process.</param>
/// <param name="Environment">Extra environment variables passed to the hosted game process.</param>
/// <param name="Timeout">Maximum time to wait for the host to become ready.</param>
internal sealed record AlvorSenseStartCommand(
    string Id,
    string Project,
    string WorkingDirectory,
    IReadOnlyDictionary<string, string> Environment,
    TimeSpan Timeout) : AlvorSenseCommand
{
    /// <summary>Creates the persistent session manifest used by the host process.</summary>
    internal AlvorSenseSessionManifest ToManifest() =>
        new(Id, Project, WorkingDirectory, new Dictionary<string, string>(Environment));
}
