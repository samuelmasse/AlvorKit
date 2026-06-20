namespace AlvorKit.Script.AlvorSense;

/// <summary>Runs the private background host loop for one session directory.</summary>
/// <param name="SessionDir">Session directory containing the manifest and request mailbox.</param>
internal sealed record AlvorSenseHostCommand(string SessionDir) : AlvorSenseCommand;
