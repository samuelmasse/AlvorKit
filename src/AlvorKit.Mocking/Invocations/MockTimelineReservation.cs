namespace AlvorKit;

/// <summary>Reserves one logical sequence number before invocation publication.</summary>
internal readonly record struct MockTimelineReservation(long TimelineId, long Sequence);
