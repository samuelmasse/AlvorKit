namespace AlvorKit.Mocking;

/// <summary>Identifies a stable logical cutoff within one mock session.</summary>
public readonly record struct MockCheckpoint
{
    /// <summary>Creates a checkpoint owned by one session timeline.</summary>
    internal MockCheckpoint(long sessionId, MockInvocationCoordinate coordinate)
    {
        SessionId = sessionId;
        TimelineId = coordinate.TimelineId;
        Sequence = coordinate.Sequence;
    }

    /// <summary>Gets the owning session identity.</summary>
    internal long SessionId { get; }

    /// <summary>Gets the owning logical timeline identity.</summary>
    internal long TimelineId { get; }

    /// <summary>Gets the last entry published before this checkpoint.</summary>
    internal long Sequence { get; }
}
