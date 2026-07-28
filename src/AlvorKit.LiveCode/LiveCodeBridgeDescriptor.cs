namespace AlvorKit.LiveCode;

/// <summary>Discoverable identity, policy, and request schema for one predefined LiveCode bridge.</summary>
public sealed record LiveCodeBridgeDescriptor(
    string Name,
    int Version,
    string Description,
    bool MutatesState,
    LiveCodeBridgeLease Lease,
    JsonElement RequestSchema);
