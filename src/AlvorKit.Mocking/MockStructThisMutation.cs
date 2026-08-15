namespace AlvorKit;

/// <summary>Identifies one synchronous writable-<c>this</c> mutation.</summary>
internal readonly record struct MockStructThisMutation(
    MockSnapshotPhase Phase,
    Delegate Mutation);
