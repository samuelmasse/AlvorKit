namespace AlvorKit;

/// <summary>One local foreground request to the retained Source Update coordinator.</summary>
internal sealed record SourceUpdateCoordinatorRequest(
    string Operation,
    string? SourcePath = null,
    string? DiffPath = null,
    string? UpdateId = null);
