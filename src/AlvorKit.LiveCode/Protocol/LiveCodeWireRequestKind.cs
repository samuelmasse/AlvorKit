namespace AlvorKit.LiveCode;

/// <summary>Identifies one operation in the private LiveCode loopback protocol.</summary>
internal enum LiveCodeWireRequestKind
{
    Graph,
    References,
    Execute,
    FrozenInspectionStatus,
    FrozenInspectionExecute,
    Bridges,
    Bridge,
    BridgeEnqueue,
    BridgeOperationStatus
}
