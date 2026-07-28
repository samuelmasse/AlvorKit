namespace AlvorKit.Interception;

/// <summary>Cold-path diagnostic state for one interception backend.</summary>
public readonly record struct InterceptionBackendState(
    bool Ready,
    bool Stopping,
    uint PendingRequests,
    uint ActivePatches,
    uint RetainedCompletions,
    ulong LastRequestId);
