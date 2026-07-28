namespace AlvorKit.Mocking;

/// <summary>Orders invocation snapshots by their logical timeline coordinate.</summary>
internal sealed class MockInvocationSequenceComparer :
    IComparer<MockInvocation>
{
    /// <summary>Gets the reusable stateless comparer.</summary>
    internal static MockInvocationSequenceComparer Instance { get; } =
        new();

    /// <inheritdoc />
    public int Compare(
        MockInvocation? left,
        MockInvocation? right)
    {
        if (ReferenceEquals(left, right))
            return 0;
        if (left is null)
            return -1;
        if (right is null)
            return 1;

        var timeline = left.Coordinate.TimelineId.CompareTo(
            right.Coordinate.TimelineId);
        return timeline != 0
            ? timeline
            : left.Coordinate.Sequence.CompareTo(
                right.Coordinate.Sequence);
    }
}
