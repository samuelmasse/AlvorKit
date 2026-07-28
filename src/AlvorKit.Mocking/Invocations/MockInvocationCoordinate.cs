namespace AlvorKit.Mocking;

/// <summary>Identifies one invocation's logical position on a timeline.</summary>
internal readonly record struct MockInvocationCoordinate(long TimelineId, long Sequence);
