namespace AlvorKit.Script.LiveCode;

/// <summary>Stable foreground status for one retained Source Update generation chain.</summary>
internal sealed record SourceUpdateCoordinatorResponse(
    bool Ok,
    string State,
    int Generation,
    string? OperationId = null,
    SourceUpdateApplyResult? Apply = null,
    string? EvidencePath = null,
    string? Error = null,
    bool WorktreeAhead = false);
