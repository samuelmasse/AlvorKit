namespace AlvorKit.Script.AlvorSense;

/// <summary>Stops a running AlvorSense session and its hosted game process.</summary>
/// <param name="Id">Session id selected by the start command.</param>
/// <param name="Timeout">Maximum time to wait for a stop response.</param>
internal sealed record AlvorSenseStopCommand(string Id, TimeSpan Timeout) : AlvorSenseCommand;
