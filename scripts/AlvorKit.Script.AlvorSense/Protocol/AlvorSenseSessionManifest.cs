namespace AlvorKit.Script.AlvorSense;

/// <summary>Manifest persisted when a session starts so the background host can launch the target.</summary>
/// <param name="Id">Stable session id used by subsequent commands.</param>
/// <param name="Project">Project file to run, or <see langword="null" /> for a prebuilt assembly.</param>
/// <param name="WorkingDirectory">Working directory for the hosted game process.</param>
/// <param name="Environment">Extra environment variables passed to the hosted game process.</param>
internal sealed record AlvorSenseSessionManifest(
    string Id,
    string? Project,
    string WorkingDirectory,
    Dictionary<string, string> Environment)
{
    /// <summary>Gets the prebuilt managed assembly to run instead of a project.</summary>
    public string? Assembly { get; init; }
}
