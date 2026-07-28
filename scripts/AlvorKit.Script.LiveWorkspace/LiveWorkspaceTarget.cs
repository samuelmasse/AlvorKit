namespace AlvorKit.Script.LiveWorkspace;

/// <summary>Stable public identity of the LiveCode process associated with a workspace.</summary>
/// <param name="SessionId">Immutable LiveCode session identifier.</param>
/// <param name="Name">Advertised LiveCode session name.</param>
/// <param name="ProcessId">Operating-system process identifier.</param>
/// <param name="StartedUtc">UTC process start time observed by LiveCode.</param>
public sealed record LiveWorkspaceTarget(
    string SessionId,
    string Name,
    int ProcessId,
    DateTimeOffset StartedUtc);
