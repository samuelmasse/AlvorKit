namespace AlvorKit;

/// <summary>Identifies one replaceable invocation-history segment.</summary>
internal readonly record struct MockHistoryEpoch(long OwnerId, long Number);
