namespace AlvorKit.Script.LiveWorkspace;

/// <summary>Location and sequence assigned to one exact live-operation record.</summary>
/// <param name="EventId">Monotonic sequence number assigned within the workspace.</param>
/// <param name="Operation">Safe operation name used in the event directory.</param>
/// <param name="EventPath">Absolute directory containing request and result artifacts.</param>
public sealed record LiveWorkspaceEventResult(
    int EventId,
    string Operation,
    string EventPath);
