namespace AlvorKit;

/// <summary>Owns invocation slots for one history epoch.</summary>
internal sealed class MockHistorySegment(MockHistoryEpoch epoch)
{
    private readonly List<MockInvocationSlot> slots = [];

    /// <summary>Gets this segment's history epoch.</summary>
    internal MockHistoryEpoch Epoch => epoch;

    /// <summary>Adds one invocation slot while the ledger lock is held.</summary>
    internal void Add(MockInvocationSlot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        slots.Add(slot);
    }

    /// <summary>Copies slots into deterministic logical-sequence order.</summary>
    internal MockInvocationSlot[] CopySlots()
    {
        var result = slots.ToArray();
        Array.Sort(
            result,
            static (left, right) =>
                left.Coordinate.Sequence.CompareTo(right.Coordinate.Sequence));
        return result;
    }
}
