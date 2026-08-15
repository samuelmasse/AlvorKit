namespace AlvorKit;

/// <summary>Versioned limits and features of the connected profiler.</summary>
public readonly record struct InterceptionCapabilities(
    InterceptionCapability Flags,
    uint MaximumIlBodyBytes,
    uint MaximumPendingRequests,
    uint MaximumActivePatches,
    uint MaximumMetadataBytes = 0,
    uint MaximumRelocations = 0,
    uint MaximumIlMapEntries = 0);
