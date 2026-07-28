namespace AlvorKit.Script.LiveWorkspace;

/// <summary>One persistent live-process effect and the evidence required to clean it up.</summary>
/// <param name="Id">Stable identifier unique within the workspace.</param>
/// <param name="Kind">Runtime mechanism responsible for the effect.</param>
/// <param name="Description">Human-readable description of the retained effect.</param>
/// <param name="State">Current cleanup state.</param>
/// <param name="RuntimeId">Optional runtime identifier used to inspect or remove the effect.</param>
/// <param name="SourcePath">Optional workspace-relative source path that created the effect.</param>
/// <param name="Cleanup">Optional cleanup procedure for a human handoff.</param>
public sealed record LiveWorkspaceIntervention(
    string Id,
    LiveWorkspaceInterventionKind Kind,
    string Description,
    LiveWorkspaceInterventionState State,
    string? RuntimeId,
    string? SourcePath,
    string? Cleanup);
