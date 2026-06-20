namespace AlvorKit.Script.AlvorSense;

/// <summary>One filesystem-mailbox request from the foreground CLI to the background host.</summary>
/// <param name="Id">Unique request id used to match responses.</param>
/// <param name="Commands">Command lines to write to the hosted game process.</param>
/// <param name="Stop">Whether the host should stop the target after handling the request.</param>
/// <param name="AppendState">Whether the host should request a fresh state line after the command batch.</param>
internal sealed record AlvorSenseRequest(string Id, string[] Commands, bool Stop, bool AppendState);
