namespace AlvorKit.Script.AlvorSense;

/// <summary>Sends one batch of interactive commands to a running AlvorSense session.</summary>
/// <param name="Id">Session id selected by the start command.</param>
/// <param name="Commands">Command lines to write to the hosted game process.</param>
/// <param name="Timeout">Maximum time to wait for a response from the host.</param>
internal sealed record AlvorSenseSendCommand(string Id, string[] Commands, TimeSpan Timeout) : AlvorSenseCommand;
