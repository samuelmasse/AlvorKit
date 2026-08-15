namespace AlvorKit;

/// <summary>Immediate acknowledgment that a bridge operation has been reserved and queued.</summary>
public sealed record LiveCodeBridgeEnqueueResponse(
    string OperationId,
    LiveCodeBridgeOperationState State,
    string Status);
