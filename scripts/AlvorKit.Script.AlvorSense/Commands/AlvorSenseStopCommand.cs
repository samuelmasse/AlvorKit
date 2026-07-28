namespace AlvorKit.Script.AlvorSense;

/// <summary>Stops a running AlvorSense session and its hosted game process.</summary>
/// <param name="Id">Session id selected by the start command.</param>
/// <param name="Timeout">Maximum time to wait for a stop response.</param>
/// <param name="Workspace">Optional live workspace receiving the exact stop request and result.</param>
internal sealed record AlvorSenseStopCommand(
    string Id,
    TimeSpan Timeout,
    string? Workspace = null) : AlvorSenseCommand;
