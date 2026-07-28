namespace AlvorKit.Mocking;

/// <summary>
/// Pins one deterministic ledger membership snapshot for matching and atomic
/// verified marking.
/// </summary>
internal sealed class MockInvocationLedgerSnapshot
{
    private readonly MockInvocationSlot[] slots;
    private readonly MockInvocation[] invocations;

    /// <summary>Creates a snapshot from slots already sorted by sequence.</summary>
    internal MockInvocationLedgerSnapshot(
        long ledgerId,
        MockHistoryEpoch epoch,
        MockInvocationSlot[] slots)
    {
        LedgerId = ledgerId;
        Epoch = epoch;
        this.slots = slots;
        invocations = SnapshotSlots(slots);
    }

    /// <summary>Gets the owning ledger ID.</summary>
    internal long LedgerId { get; }

    /// <summary>Gets the history epoch represented by this snapshot.</summary>
    internal MockHistoryEpoch Epoch { get; }

    /// <summary>Gets immutable invocation views in logical entry order.</summary>
    internal ReadOnlySpan<MockInvocation> Invocations => invocations;

    /// <summary>Gets one pinned slot for ledger-owned atomic marking.</summary>
    internal MockInvocationSlot SlotAt(int index) => slots[index];

    /// <summary>Copies pinned membership for a ledger-owned refresh.</summary>
    internal MockInvocationSlot[] CopySlots() => [.. slots];

    private static MockInvocation[] SnapshotSlots(MockInvocationSlot[] source)
    {
        var result = new MockInvocation[source.Length];
        for (var i = 0; i < source.Length; i++)
            result[i] = source[i].Snapshot();
        return result;
    }
}
