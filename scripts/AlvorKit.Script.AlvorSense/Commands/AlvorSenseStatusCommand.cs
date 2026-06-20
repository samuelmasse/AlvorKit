namespace AlvorKit.Script.AlvorSense;

/// <summary>Reads the persisted state of one known AlvorSense session.</summary>
/// <param name="Id">Session id selected by the start command.</param>
internal sealed record AlvorSenseStatusCommand(string Id) : AlvorSenseCommand;
