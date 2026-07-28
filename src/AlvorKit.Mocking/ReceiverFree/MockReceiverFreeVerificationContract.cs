namespace AlvorKit.Mocking;

/// <summary>Runs one receiver-free count check against its owning session ledger.</summary>
internal delegate void MockReceiverFreeVerify(
    MockReceiverFreeSetupDescriptor descriptor,
    MockVerificationCountKind kind,
    int expected,
    MockSession? session,
    MockCheckpoint after,
    MockCheckpoint through);

/// <summary>
/// Immutable receiver-free verification scope plus its session-owned executor.
/// </summary>
internal sealed class MockReceiverFreeVerificationContract
{
    private readonly MockReceiverFreeVerify verify;

    internal MockReceiverFreeVerificationContract(
        MockReceiverFreeSetupDescriptor descriptor,
        MockReceiverFreeVerify verify)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(verify);

        Descriptor = descriptor;
        this.verify = verify;
    }

    internal MockReceiverFreeSetupDescriptor Descriptor { get; }

    internal MockReceiverFreeVerificationContract AtSite(MockCallSite site) =>
        new(Descriptor.AtSite(site), verify);

    internal void Verify(
        MockVerificationCountKind kind,
        int expected,
        MockSession? session,
        MockCheckpoint after,
        MockCheckpoint through) =>
        verify(
            Descriptor,
            kind,
            expected,
            session,
            after,
            through);
}
