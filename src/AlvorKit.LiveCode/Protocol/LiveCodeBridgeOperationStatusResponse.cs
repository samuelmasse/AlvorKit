namespace AlvorKit.LiveCode;

/// <summary>Out-of-band status for one accepted two-phase bridge operation.</summary>
public sealed record LiveCodeBridgeOperationStatusResponse(
    string OperationId,
    LiveCodeBridgeOperationState State,
    DateTimeOffset AcceptedUtc,
    DateTimeOffset? StartedUtc,
    DateTimeOffset? CompletedUtc,
    LiveCodeBridgeExecutionResult? Result);
