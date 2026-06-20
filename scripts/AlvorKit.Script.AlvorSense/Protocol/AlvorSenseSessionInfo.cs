namespace AlvorKit.Script.AlvorSense;

/// <summary>Agent-readable summary of one persisted session directory.</summary>
/// <param name="Id">Session id matching the directory name.</param>
/// <param name="SessionDir">Session directory path.</param>
/// <param name="Ready">Whether the session has a ready marker.</param>
/// <param name="ProcessId">Background host process id from the ready marker when available.</param>
/// <param name="RequestCount">Number of committed request files.</param>
/// <param name="ResponseCount">Number of response files.</param>
/// <param name="LastWriteTimeUtc">Most recent session directory write time in UTC.</param>
internal sealed record AlvorSenseSessionInfo(
    string Id,
    string SessionDir,
    bool Ready,
    int? ProcessId,
    int RequestCount,
    int ResponseCount,
    DateTime LastWriteTimeUtc);
