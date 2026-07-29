namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Versioned Source Update bridge request envelope.</summary>
public sealed record SourceUpdateBridgeRequest(
    string Operation,
    SourceUpdateApplyRequest? Apply = null);
