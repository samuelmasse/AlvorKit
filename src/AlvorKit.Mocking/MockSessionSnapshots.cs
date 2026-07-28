namespace AlvorKit.Mocking;

/// <summary>Reads and validates stable invocation windows owned by a mock session.</summary>
internal static class MockSessionSnapshots
{
    /// <summary>Returns session-owned invocations through a stable checkpoint.</summary>
    internal static MockInvocation[] SnapshotThrough(
        this MockSession session,
        MockCheckpoint checkpoint) =>
        session.SnapshotBetween(
            new(session.Id, new(session.Timeline.Id, 0)),
            checkpoint);

    /// <summary>Returns session-owned invocations in one stable checkpoint window.</summary>
    internal static MockInvocation[] SnapshotBetween(
        this MockSession session,
        MockCheckpoint after,
        MockCheckpoint through)
    {
        session.ValidateWindow(after, through);

        var invocations = new List<MockInvocation>();
        foreach (IMockInvocationParticipant participant in session.Participants)
        {
            ReadOnlySpan<MockInvocation> snapshot =
                participant.Invocations.Snapshot().Invocations;
            for (var i = 0; i < snapshot.Length; i++)
            {
                MockInvocation invocation = snapshot[i];
                if (invocation.Coordinate.TimelineId ==
                        session.Timeline.Id &&
                    invocation.Coordinate.Sequence > after.Sequence &&
                    invocation.Coordinate.Sequence <= through.Sequence)
                {
                    invocations.Add(invocation);
                }
            }
        }

        invocations.Sort(
            static (left, right) =>
                left.Coordinate.Sequence.CompareTo(
                    right.Coordinate.Sequence));
        return [.. invocations];
    }

    /// <summary>Validates that a checkpoint belongs to this session.</summary>
    internal static void ValidateCheckpoint(
        this MockSession session,
        MockCheckpoint checkpoint)
    {
        session.ThrowIfDisposed();
        if (checkpoint.SessionId != session.Id ||
            checkpoint.TimelineId != session.Timeline.Id)
        {
            throw new MockException(MockDiagnostics.ForeignCheckpoint());
        }

        if (checkpoint.Sequence >
            session.Timeline.Checkpoint().Sequence)
        {
            throw new MockException(MockDiagnostics.FutureCheckpoint());
        }
    }

    /// <summary>Validates an ordered checkpoint window owned by this session.</summary>
    internal static void ValidateWindow(
        this MockSession session,
        MockCheckpoint after,
        MockCheckpoint through)
    {
        session.ValidateCheckpoint(after);
        session.ValidateCheckpoint(through);
        if (after.Sequence > through.Sequence)
        {
            throw new MockException(
                MockDiagnostics.ReversedCheckpointWindow());
        }
    }
}
