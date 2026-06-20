namespace AlvorKit.Script.AlvorSense;

/// <summary>Manifest persisted when a session starts so the background host can launch the target project.</summary>
/// <param name="Id">Stable session id used by subsequent commands.</param>
/// <param name="Project">Project file to run under the agent windowing backend.</param>
/// <param name="WorkingDirectory">Working directory for the hosted game process.</param>
/// <param name="Environment">Extra environment variables passed to the hosted game process.</param>
internal sealed record AlvorSenseSessionManifest(
    string Id,
    string Project,
    string WorkingDirectory,
    Dictionary<string, string> Environment);
