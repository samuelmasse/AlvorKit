namespace AlvorKit;

/// <summary>Identifies one pending ledger slot through dispatch completion.</summary>
internal sealed class MockInvocationToken
{
    /// <summary>Creates a token that pins its invocation and history segment.</summary>
    internal MockInvocationToken(
        long ledgerId,
        MockHistorySegment segment,
        MockInvocationSlot slot)
    {
        LedgerId = ledgerId;
        Segment = segment;
        Slot = slot;
    }

    /// <summary>Gets the owning ledger ID.</summary>
    internal long LedgerId { get; }

    /// <summary>Gets the history segment pinned by this invocation.</summary>
    internal MockHistorySegment Segment { get; }

    /// <summary>Gets the mutable invocation slot.</summary>
    internal MockInvocationSlot Slot { get; }

    /// <summary>Gets the logical timeline coordinate.</summary>
    internal MockInvocationCoordinate Coordinate => Slot.Coordinate;

    /// <summary>Gets the epoch entered by this invocation.</summary>
    internal MockHistoryEpoch Epoch => Segment.Epoch;
}
