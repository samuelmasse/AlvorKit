namespace AlvorKit.Mocking;

/// <summary>Queries immutable projector selections used by typed matching.</summary>
internal static class MockTypedMatcherProjectors
{
    /// <summary>Gets whether a selection contains an exit-phase projector.</summary>
    internal static bool HasExit(
        ReadOnlySpan<MockSnapshotProjector> projectors)
    {
        foreach (MockSnapshotProjector projector in projectors)
        {
            if (projector.Phase == MockSnapshotPhase.Exit)
                return true;
        }

        return false;
    }
}
