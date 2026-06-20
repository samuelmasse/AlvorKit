namespace AlvorKit.Script.AlvorSense;

/// <summary>Prints generated CLI help without contacting a session.</summary>
/// <param name="Args">Arguments to parse when rendering contextual help.</param>
internal sealed record AlvorSenseHelpCommand(string[] Args) : AlvorSenseCommand;
