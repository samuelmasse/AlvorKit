namespace AlvorKit;

/// <summary>
/// Runs one struct invocation count check against its owning session ledger.
/// </summary>
internal delegate void MockStructVerify(
    MockStructSetupDescriptor descriptor,
    MockVerificationCountKind kind,
    int expected,
    MockSession? windowSession,
    MockCheckpoint after,
    MockCheckpoint through);

/// <summary>
/// Immutable struct verification scope plus its session-owned executor.
/// </summary>
internal sealed class MockStructVerificationContract
{
    private readonly MockStructVerify verify;

    internal MockStructVerificationContract(
        MockStructSetupDescriptor descriptor,
        MockStructVerify verify)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(verify);
        Descriptor = descriptor;
        this.verify = verify;
    }

    internal MockStructSetupDescriptor Descriptor { get; }

    internal void Verify(
        MockVerificationCountKind kind,
        int expected,
        MockSession? windowSession,
        MockCheckpoint after,
        MockCheckpoint through) =>
        verify(
            Descriptor,
            kind,
            expected,
            windowSession,
            after,
            through);
}
