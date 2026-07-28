namespace AlvorKit.Mocking;

/// <summary>Applies a count constraint to one captured struct call.</summary>
public sealed class MockStructVerification
{
    private readonly MockStructVerificationContract contract;
    private readonly MockSession? windowSession;
    private readonly MockCheckpoint after;
    private readonly MockCheckpoint through;

    /// <summary>Creates one verification over an immutable struct contract.</summary>
    internal MockStructVerification(
        MockStructVerificationContract contract)
        : this(contract, null, default, default)
    {
    }

    private MockStructVerification(
        MockStructVerificationContract contract,
        MockSession? windowSession,
        MockCheckpoint after,
        MockCheckpoint through)
    {
        ArgumentNullException.ThrowIfNull(contract);
        this.contract = contract;
        this.windowSession = windowSession;
        this.after = after;
        this.through = through;
    }

    /// <summary>
    /// Restricts verification to entries after one checkpoint through another.
    /// </summary>
    public MockStructVerification Between(
        MockCheckpoint after,
        MockCheckpoint through)
    {
        MockSession current = MockSession.Current ??
            throw new MockException(
                MockDiagnostics.SessionMustBeCurrent(
                    "Checkpoint verification"));
        current.ValidateWindow(after, through);
        return new(contract, current, after, through);
    }

    /// <summary>Verifies that no matching invocation occurred.</summary>
    public void Never() =>
        Verify(MockVerificationCountKind.Exactly, 0);

    /// <summary>Verifies that exactly one matching invocation occurred.</summary>
    public void Once() =>
        Verify(MockVerificationCountKind.Exactly, 1);

    /// <summary>Verifies an exact non-negative invocation count.</summary>
    public void Exactly(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Verify(MockVerificationCountKind.Exactly, count);
    }

    /// <summary>Verifies a minimum non-negative invocation count.</summary>
    public void AtLeast(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Verify(MockVerificationCountKind.AtLeast, count);
    }

    /// <summary>Verifies a maximum non-negative invocation count.</summary>
    public void AtMost(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Verify(MockVerificationCountKind.AtMost, count);
    }

    private void Verify(
        MockVerificationCountKind kind,
        int expected)
    {
        windowSession?.ValidateWindow(after, through);
        contract.Verify(
            kind,
            expected,
            windowSession,
            after,
            through);
    }
}
