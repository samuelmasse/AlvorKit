namespace AlvorKit;

/// <summary>Machine-readable identity, progress, and cleanup state for one agent live-debug workspace.</summary>
/// <param name="SchemaVersion">Workspace manifest schema version.</param>
/// <param name="Id">Safe workspace identifier.</param>
/// <param name="Purpose">Human-readable reason for the live session.</param>
/// <param name="Status">Current workspace lifecycle state.</param>
/// <param name="RepositoryRoot">Absolute repository root that owns the workspace.</param>
/// <param name="WorkspacePath">Absolute workspace directory.</param>
/// <param name="CreatedUtc">UTC creation time.</param>
/// <param name="UpdatedUtc">UTC time of the latest persisted change.</param>
/// <param name="LiveCode">Immutable identity of the target process.</param>
/// <param name="AlvorSenseSessionId">Optional associated visual-session identifier.</param>
/// <param name="BaselineGraphRevision">Scope-graph revision captured when the workspace was created.</param>
/// <param name="NextEventId">Next preferred event sequence number.</param>
/// <param name="Interventions">Persistent live effects and their cleanup states.</param>
public sealed record LiveWorkspaceManifest(
    int SchemaVersion,
    string Id,
    string Purpose,
    LiveWorkspaceStatus Status,
    string RepositoryRoot,
    string WorkspacePath,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc,
    LiveWorkspaceTarget LiveCode,
    string? AlvorSenseSessionId,
    long BaselineGraphRevision,
    int NextEventId,
    LiveWorkspaceIntervention[] Interventions);
