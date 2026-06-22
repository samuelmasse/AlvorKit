namespace AlvorKit.Script.AlvorSense;

/// <summary>Sends one batch of interactive commands to a running AlvorSense session.</summary>
/// <param name="Id">Session id selected by the start command.</param>
/// <param name="Commands">Command lines to write to the hosted game process.</param>
/// <param name="Timeout">Maximum time to wait for a response from the host.</param>
/// <param name="StderrTailLines">Number of stderr lines to include when a failed send observes target exit.</param>
internal sealed record AlvorSenseSendCommand(string Id, string[] Commands, TimeSpan Timeout, int StderrTailLines) : AlvorSenseCommand;
