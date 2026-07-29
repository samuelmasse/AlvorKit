namespace AlvorKit.LiveCode;

/// <summary>One authenticated request sent to a running LiveCode host.</summary>
internal sealed record LiveCodeWireRequest(
    string Token,
    LiveCodeWireRequestKind Kind,
    long ScopeId = 0,
    string? EntryType = null,
    byte[]? Assembly = null,
    byte[]? Symbols = null,
    string? Bridge = null,
    int BridgeVersion = 0,
    JsonElement? Payload = null,
    string? OperationId = null);
