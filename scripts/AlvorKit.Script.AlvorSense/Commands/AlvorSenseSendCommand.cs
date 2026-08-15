namespace AlvorKit;

/// <summary>Sends one batch of interactive commands to a running AlvorSense session.</summary>
/// <param name="Id">Session id selected by the start command.</param>
/// <param name="Commands">Command lines to write to the hosted game process.</param>
/// <param name="Timeout">Maximum time to wait for a response from the host.</param>
/// <param name="StderrTailLines">Number of stderr lines to include when a failed send observes target exit.</param>
/// <param name="Workspace">Optional live workspace receiving the exact command batch and result.</param>
internal sealed record AlvorSenseSendCommand(
    string Id,
    string[] Commands,
    TimeSpan Timeout,
    int StderrTailLines,
    string? Workspace = null) : AlvorSenseCommand;
