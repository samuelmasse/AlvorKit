namespace AlvorKit;

/// <summary>Reads the persisted state of one known AlvorSense session.</summary>
/// <param name="Id">Session id selected by the start command.</param>
/// <param name="Workspace">Optional live workspace receiving the exact status request and result.</param>
internal sealed record AlvorSenseStatusCommand(
    string Id,
    string? Workspace = null) : AlvorSenseCommand;
