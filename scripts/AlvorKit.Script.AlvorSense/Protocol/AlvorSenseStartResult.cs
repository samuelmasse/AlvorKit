namespace AlvorKit;

/// <summary>Structured foreground output produced after starting a session.</summary>
/// <param name="Id">Stable session id used by subsequent commands.</param>
/// <param name="SessionDir">Session directory containing the mailbox, logs, and artifacts.</param>
/// <param name="Ready">Whether the host wrote its ready marker before start returned.</param>
/// <param name="ProcessId">Background host process id from the ready marker when available.</param>
/// <param name="EditableLaunchManifestPath">
/// Exact editable-launch identity manifest when the session was started from a project.
/// </param>
internal sealed record AlvorSenseStartResult(
    string Id,
    string SessionDir,
    bool Ready,
    int? ProcessId,
    string? EditableLaunchManifestPath = null);
