namespace AlvorKit;

/// <summary>Applies a count constraint to one captured mocked call.</summary>
public sealed class MockVerification
{
    private readonly MockCapturedInvocation? captured;
    private readonly MockReceiverFreeVerificationContract? receiverFree;
    private readonly MockSession? session;
    private readonly MockCheckpoint after;
    private readonly MockCheckpoint through;

    /// <summary>Creates a verification clause for one captured call.</summary>
    internal MockVerification(MockCapturedInvocation captured)
    {
        this.captured = captured;
    }

    /// <summary>Creates a verification clause for one receiver-free operation.</summary>
    internal MockVerification(
        MockReceiverFreeVerificationContract receiverFree)
    {
        ArgumentNullException.ThrowIfNull(receiverFree);
        this.receiverFree = receiverFree;
    }

    private MockVerification(
        MockCapturedInvocation? captured,
        MockReceiverFreeVerificationContract? receiverFree,
        MockSession? session,
        MockCheckpoint after,
        MockCheckpoint through)
    {
        this.captured = captured;
        this.receiverFree = receiverFree;
        this.session = session;
        this.after = after;
        this.through = through;
    }

    /// <summary>Restricts this receiver-free verification to one exact call site.</summary>
    public MockVerification AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        if (receiverFree is null)
        {
            throw new MockException(
                "Call-site verification applies only to receiver-free interception operations.");
        }

        return new(
            captured,
            receiverFree.AtSite(site),
            session,
            after,
            through);
    }

    /// <summary>Restricts verification to entries after one checkpoint through another.</summary>
    public MockVerification Between(
        MockCheckpoint after,
        MockCheckpoint through)
    {
        var current = MockSession.Current ??
            throw new MockException(
                MockDiagnostics.SessionMustBeCurrent(
                    "Checkpoint verification"));
        current.ValidateWindow(after, through);

        return new(
            captured,
            receiverFree,
            current,
            after,
            through);
    }

    /// <summary>Verifies that no matching invocation occurred.</summary>
    public void Never() => Verify(MockVerificationCountKind.Exactly, 0);

    /// <summary>Verifies that exactly one matching invocation occurred.</summary>
    public void Once() => Verify(MockVerificationCountKind.Exactly, 1);

    /// <summary>Verifies that exactly the requested number of invocations occurred.</summary>
    public void Exactly(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Verify(MockVerificationCountKind.Exactly, count);
    }

    /// <summary>Verifies that at least the requested number of invocations occurred.</summary>
    public void AtLeast(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Verify(MockVerificationCountKind.AtLeast, count);
    }

    /// <summary>Verifies that at most the requested number of invocations occurred.</summary>
    public void AtMost(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Verify(MockVerificationCountKind.AtMost, count);
    }

    private void Verify(MockVerificationCountKind kind, int expected)
    {
        session?.ValidateWindow(after, through);
        if (receiverFree is not null)
        {
            receiverFree.Verify(
                kind,
                expected,
                session,
                after,
                through);
            return;
        }

        var instanceCapture = captured ??
            throw new UnreachableException(
                "A verification clause has no captured operation.");
        var snapshot = instanceCapture.Mocked.Invocations.Snapshot();
        var invocations = snapshot.Invocations;
        var matchingIndices = new int[invocations.Length];
        var matchingCount = 0;

        for (var i = 0; i < invocations.Length; i++)
        {
            if (IsInWindow(invocations[i]) &&
                instanceCapture.Matches(invocations[i]))
            {
                matchingIndices[matchingCount++] = i;
            }
        }

        var succeeded = kind switch
        {
            MockVerificationCountKind.Exactly => matchingCount == expected,
            MockVerificationCountKind.AtLeast => matchingCount >= expected,
            MockVerificationCountKind.AtMost => matchingCount <= expected,
            _ => throw new UnreachableException($"Unknown count constraint '{kind}'.")
        };

        if (!succeeded)
            throw CreateFailure(kind, expected, matchingCount, invocations);

        instanceCapture.Mocked.Invocations.MarkVerifiedAtomically(
            snapshot,
            matchingIndices.AsSpan(0, matchingCount));
    }

    private bool IsInWindow(MockInvocation invocation)
    {
        if (session is null)
            return true;

        var coordinate = invocation.Coordinate;
        return coordinate.TimelineId == after.TimelineId &&
            coordinate.Sequence > after.Sequence &&
            coordinate.Sequence <= through.Sequence;
    }

    private MockException CreateFailure(
        MockVerificationCountKind kind,
        int expected,
        int observed,
        ReadOnlySpan<MockInvocation> invocations)
    {
        var candidates = new MockInvocation[invocations.Length];
        var candidateCount = 0;
        for (var i = 0; i < invocations.Length; i++)
        {
            if (IsInWindow(invocations[i]) &&
                captured!.IsSameOperation(invocations[i]))
            {
                candidates[candidateCount++] = invocations[i];
            }
        }

        return new(
            MockDiagnostics.CountFailure(
                captured!,
                kind,
                expected,
                observed,
                candidates.AsSpan(0, candidateCount)));
    }
}
