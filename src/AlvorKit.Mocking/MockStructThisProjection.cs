namespace AlvorKit.Mocking;

/// <summary>Identifies one heap-safe projection of live <c>this</c>.</summary>
internal readonly record struct MockStructThisProjection(
    MockSnapshotPhase Phase,
    MockSnapshotProjector Projector);
