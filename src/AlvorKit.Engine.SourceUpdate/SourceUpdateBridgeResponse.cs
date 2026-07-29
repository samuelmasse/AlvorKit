namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Versioned Source Update bridge response envelope.</summary>
public sealed record SourceUpdateBridgeResponse(
    SourceUpdateCapabilities? Capabilities = null,
    SourceUpdateApplyResult? Apply = null);
