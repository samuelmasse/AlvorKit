namespace AlvorKit.Mocking;

/// <summary>Captures, compares, diagnoses, and marks exact logical invocation sequences.</summary>
internal static class MockSequenceVerification
{
    /// <summary>Verifies one exact sequence in a stable checkpoint window.</summary>
    internal static void Verify(
        MockSession session,
        MockCheckpoint after,
        MockCheckpoint through,
        Action[] expectedCalls)
    {
        var expected = CaptureExpected(expectedCalls);
        var actual = session.SnapshotBetween(after, through);
        var comparisonCount = Math.Min(expected.Length, actual.Length);

        for (var i = 0; i < comparisonCount; i++)
        {
            if (!expected[i].Matches(actual[i]))
            {
                throw new MockException(
                    MockDiagnostics.SequenceFailure(
                        i,
                        expected[i],
                        actual[i]));
            }
        }

        if (expected.Length != actual.Length)
        {
            var index = comparisonCount;
            throw new MockException(
                MockDiagnostics.SequenceFailure(
                    index,
                    index < expected.Length
                        ? expected[index]
                        : null,
                    index < actual.Length
                        ? actual[index]
                        : null));
        }

        MarkVerified(session, actual);
    }

    private static MockCapturedInvocation[] CaptureExpected(
        Action[] expectedCalls)
    {
        var expected = new MockCapturedInvocation[expectedCalls.Length];
        for (var i = 0; i < expectedCalls.Length; i++)
        {
            var action = expectedCalls[i] ??
                throw new ArgumentException(
                    "Expected sequence calls cannot contain null.",
                    nameof(expectedCalls));
            expected[i] = Capture.Run(
                CaptureOperation.Verification,
                action);
        }

        return expected;
    }

    private static void MarkVerified(
        MockSession session,
        ReadOnlySpan<MockInvocation> invocations)
    {
        foreach (IMockInvocationParticipant participant in session.Participants)
        {
            var snapshot = participant.Invocations.Snapshot();
            var ledgerInvocations = snapshot.Invocations;
            var indices = new int[ledgerInvocations.Length];
            var count = 0;

            for (var i = 0; i < ledgerInvocations.Length; i++)
            {
                var candidate = ledgerInvocations[i];
                for (var j = 0; j < invocations.Length; j++)
                {
                    if (candidate.Coordinate == invocations[j].Coordinate)
                    {
                        indices[count++] = i;
                        break;
                    }
                }
            }

            participant.Invocations.MarkVerifiedAtomically(
                snapshot,
                indices.AsSpan(0, count));
        }
    }

}
