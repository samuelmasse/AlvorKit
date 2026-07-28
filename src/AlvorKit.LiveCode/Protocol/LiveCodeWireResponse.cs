namespace AlvorKit.LiveCode;

/// <summary>One response returned by a running LiveCode host.</summary>
internal sealed record LiveCodeWireResponse(
    bool Ok,
    string? Error = null,
    LiveCodeScopeGraph? Graph = null,
    LiveCodeReferenceManifest? References = null,
    LiveCodeExecutionResult? Execution = null,
    LiveCodeFrozenInspectionSnapshot? FrozenInspection = null,
    LiveCodeFrozenInspectionExecutionResult? FrozenExecution = null,
    LiveCodeBridgeDescriptor[]? Bridges = null,
    LiveCodeBridgeExecutionResult? BridgeExecution = null);
